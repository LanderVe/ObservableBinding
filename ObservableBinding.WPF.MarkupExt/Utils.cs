using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ObservableBinding.WPF.MarkupExt
{
  public static class Utils
  {
    //https://stackoverflow.com/questions/4764916/listen-to-changes-of-dependency-property, last comment
    public static IObservable<TProperty> Observe<TComponent, TProperty>(this TComponent component, DependencyProperty dependencyProperty)
    where TComponent : DependencyObject
    {
      return Observable.Create<TProperty>(observer =>
      {
        EventHandler update = (sender, args) => observer.OnNext((TProperty)((TComponent)sender).GetValue(dependencyProperty));
        var property = DependencyPropertyDescriptor.FromProperty(dependencyProperty, typeof(TComponent));
        property.AddValueChanged(component, update);
        return Disposable.Create(() => property.RemoveValueChanged(component, update));
      });
    }


  }
}
