using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.UI.Xaml;

namespace ObservableBinding.UWP.AttachedProp
{
  /// <summary>
  /// keeps observable subscriptions per control/property pair
  /// unsubscribes from the previous observable when a new value arrives
  /// </summary>
  internal class SubscriptionKeeper
  {
    private static Dictionary<DependencyObject, Dictionary<DependencyProperty, IDisposable>> subscriptions = new Dictionary<DependencyObject, Dictionary<DependencyProperty, IDisposable>>();

    public void UpdateSubscriptions(DependencyObject control, DependencyProperty prop, IDisposable newSub)
    {
      var controlDict = subscriptions.ContainsKey(control) ? subscriptions[control] : null;

      //find old subscription based on control and property name
      var oldSub = controlDict?.SingleOrDefault(kv => kv.Key == prop).Value;
      //remove old sub
      oldSub?.Dispose();

      //add newSub
      controlDict = controlDict ?? new Dictionary<DependencyProperty, IDisposable>();
      if (newSub != null)
      {
        controlDict[prop] = newSub;
      }
      else
      {
        controlDict.Remove(prop);
      }

      if (controlDict.Count > 0)
      {
        subscriptions[control] = controlDict;
      }
      else
      {
        subscriptions.Remove(control);
      }
    }
  }
}
