using Xamarin.Forms;
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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            vm.OnAppearing();
        }
    }
}
