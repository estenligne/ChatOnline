using Xamarin.Forms;
using XamApp.Models;
using XamApp.ViewModels;
using Xamarin.Forms.Xaml;

namespace XamApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RoomsPage : ContentPage
    {
        private readonly RoomsViewModel vm;

        public RoomsPage()
        {
            InitializeComponent();
            vm = new RoomsViewModel();
            BindingContext = vm;
        }

        protected async override void OnAppearing()
        {
            User user = await DataStore.Instance.GetUserAsync();
            if (user == null || user.DeviceUsedId <= 0)
            {
                vm.IsBusy = true;
                await AppShell.DoSignOut(this, user);
                vm.IsBusy = false;
            }
            else
            {
                base.OnAppearing();
                vm.OnAppearing();
                App.Appearing = vm;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            App.Appearing = null;
        }
    }
}
