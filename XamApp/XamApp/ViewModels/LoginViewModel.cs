using Xamarin.Forms;

namespace XamApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string email;
        private string password;
        private string passwordConfirm;
        private string phoneNumber;
        private string profileName;

        public string Email
        {
            get { return email; }
            set
            {
                if (SetProperty(ref email, value))
                {
                    UpdateButtons();
                    OnPropertyChanged(nameof(EmailColor));
                }
            }
        }

        public string Password
        {
            get { return password; }
            set
            {
                if (SetProperty(ref password, value))
                {
                    PasswordConfirm = null;
                    UpdateButtons();
                    OnPropertyChanged(nameof(PasswordColor));
                }
            }
        }

        public string PasswordConfirm
        {
            get { return passwordConfirm; }
            set
            {
                if (SetProperty(ref passwordConfirm, value))
                {
                    UpdateButtons();
                    OnPropertyChanged(nameof(PasswordConfirmColor));
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
                    UpdateButtons();
                    OnPropertyChanged(nameof(PhoneNumberColor));
                }
            }
        }

        public string ProfileName
        {
            get { return profileName; }
            set
            {
                if (SetProperty(ref profileName, value))
                {
                    UpdateButtons();
                    OnPropertyChanged(nameof(ProfileNameColor));
                }
            }
        }

        public void UpdateButtons()
        {
            OnPropertyChanged(nameof(CanLogin));
            OnPropertyChanged(nameof(CanChangePassword));
            OnPropertyChanged(nameof(CanRegister));
        }

        public bool CanLogin => !IsBusy && IsValid(Type.Email, Email) && IsValid(Type.Password, Password);

        public bool CanChangePassword => CanLogin && Password == PasswordConfirm;

        public bool CanRegister => CanChangePassword && IsValid(Type.PhoneNumber, PhoneNumber) && IsValid(Type.ProfileName, ProfileName);

        public Color EmailColor => IsValid(Type.Email, Email) ? Color.Green : Color.Black;

        public Color PasswordColor => IsValid(Type.Password, Password) ? Color.Green : Color.Black;

        public Color PasswordConfirmColor => Password == PasswordConfirm ? Color.Green : Color.Black;

        public Color PhoneNumberColor => IsValid(Type.PhoneNumber, PhoneNumber) ? Color.Green : Color.Black;

        public Color ProfileNameColor => IsValid(Type.ProfileName, ProfileName) ? Color.Green : Color.Black;
    }
}
