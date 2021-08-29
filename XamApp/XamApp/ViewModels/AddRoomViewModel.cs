using Xamarin.Forms;

namespace XamApp.ViewModels
{
    public class AddRoomViewModel : BaseViewModel
    {
        private string email;
        private string phoneNumber;
        private string groupName;
        private string joinToken;

        public string Email
        {
            get { return email; }
            set
            {
                if (SetProperty(ref email, value))
                {
                    OnUserEntry(Type.Email, value);
                    OnPropertyChanged(nameof(EmailColor));
                }
            }
        }

        public string PhoneNumber
        {
            get { return phoneNumber; }
            set
            {
                if (SetProperty(ref phoneNumber, value))
                {
                    OnUserEntry(Type.PhoneNumber, value);
                    OnPropertyChanged(nameof(PhoneNumberColor));
                }
            }
        }

        public string GroupName
        {
            get { return groupName; }
            set
            {
                if (SetProperty(ref groupName, value))
                {
                    OnUserEntry(Type.ProfileName, value);
                    OnPropertyChanged(nameof(GroupNameColor));
                }
            }
        }

        public string JoinToken
        {
            get { return joinToken; }
            set
            {
                if (SetProperty(ref joinToken, value))
                {
                    OnUserEntry(Type.JoinToken, value);
                    OnPropertyChanged(nameof(JoinTokenColor));
                }
            }
        }

        public Color EmailColor => IsValid(Type.Email, Email) ? Color.Green : Color.Black;

        public Color PhoneNumberColor => IsValid(Type.PhoneNumber, PhoneNumber) ? Color.Green : Color.Black;

        public Color GroupNameColor => IsValid(Type.ProfileName, GroupName) ? Color.Green : Color.Black;

        public Color JoinTokenColor => IsValid(Type.JoinToken, JoinToken) ? Color.Green : Color.Black;

        public bool CanAdd => !IsBusy && (
            IsValid(Type.Email, Email) != // using XOR
            IsValid(Type.PhoneNumber, PhoneNumber) !=
            IsValid(Type.ProfileName, GroupName) !=
            IsValid(Type.JoinToken, JoinToken));

        public void UpdateButton()
        {
            OnPropertyChanged(nameof(CanAdd));
        }

        private void OnUserEntry(Type type, string value)
        {
            if (value != null)
            {
                if (type != Type.Email) Email = null;
                if (type != Type.PhoneNumber) PhoneNumber = null;
                if (type != Type.ProfileName) GroupName = null;
                if (type != Type.JoinToken) JoinToken = null;
            }
            UpdateButton();
        }
    }
}
