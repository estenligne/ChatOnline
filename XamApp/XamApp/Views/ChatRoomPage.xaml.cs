using Xamarin.Forms;
using XamApp.ViewModels;
using XamApp.Services;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System;
using System.IO;
using System.Web;
using System.Diagnostics;
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

        private void OnSwipedRight(object sender, EventArgs e)
        {
            Element element = (Element)sender;
            Message message = (Message)element.BindingContext;
            vm.SetLinkedMessage(message);
        }

        private void OnCloseReplyView(object sender, EventArgs e)
        {
            vm.SetLinkedMessage(null);
        }

        private async void SendFile(object sender, EventArgs e)
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
                else Trace.TraceInformation("No file chosen");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "Ok");
            }
            ((Button)sender).IsEnabled = true;
        }

        private async void OnMessageDoubleTapped(object sender, EventArgs e)
        {
            Element element = (Element)sender;
            Message message = (Message)element.BindingContext;

            string action = await DisplayActionSheet(null, "Cancel", null, "Copy", "Reply", "Star", "Infos", "Delete", "Modify");

            if (action == "Copy")
            {
                await Clipboard.SetTextAsync(message.Body);
            }
            else if (action == "Delete")
            {
                DateTimeOffset now = DateTimeOffset.Now;

                string dateDeleted = HttpUtility.UrlEncode(now.ToString("O"));
                string args = $"/api/Message?messageSentId={message.Id}&dateDeleted={dateDeleted}";

                var response = await HTTPClient.DeleteAsync(null, args);
                if (response.IsSuccessStatusCode)
                {
                    message.Delete(now);
                }
                else await DisplayAlert("Error", await HTTPClient.GetResponseError(response), "Ok");
            }
            else if (!string.IsNullOrEmpty(action))
            {
                await DisplayAlert("Sorry", $"{action} not yet implemented!", "Ok");
            }
        }
    }
}
