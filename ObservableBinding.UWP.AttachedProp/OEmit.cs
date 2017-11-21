using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace ObservableBinding.UWP.AttachedProp
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

    public static readonly DependencyProperty TextProperty =
    DependencyProperty.RegisterAttached("Text",
      typeof(IObserver<string>),
      typeof(OEmit),
      new PropertyMetadata(null, (d, e) => OnPropertyChanged<string>(d, e, TextBox.TextProperty)));

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

    public static readonly DependencyProperty ValueAsDoubleProperty =
        DependencyProperty.RegisterAttached("ValueAsDouble",
          typeof(IObserver<double>),
          typeof(OEmit),
          new PropertyMetadata(null, (d, e) => OnPropertyChanged<double>(d, e, RangeBase.ValueProperty)));

    #endregion

    private static void OnPropertyChanged<TProperty>(DependencyObject d, DependencyPropertyChangedEventArgs e, DependencyProperty propertyToMonitor)
    {
      var newObserver = e.NewValue as IObserver<TProperty>;
      //DependencyProperty propertyToMonitor = e.Property;
      IDisposable newSub = null;

      if (newObserver != null)
      {
        newSub = d.Observe<DependencyObject, TProperty>(propertyToMonitor)
                  .Subscribe(newObserver);
      }

      subscriptionKeeper.UpdateSubscriptions(d, propertyToMonitor, newSub);
    }



  }
}
