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
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xam
{
  [ContentProperty("Path")]
  class BindExtension : IMarkupExtension
  {
    private IDisposable listenSubscription;
    private IDisposable emitSubscription;
    private BindableObject bindingTarget;
    private BindableProperty bindingProperty;

    public string Path { get; set; }
    public BindingMode Mode { get; set; }

    public BindExtension() { }

    public object ProvideValue(IServiceProvider serviceProvider)
    {

      if (serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget valueProvider)
      {
        bindingTarget = valueProvider.TargetObject as BindableObject;
        bindingProperty = valueProvider.TargetProperty as BindableProperty;

        bindingTarget.BindingContextChanged += BindingContextSource_BindingContextChanged;
        var bindingContext = bindingTarget?.BindingContext;

        SetupBinding(bindingContext);
      }

      return bindingProperty.DefaultValue;
    }

    private void BindingContextSource_BindingContextChanged(object sender, EventArgs e)
    {
      RemoveBinding();

      var newBindingContext = (bindingTarget?.BindingContext);
      if (newBindingContext != null)
      {
        SetupBinding(newBindingContext);
      }
    }

    private void SetupBinding(object source)
    {
      //sanity check
      if (source == null) return;

      //set default BindingMode 
      if (Mode == BindingMode.Default) Mode = bindingProperty.DefaultBindingMode;

      //Bind to Observable and update property
      if (Mode == BindingMode.OneWay || Mode == BindingMode.TwoWay)
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
      var observable = GetObjectFromPath(source, Path);

      //IObservable<T> --> typeof(T)
      var observableGenericType = observable.GetType().GetTypeInfo()
        .ImplementedInterfaces
        .Single(type => type.IsConstructedGenericType && type.GetGenericTypeDefinition() == (typeof(IObservable<>)))
        .GenericTypeArguments[0];

      //add subscription
      MethodInfo method = typeof(BindExtension).GetTypeInfo().DeclaredMethods
        .Where(mi => mi.Name == nameof(SubscribePropertyForObservable))
        .Single();

      MethodInfo generic = method.MakeGenericMethod(observableGenericType);
      generic.Invoke(this, new object[] { observable, bindingTarget, bindingProperty});
    }

    private void SubscribePropertyForObservable<TProperty>(IObservable<TProperty> observable, BindableObject d, BindableProperty property)
    {
      if (observable != null)
      {
        //automatic ToString
        if (property.ReturnType == typeof(string) && typeof(TProperty) != typeof(string))
        {
          listenSubscription = observable.Select(val => val.ToString()).ObserveOn(SynchronizationContext.Current).Subscribe(val => d.SetValue(property, val));
        }
        //any other case
        else
        {
          listenSubscription = observable.ObserveOn(SynchronizationContext.Current).Subscribe(val => d.SetValue(property, val));
        }

      }
    }
    #endregion

    #region Emit
    private void SetupEmitBinding(object source)
    {
      //Get IObserver from path
      var observer = GetObjectFromPath(source, Path);

      //add subscription
      MethodInfo method = typeof(BindExtension).GetTypeInfo().DeclaredMethods
        .Where(mi => mi.Name == nameof(SubScribeObserverForProperty))
        .Single();
      MethodInfo generic = method.MakeGenericMethod(bindingProperty.ReturnType);
      generic.Invoke(this, new object[] { observer, bindingTarget, bindingProperty });
    }

    private void SubScribeObserverForProperty<TProperty>(IObserver<TProperty> observer, BindableObject d, BindableProperty propertyToMonitor)
    {
      if (propertyToMonitor.DeclaringType.GetTypeInfo().IsAssignableFrom(d.GetType().GetTypeInfo()) && observer != null)
      {
        emitSubscription = d.Observe<TProperty>(propertyToMonitor)
                  .Subscribe(observer);
      }
    }
    #endregion

    #region Helper

    private object GetObjectFromPath(object bindingContext, string path)
    {
      var properties = path.Split('.');
      var current = bindingContext;

      foreach (var prop in properties)
      {
        current = current.GetType().GetRuntimeProperty(prop).GetValue(current);
      }

      return current;
    }

    #endregion

  }
}
