using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ObservableBinding.WPF.AttachedProp
{
  /// <summary>
  /// keeps observable subscriptions per control/property pair
  /// unsubscribes from the previous observable when a new value arrives
  /// </summary>
  internal class SubscriptionKeeper
  {
    private static Dictionary<DependencyObject, Dictionary<string, IDisposable>> subscriptions = new Dictionary<DependencyObject, Dictionary<string, IDisposable>>();

    public void UpdateSubscriptions(DependencyObject control, string propName, IDisposable newSub)
    {
      var controlDict = subscriptions.ContainsKey(control) ? subscriptions[control] : null;

      //find old subscription based on control and property name
      var oldSub = controlDict?.SingleOrDefault(kv => kv.Key == propName).Value;
      //remove old sub
      oldSub?.Dispose();

      //add newSub
      controlDict = controlDict ?? new Dictionary<string, IDisposable>();
      if (newSub != null)
      {
        controlDict[propName] = newSub;
      }
      else
      {
        controlDict.Remove(propName);
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
