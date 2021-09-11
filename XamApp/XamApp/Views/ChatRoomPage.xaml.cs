﻿using Xamarin.Forms;
using XamApp.ViewModels;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System;
using System.IO;
using System.Net.Http;
using XamApp.Services;
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

        private async void button_cliked(object sender, System.EventArgs e)
        {
            ((Button)sender).IsEnabled = false;
            try
            {
                FileResult file = await FilePicker.PickAsync();

                if (file != null)
                {
                    Stream stream = await file.OpenReadAsync();
                    var streamContent = new StreamContent(stream);
                    var fileContent = new MultipartFormDataContent();
                    fileContent.Add(streamContent, "file", file.FileName);

                    var response = await HTTPClient.PostOrPut(true, null, "/api/File", fileContent);
                    if (response.IsSuccessStatusCode)
                    {
                        FileDTO fileDto = await HTTPClient.ReadAsAsync<FileDTO>(response);            
                        await vm.SendMessage(fileDto.Id, MessageTypeEnum.File);
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
