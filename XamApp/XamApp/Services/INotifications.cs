﻿using Global.Models;

namespace XamApp.Services
{
    public interface INotifications
    {
        void RegisterFcmToken(long deviceUsedId);

        void Notify(PushNotificationDTO notification);

        void Toast(string message, bool takeLong);
    }
}
