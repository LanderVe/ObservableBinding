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

namespace ObservableBinding.WPF
{
  [MarkupExtensionReturnType(typeof(object))]
  class OBindingExtension : MarkupExtension
  {
    private IDisposable listenSubscription;
    private IDisposable emitSubscription;
    private DependencyObject bindingTarget;
    private DependencyProperty bindingProperty;

    [ConstructorArgument("path")]
    public PropertyPath Path { get; set; }
    public BindingMode Mode { get; set; }

    public OBindingExtension() { }
    public OBindingExtension(PropertyPath path)
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

      return null;
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

      //set default BindingMode 
      if (Mode == BindingMode.Default) Mode = BindingMode.OneWay;

      //Bind to Observable and update property
      if (Mode == BindingMode.OneTime || Mode == BindingMode.OneWay || Mode == BindingMode.TwoWay)
      {
        SetupListenerBinding(source);
      }

      //send property values to Observer
      if (Mode == BindingMode.OneWayToSource || Mode == BindingMode.TwoWay)
      {
        SetupEmitBinding(source);
      }

    }

    private void RemoveBinding()
    {
      //stop listening to observable
      listenSubscription?.Dispose();
      emitSubscription?.Dispose();
    }

    #region Listen
    private void SetupListenerBinding(object source)
    {
      //get observable from path
      var observable = GetObjectFromPath(source, Path.Path);

      //add subscription
      MethodInfo method = typeof(OBindingExtension)
        .GetMethod(nameof(SubscribePropertyForObservable), BindingFlags.NonPublic | BindingFlags.Instance);
      MethodInfo generic = method.MakeGenericMethod(bindingProperty.PropertyType);
      generic.Invoke(this, new object[] { observable, bindingTarget, bindingProperty, Mode == BindingMode.OneTime });
    }

    private void SubscribePropertyForObservable<TProperty>(IObservable<TProperty> observable, DependencyObject d, DependencyProperty property, bool isOneTime)
    {
      if (observable != null)
      {
        if (isOneTime)
        {
          observable = observable.Take(1);
        }

        //efficient binding
        //var setPropertyAction = Utils.GetCompiledSetterLambda<TProperty>(d, property.Name); //val => d.Text = val
        //listenSubscription = observable.ObserveOn(SynchronizationContext.Current).Subscribe(setPropertyAction);
        listenSubscription = observable.ObserveOn(SynchronizationContext.Current).Subscribe(val => d.SetValue(property, val));
      }
    }
    #endregion

    #region Emit
    private void SetupEmitBinding(object source)
    {
      //Get IObserver from path
      var observer = GetObjectFromPath(source, Path.Path);

      //add subscription
      MethodInfo method = typeof(OBindingExtension)
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
    public FrameworkElement FindDataContextSource(DependencyObject d)
    {
      DependencyObject current = d;
      while (!(current is FrameworkElement) && current != null)
      {
        current = LogicalTreeHelper.GetParent(d);
      }

      return (FrameworkElement)current;
    }

    private object GetObjectFromPath(object dataContext, string path)
    {
      var properties = path.Split('.');
      var current = dataContext;

      foreach (var prop in properties)
      {
        current = current.GetType().GetProperty(prop).GetValue(current);
      }

      return current;
    }
    #endregion

  }
}
