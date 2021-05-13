using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace XamApp.Views
{
    public partial class AboutPage : ContentPage
    {
        private const string url = "https://github.com/estenligne/ChatOnline";

        public AboutPage()
        {
            InitializeComponent();
            BindingContext = url;
        }

        private async void LearnMoreClicked(object sender, EventArgs e)
        {
            await Browser.OpenAsync(url);
        }
    }
}
