using Xamarin.Forms;
using XamApp.Models;
using XamApp.Views;

namespace XamApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = GetMainPage();
        }

        private Page GetMainPage()
        {
            var user = DataStore.Instance.GetUserAsync().Result;
            var page = user == null ? nameof(LoginPage) : nameof(RoomsPage);
            var appShell = new AppShell();
            appShell.GoToAsync("//" + page).Wait();
            return appShell;
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
