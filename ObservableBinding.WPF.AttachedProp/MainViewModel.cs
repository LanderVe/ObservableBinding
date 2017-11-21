using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace ObservableBinding.WPF.AttachedProp
{
  class MainViewModel
  {
    public IObservable<string> StringStream { get; set; }
      = Observable.Interval(TimeSpan.FromSeconds(1)).Select(i => i.ToString());
    public Subject<string> StringSubject { get; set; } = new Subject<string>();

    public Subject<double> DoubleSubject { get; set; } = new Subject<double>();
    public IObservable<string> DoubleAsStringStream { get; set; }

    public Subject<bool> BoolSubject { get; set; } = new Subject<bool>();
    public IObservable<string> BoolAsStringStream { get; set; }


    public MainViewModel()
    {
      DoubleAsStringStream = DoubleSubject.Select(d => d.ToString());
      BoolAsStringStream = BoolSubject.Select(d => d.ToString());
    }

  }
}
