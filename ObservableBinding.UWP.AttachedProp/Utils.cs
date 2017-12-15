using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace ObservableBinding.UWP.AttachedProp
{
  public static class Utils
  {
    public static IObservable<TProperty> Observe<TProperty>(this DependencyObject component, DependencyProperty dependencyProperty)
    {
      return Observable.Create<TProperty>(observer => {

        var token = component.RegisterPropertyChangedCallback(dependencyProperty, (d, dp) => {
          observer.OnNext((TProperty)d.GetValue(dp));
        });

        return () => component.UnregisterPropertyChangedCallback(dependencyProperty, token);
      });
    }

  }
}
