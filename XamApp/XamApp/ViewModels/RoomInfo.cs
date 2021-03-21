using System;
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

        public string DateSent
        {
            get
            {
                if (LatestMessage.Id == 0)
                    return null;
                var dateUtc = DateTime.SpecifyKind(LatestMessage.DateSent, DateTimeKind.Utc);
                var dateLocal = dateUtc.ToLocalTime();
                if (dateUtc.Date == DateTime.UtcNow.Date)
                    return dateLocal.ToString("H:mm");
                else return dateLocal.ToString("yyyy-MM-dd");
            }
        }
    }
}
