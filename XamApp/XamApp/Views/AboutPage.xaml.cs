using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace XamApp.Views
{
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private async void LearnMoreClicked(object sender, EventArgs e)
        {
            await Browser.OpenAsync("https://aka.ms/xamarin-quickstart");
        }
    }
}
