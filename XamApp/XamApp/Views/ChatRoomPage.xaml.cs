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
    [QueryProperty(nameof(ChatRoomId), "id")]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChatRoomPage : ContentPage
    {
        private readonly ChatRoomViewModel vm;
        public long ChatRoomId { get; set; }

        public ChatRoomPage()
        {
            InitializeComponent();
            vm = new ChatRoomViewModel();
            BindingContext = vm;
            vm.MessagesView = MessagesView;
        }

        protected override async void OnAppearing()
        {
            if (ChatRoomId != 0)
            {
                var response = await HTTPClient.GetAsync(null, "/api/ChatRoom/GetInfo?id=" + ChatRoomId);
                if (response.IsSuccessStatusCode)
                {
                    vm.Room = await HTTPClient.ReadAsAsync<RoomInfo>(response);
                }
                else
                {
                    string message = await HTTPClient.GetResponseError(response);
                    await DisplayAlert("Failed to Load", message, "Ok");
                }
                ChatRoomId = 0;
            }
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
            MessageEditor.Focus();
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

            string action = await DisplayActionSheet(null, null, null, "Copy", "Reply", "Star", "Infos", "Delete", "Modify");

            if (action == "Copy")
            {
                await Clipboard.SetTextAsync(message.Body);
            }
            else if (action == "Reply")
            {
                OnSwipedRight(sender, e);
            }
            else if (action == "Star")
            {
                DateTimeOffset now = DateTimeOffset.Now;

                string dateStarred = HttpUtility.UrlEncode(now.ToString("O"));

                string args = $"/api/Message/Starred?messageSentId={message.Id}&dateStarred={dateStarred}";

                var response = await HTTPClient.PatchAsync(null, args, (string)null);
                if (response.IsSuccessStatusCode)
                {
                    message.Starred(now);
                }
                else await DisplayAlert("Error", await HTTPClient.GetResponseError(response), "Ok");
            }
            else if (action == "Delete")
            {
                DateTimeOffset now = DateTimeOffset.Now;

                string dateDeleted = HttpUtility.UrlEncode(now.ToString("O"));
                string args = $"/api/Message?messageSentId={message.Id}&dateDeleted={dateDeleted}";

                var response = await HTTPClient.DeleteAsync(null, args);
                if (response.IsSuccessStatusCode)
                {
                    message.Deleted(now);
                }
                else await DisplayAlert("Error", await HTTPClient.GetResponseError(response), "Ok");
            }
            else if (!string.IsNullOrEmpty(action))
            {
                await DisplayAlert("Sorry", $"{action} not yet implemented!", "Ok");
            }
        }

        private void OnLinkedMessageTapped(object sender, EventArgs e)
        {
            Element element = (Element)sender;
            Message message = (Message)element.BindingContext;
            vm.ScrollToMessage(message.LinkedId);
        }
    }
}
