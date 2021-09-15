using Xamarin.Forms;
using XamApp.ViewModels;
using XamApp.Services;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System;
using System.IO;
using Global.Models;
using Global.Enums;

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
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            vm.OnDisappearing();
        }

        private async void SendFile(object sender, System.EventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            try
            {
                FileResult file = await FilePicker.PickAsync();
                if (file != null)
                {
                    Stream stream = await file.OpenReadAsync();
                    var response = await HTTPClient.PostFile(null, "file", file.FileName, stream);
                    if (response.IsSuccessStatusCode)
                    {
                        FileDTO fileDto = await HTTPClient.ReadAsAsync<FileDTO>(response);            
                        await vm.SendMessage(fileDto);
                    }
                    else await DisplayAlert("Error", await HTTPClient.GetResponseError(response), "Ok");
                }
                else System.Diagnostics.Trace.TraceInformation("No file chosen");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }
            ((Button)sender).IsEnabled = true;
        }
    }
}
