using Xamarin.Forms;
using XamApp.ViewModels;
using Xamarin.Forms.Xaml;

namespace XamApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatRoomPage : ContentPage
    {
        private readonly ChatRoomViewModel vm;

        public ChatRoomPage()
        {
            InitializeComponent();
            vm = new ChatRoomViewModel();
            BindingContext = vm;
            vm.MessagesView = MessagesView;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await vm.OnAppearing();
            vm.ScrollTo(-1);
        }
    }
}
