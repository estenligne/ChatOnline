﻿using System;
using Global.Models;
using Xamarin.Forms;

namespace XamApp.ViewModels
{
    public class RoomInfo : ChatRoomInfo
    {
        private ImageSource profilePhoto;

        public ImageSource ProfilePhoto
        {
            get
            {
                if (profilePhoto == null)
                {
                    profilePhoto = ImageSource.FromFile("icon_about.png");
                }
                return profilePhoto;
            }
        }

        public string ShortBody
        {
            get
            {
                if (LatestMessage.Id == 0)
                    return "<i>(no message)</i>";
                else return LatestMessage.ShortBody;
            }
        }

        private static string GetMsgReadTick(MessageShortInfo message)
        {
            if (message.Id == 0)
                return null;
            const string tick = "&#10003;";
            string html = tick;
            if (message.NotReceivedCount == 0)
                html += tick;
            if (message.NotReadCount == 0)
                html += tick;
            return $"<span>{html}</span>";
        }

        private static string GetDateSent(MessageShortInfo message)
        {
            if (message.Id == 0)
                return null;
            var dateLocal = message.DateSent.ToLocalTime();
            string time = dateLocal.ToString("HH:mm");
            var diff = DateTimeOffset.Now - dateLocal;
            if (diff >= TimeSpan.FromDays(1))
                time = dateLocal.ToString("yyyy-MM-dd ") + time;
            return time;
        }

        public string MsgReadTick => GetMsgReadTick(LatestMessage);

        public string DateSent => GetDateSent(LatestMessage);
    }
}
