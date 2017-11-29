using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Xaml;

namespace ObservableBinding.WPF.MarkupExt
{
  [MarkupExtensionReturnType(typeof(object))]
  class BindExtension : MarkupExtension
  {
    private IDisposable listenSubscription;
    private IDisposable emitSubscription;
    private DependencyObject bindingTarget;
    private DependencyProperty bindingProperty;

    [ConstructorArgument("path")]
    public PropertyPath Path { get; set; }
    public BindingMode Mode { get; set; }

    public BindExtension() { }
    public BindExtension(PropertyPath path)
      : this()
    {
      Path = path;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
      var valueProvider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

      if (valueProvider != null)
      {
        bindingTarget = valueProvider.TargetObject as DependencyObject;
        bindingProperty = valueProvider.TargetProperty as DependencyProperty;

        //DataContext, TODO other sources
        var dataContextSource = FindDataContextSource(bindingTarget);
        dataContextSource.DataContextChanged += DataContextSource_DataContextChanged;
        var dataContext = dataContextSource?.DataContext;

        SetupBinding(dataContext);
      }

      //return default
      return bindingProperty.DefaultMetadata.DefaultValue;
    }

    private void DataContextSource_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      RemoveBinding();
      if (e.NewValue != null)
      {
        SetupBinding(e.NewValue);
      }
    }

    private void SetupBinding(object source)
    {
      //sanity check
      if (source == null) return;

      var boundObject = GetObjectFromPath(source, Path.Path);
      if (boundObject == null) return;

      //set default BindingMode 
      if (Mode == BindingMode.Default)
      {
        if (bindingProperty.GetMetadata(bindingTarget) is FrameworkPropertyMetadata metadata
            && metadata.BindsTwoWayByDefault)
        {
          Mode = BindingMode.TwoWay;
        }
        else
        {
          Mode = BindingMode.OneWay;
        }
      }

      //bind to Observable and update property
      if (Mode == BindingMode.OneTime || Mode == BindingMode.OneWay || Mode == BindingMode.TwoWay)
      {
        SetupListenerBinding(boundObject);
      }

      //send property values to Observer
      if (Mode == BindingMode.OneWayToSource || Mode == BindingMode.TwoWay)
      {
        SetupEmitBinding(boundObject);
      }

    }

    private void RemoveBinding()
    {
      //stop listening to observable
      listenSubscription?.Dispose();
      emitSubscription?.Dispose();
    }

    #region Listen
    private void SetupListenerBinding(object observable)
    {
      //IObservable<T> --> typeof(T)
      var observableGenericType = observable.GetType()
        .GetInterfaces()
        .Single(type => type.IsGenericType && type.GetGenericTypeDefinition() == (typeof(IObservable<>)))
        .GetGenericArguments()[0];

      //add subscription
      MethodInfo method = typeof(BindExtension)
        .GetMethod(nameof(SubscribePropertyForObservable), BindingFlags.NonPublic | BindingFlags.Instance);
      MethodInfo generic = method.MakeGenericMethod(observableGenericType);
      generic.Invoke(this, new object[] { observable, bindingTarget, bindingProperty });
    }

    private void SubscribePropertyForObservable<TProperty>(IObservable<TProperty> observable, DependencyObject d, DependencyProperty property)
    {
      if (observable == null) return;

      if (Mode == BindingMode.OneTime)
      {
        observable = observable.Take(1);
      }

      //automatic ToString
      if (property.PropertyType == typeof(string) && typeof(TProperty) != typeof(string))
      {
        listenSubscription = observable.Select(val => val.ToString()).ObserveOn(SynchronizationContext.Current).Subscribe(val => d.SetValue(property, val));
      }
      //any other case
      else
      {
        listenSubscription = observable.ObserveOn(SynchronizationContext.Current).Subscribe(val => d.SetValue(property, val));
      }
    }
    #endregion

    #region Emit
    private void SetupEmitBinding(object observer)
    {
      //add subscription
      MethodInfo method = typeof(BindExtension)
        .GetMethod(nameof(SubScribeObserverForProperty), BindingFlags.NonPublic | BindingFlags.Instance);
      MethodInfo generic = method.MakeGenericMethod(bindingProperty.PropertyType);
      generic.Invoke(this, new object[] { observer, bindingTarget, bindingProperty });
    }

    private void SubScribeObserverForProperty<TProperty>(IObserver<TProperty> observer, DependencyObject d, DependencyProperty propertyToMonitor)
    {
      if (propertyToMonitor.OwnerType.IsAssignableFrom(d.GetType()) && observer != null)
      {
        emitSubscription = d.Observe<DependencyObject, TProperty>(propertyToMonitor)
                  .Subscribe(observer);
      }
    }
    #endregion

    #region Helper

    /// <summary>
    /// Not all DependencyObjects have a DataContext Property
    /// e.g. RotateTransform
    /// If the Binding happens there, this method will travel up the logical tree to retrieve the first parent that does have it
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public FrameworkElement FindDataContextSource(DependencyObject d)
    {
      DependencyObject current = d;
      while (!(current is FrameworkElement) && current != null)
      {
        current = LogicalTreeHelper.GetParent(d);
      }

      return (FrameworkElement)current;
    }

    /// <summary>
    /// If the Binding looks something like this
    /// {o:Bind Parent.Child.SubChild}, we need to retrieve SubChild
    /// </summary>
    /// <param name="dataContext"></param>
    /// <param name="path"></param>
    /// <returns>The value returned by the path, null if any properties in the chain is null</returns>
    private object GetObjectFromPath(object dataContext, string path)
    {
      var properties = path.Split('.');
      var current = dataContext;

      foreach (var prop in properties)
      {
        if (current == null) break;
        current = current.GetType().GetProperty(prop).GetValue(current);
      }

      return current;
    }
    #endregion

  }
}
