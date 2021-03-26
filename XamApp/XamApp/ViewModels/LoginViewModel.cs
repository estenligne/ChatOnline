using System.Text.RegularExpressions;
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

        private enum Type
        {
            Email,
            Password,
            PhoneNumber,
            ProfileName,
        }

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
                    UpdateButtons();
                    PasswordConfirm = null;
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

        private static bool IsValid(Type Type, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            if (value[0] == ' ' || value[value.Length - 1] == ' ')
                return false;

            string regexPattern;
            switch (Type)
            {
                case Type.Email:
                    regexPattern =
                        @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" + "@" +
                        @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";

                    return Regex.IsMatch(value, regexPattern);

                case Type.Password:
                    return value.Length >= 6;

                case Type.ProfileName:
                    return value.Length >= 5;

                case Type.PhoneNumber:
                    regexPattern =
                        @"^\+(9[976]\d|8[987530]\d|6[987]\d|5[90]\d|42\d|3[875]\d|" +
                        @"2[98654321]\d|9[8543210]|8[6421]|6[6543210]|5[87654321]|" +
                        @"4[987654310]|3[9643210]|2[70]|7|1)\d{1,14}$";
                    return Regex.IsMatch(value, regexPattern);

                default: return false;
            }
        }
    }
}
