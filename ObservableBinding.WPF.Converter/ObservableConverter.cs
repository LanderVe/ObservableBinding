using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace ObservableBinding.WPF.Converter
{
  class ObservableConverter : DependencyObject, IValueConverter
  {
    public DependencyObject BindingTarget
    {
      get { return (DependencyObject)GetValue(BindingTargetProperty); }
      set { SetValue(BindingTargetProperty, value); }
    }

    public static readonly DependencyProperty BindingTargetProperty =
        DependencyProperty.Register("BindingTarget", typeof(DependencyObject), typeof(ObservableConverter), new PropertyMetadata(null));



    public DependencyProperty BindingProperty { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      MethodInfo method = typeof(ObservableConverter)
        .GetMethod(nameof(RegisterObservable), BindingFlags.NonPublic | BindingFlags.Instance);
      MethodInfo generic = method.MakeGenericMethod(targetType);
      generic.Invoke(this, new object[] { value, BindingTarget, BindingProperty });

      return DependencyProperty.UnsetValue;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new NotImplementedException();
    }

    public void RegisterObservable<TTargetType>(IObservable<TTargetType> observable, DependencyObject d, DependencyProperty dp)
    {
      observable.ObserveOn(SynchronizationContext.Current).Subscribe(val => d.SetValue(dp, val));
    }

  }
}
