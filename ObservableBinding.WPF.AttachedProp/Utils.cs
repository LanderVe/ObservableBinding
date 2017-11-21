using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObservableBinding.WPF.AttachedProp
{
  public static class Utils
  {
    /// <summary>
    /// produces a labmda like this: (TValue val) => target.Property = val
    /// where target is a captured value
    /// Usefull if you want to set a property of an object without knowing the details about its type
    /// especially when setting it mutiple times, performance is better than reflection
    /// </summary>
    /// <typeparam name="TValue">indicates the type of the property you wan to set</typeparam>
    /// <param name="target">specifies the object you want to the property of</param>
    /// <param name="propName">indicates the name of the property you want to set</param>
    /// <returns>the labmda</returns>
    public static Action<TValue> GetCompiledSetterLambda<TValue>(object target, string propName)
    {
      var targetExp = Expression.Parameter(target.GetType(), "t"); // use GetType(), you want the specific type
      var valueExp = Expression.Parameter(typeof(TValue), "v");

      var pi = target.GetType().GetProperty(propName);
      MemberExpression propExp = Expression.Property(targetExp, pi);

      var factoryExpr = Expression.Lambda(
                  Expression.Lambda<Action<TValue>>(
                    Expression.Assign(propExp, valueExp),
                valueExp),
              targetExp);


      var factory = factoryExpr.Compile(); //(object target) => (TValue val) => target.Text = val
      var lamb = (Action<TValue>)factory.DynamicInvoke(target); //(TValue val) => target.Text = val

      return lamb;
    }

    //https://stackoverflow.com/questions/4764916/listen-to-changes-of-dependency-property, last comment
    public static IObservable<TProperty> Observe<TComponent, TProperty>(this TComponent component, System.Windows.DependencyProperty dependencyProperty)
    where TComponent : System.Windows.DependencyObject
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
