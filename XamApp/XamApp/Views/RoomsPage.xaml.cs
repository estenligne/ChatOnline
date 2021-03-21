using Xamarin.Forms;
using XamApp.ViewModels;

namespace XamApp.Views
{
    public partial class RoomsPage : ContentPage
    {
        public RoomsPage()
        {
            InitializeComponent();
            BindingContext = new RoomsViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ((RoomsViewModel)BindingContext).OnAppearing();
        }
    }
}
