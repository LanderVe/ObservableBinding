using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ObservableBinding.WPF
{
  public class ObservableText : Behavior<TextBlock>
  {
    public IObservable<string> Stream
    {
      get { return (IObservable<string>)GetValue(StreamProperty); }
      set { SetValue(StreamProperty, value); }
    }

    // Using a DependencyProperty as the backing store for Stream.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty StreamProperty =
        DependencyProperty.Register("Stream", typeof(IObservable<string>), typeof(ObservableText), new PropertyMetadata(null, StreamChanged));

    private static void StreamChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var self = ((ObservableText)d);
      self.OnDetaching();
      self.OnAttached();
    }

    private IDisposable sub;

    protected override void OnAttached()
    {
      sub = Stream?.ObserveOn(SynchronizationContext.Current).Subscribe(val => AssociatedObject.Text = val);
    }

    protected override void OnDetaching()
    {
      sub?.Dispose();
    }
  }
}
