using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Xam
{
  public partial class MainPage : ContentPage
  {
    public MainPage()
    {
      BindingContext = new MainViewModel();
      InitializeComponent();
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
      sl.Children.Clear();
    }
  }
}
