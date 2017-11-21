using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ObservableBinding.WPF.AttachedProp
{
  public static class OEmit
  {
    private static SubscriptionKeeper subscriptionKeeper = new SubscriptionKeeper();

    #region Text
    public static IObservable<string> GetText(DependencyObject obj)
    {
      return (IObservable<string>)obj.GetValue(TextProperty);
    }

    public static void SetText(DependencyObject obj, IObservable<string> value)
    {
      obj.SetValue(TextProperty, value);
    }

    // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.RegisterAttached("Text", typeof(IObserver<string>), typeof(OEmit), new PropertyMetadata(null, OnTextChanged));

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var propName = "Text";
      var newObserver = e.NewValue as IObserver<string>;
      IDisposable newSub = null;

      if (d is TextBox textBox && newObserver != null)
      {
        newSub = Observable.FromEventPattern<TextChangedEventArgs>(textBox, nameof(textBox.TextChanged))
          .Select(patt => ((TextBox)patt.Sender).Text)
          .Subscribe(newObserver);
      }

      //if (d is TextBox textBox && newObserver != null)
      //{
      //  newSub = d.Observe(TextBox.TextProperty)
      //            .Select(ea => (string)d.GetValue(TextBox.TextProperty))
      //            .Subscribe(newObserver);
      //}

      subscriptionKeeper.UpdateSubscriptions(d, propName, newSub);
    }

    #endregion

    #region ValueAsDouble
    public static IObservable<double> GetValueAsDouble(DependencyObject obj)
    {
      return (IObservable<double>)obj.GetValue(ValueAsDoubleProperty);
    }

    public static void SetValueAsDouble(DependencyObject obj, IObservable<double> value)
    {
      obj.SetValue(TextProperty, value);
    }

    // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ValueAsDoubleProperty =
        DependencyProperty.RegisterAttached("ValueAsDouble", typeof(IObserver<double>), typeof(OEmit), new PropertyMetadata(null, OnValueAsDoubleChanged));

    private static void OnValueAsDoubleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var propName = "ValueAsDouble";
      var newObserver = e.NewValue as IObserver<double>;
      IDisposable newSub = null;

      if (d is RangeBase ranger && newObserver != null)
      {
        newSub = Observable.FromEventPattern<RoutedPropertyChangedEventArgs<double>>(ranger, nameof(ranger.ValueChanged))
          .Select(patt => ((RangeBase)patt.Sender).Value)
          .Subscribe(newObserver);
      }

      subscriptionKeeper.UpdateSubscriptions(d, propName, newSub);
    }

    #endregion

    #region IsFocused
    public static IObservable<bool> GetIsFocused(DependencyObject obj)
      => (IObservable<bool>)obj.GetValue(IsFocusedProperty);

    public static void SetIsFocused(DependencyObject obj, IObservable<bool> value)
      => obj.SetValue(TextProperty, value);

    // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IsFocusedProperty =
        DependencyProperty.RegisterAttached("IsFocused",
          typeof(IObserver<bool>),
          typeof(OEmit),
          new PropertyMetadata(null, (d, e) => OnPropertyChanged<bool>(UIElement.IsFocusedProperty, d, e)));
    #endregion

    private static void OnPropertyChanged<TProperty>(DependencyProperty propertyToMonitor, DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var newObserver = e.NewValue as IObserver<TProperty>;
      IDisposable newSub = null;

      if (propertyToMonitor.OwnerType.IsAssignableFrom(d.GetType()) && newObserver != null)
      {
        newSub = d.Observe<DependencyObject, TProperty>(propertyToMonitor)
                  .Subscribe(newObserver);
      }

      subscriptionKeeper.UpdateSubscriptions(d, e.Property.Name, newSub);
    }


  }
}
