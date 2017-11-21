using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Xam
{
  public static class Utils
  {
    public static IObservable<TProperty> Observe<TProperty>(this BindableObject bindableObject, BindableProperty bindableProperty)
    {
      var propertyName = bindableProperty.PropertyName;

      return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                      handler => handler.Invoke,
                      h => bindableObject.PropertyChanged += h,
                      h => bindableObject.PropertyChanged -= h)
                  .Where(e => e.EventArgs.PropertyName == propertyName)
                  .Select(e => (TProperty)bindableObject.GetValue(bindableProperty));
    }


  }
}
