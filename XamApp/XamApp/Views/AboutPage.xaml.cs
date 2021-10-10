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
            BindingContext = $"Learn more about the open-source project at <strong>{url}</strong>";
        }

        private async void LearnMoreClicked(object sender, EventArgs e)
        {
            await Browser.OpenAsync(url);
        }
    }
}
