using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ObservableBinding.WPF.AttachedProp
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
          new PropertyMetadata(null, (d,e) => SubscribePropertyForObservable<string>(d,e)));

    /* FIRST VERSION */
    //private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //{
      //IDisposable newSub = SubscribeToText();

      //UpdateSubscriptions(d, propName, newSub);

      ////inner helper function

      //IDisposable SubscribeToText()
      //{
      //  if (newStream == null) return null;

      //  //efficient binding
      //  if (d is TextBox textBox)
      //  {
      //    return newStream.ObserveOn(SynchronizationContext.Current).Subscribe(val => textBox.Text = val);
      //  }
      //  if (d is TextBlock textBlock)
      //  {
      //    return newStream.ObserveOn(SynchronizationContext.Current).Subscribe(val => textBlock.Text = val);
      //  }
      //  //generic binding
      //  else
      //  {
      //    dynamic control = d;
      //    return newStream.ObserveOn(SynchronizationContext.Current).Subscribe(val => control.Text = val);
      //  }

      //}

    //}
    /* SECOND VERSION */
    //private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //{
    //  var newStream = e.NewValue as IObservable<string>;

    //  //map to strings if they are not strings
    //  if (newStream == null)
    //  {
    //    var objStream = e.NewValue as IObservable<object>;
    //    newStream = objStream?.Select(o => o.ToString());
    //  }

    //  SubscribePropertyForObservable(d, e.Property.Name, newStream);
    //}
    #endregion

    /*SECOND VERSION*/
    //private static void SubscribePropertyForObservable<TValue>(DependencyObject control, string propName, IObservable<TValue> newStream)
    //{
    //  IDisposable newSub = null;

    //  if (newStream != null)
    //  {
    //    //efficient binding
    //    var setPropertyAction = Utils.GetCompiledSetterLambda<TValue>(control, propName); //val => d.Text = val
    //    newSub = newStream.ObserveOn(SynchronizationContext.Current).Subscribe(setPropertyAction);
    //  }

    //  subscriptionKeeper.UpdateSubscriptions(control, propName, newSub);
    //}

    /* THIRD VERSION */
    private static void SubscribePropertyForObservable<TProperty>(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var newStream = e.NewValue as IObservable<TProperty>;
      var propName = e.Property.Name;
      IDisposable newSub = null;

      if (newStream != null)
      {
        //efficient binding
        var setPropertyAction = Utils.GetCompiledSetterLambda<TProperty>(d, propName); //val => d.Text = val
        newSub = newStream.ObserveOn(SynchronizationContext.Current).Subscribe(setPropertyAction);
      }

      subscriptionKeeper.UpdateSubscriptions(d, propName, newSub);
    }


  }
}
