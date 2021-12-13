using Global.Models;
using System.Threading.Tasks;

namespace XamApp.Services
{
    /// <summary>
    /// Interface for responding to push notification
    /// </summary>
    interface IProcessNotification
    {
        /// <returns>True if push notification processed else false</returns>
        bool ProcessPushNotification(PushNotificationDTO notification);
    }
}
