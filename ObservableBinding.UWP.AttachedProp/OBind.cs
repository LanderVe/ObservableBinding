using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace ObservableBinding.UWP.AttachedProp
{
  public static class OBind
  {
    private static SubscriptionKeeper subscriptionKeeper = new SubscriptionKeeper();

    #region Text
    public static IObservable<object> GetText(DependencyObject obj)
    {
      return (IObservable<object>)obj.GetValue(TextProperty);
    }

    public static void SetText(DependencyObject obj, IObservable<object> value)
    {
      obj.SetValue(TextProperty, value);
    }

    // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.RegisterAttached(
          "Text",
          typeof(IObservable<object>),
          typeof(OBind),
          new PropertyMetadata(null, (d, e) => SubscribeTextProperty(d, e)));

    private static void SubscribeTextProperty(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (d is TextBlock)
      {
        SubscribePropertyForObservable<string>(d, e, TextBlock.TextProperty);
      }
      else if (d is TextBox textBox)
      {
        SubscribePropertyForObservable<string>(d, e, TextBox.TextProperty);
      }
    }
    #endregion

    #region ValueAsDouble
    public static IObservable<double> GetValueAsDouble(DependencyObject obj)
    {
      return (IObservable<double>)obj.GetValue(ValueAsDoubleProperty);
    }

    public static void SetValueAsDouble(DependencyObject obj, IObservable<double> value)
    {
      obj.SetValue(ValueAsDoubleProperty, value);
    }

    // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty ValueAsDoubleProperty =
        DependencyProperty.RegisterAttached(
          "ValueAsDouble",
          typeof(IObservable<double>),
          typeof(OBind),
          new PropertyMetadata(0, (d, e) => SubscribePropertyForObservable<double>(d, e, RangeBase.ValueProperty)));
    #endregion

    private static void SubscribePropertyForObservable<TProperty>(DependencyObject d, DependencyPropertyChangedEventArgs e, DependencyProperty propertyToMonitor)
    {
      var newStream = e.NewValue as IObservable<TProperty>;
      IDisposable newSub = null;

      //destroy when unloaded from main tree
      ((FrameworkElement)d).Unloaded += (s,ea) => subscriptionKeeper.UpdateSubscriptions(d, propertyToMonitor, null);

      if (newStream != null)
      {
        newSub = newStream.ObserveOn(SynchronizationContext.Current)
          .Subscribe(val => d.SetValue(propertyToMonitor, val));
      }

      subscriptionKeeper.UpdateSubscriptions(d, propertyToMonitor, newSub);
    }


  }
}
