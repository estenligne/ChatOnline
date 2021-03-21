using System.Text.RegularExpressions;

namespace XamApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string emailOrPhone;
        private string password;

        public string EmailOrPhone
        {
            get { return emailOrPhone; }
            set
            {
                if (SetProperty(ref emailOrPhone, value))
                    UpdateCanLogin();
            }
        }

        public string Password
        {
            get { return password; }
            set
            {
                if (SetProperty(ref password, value))
                    UpdateCanLogin();
            }
        }

        public void UpdateCanLogin()
        {
            OnPropertyChanged(nameof(CanLogin));
        }

        public bool CanLogin
        {
            get
            {
                if (IsBusy
                || string.IsNullOrWhiteSpace(EmailOrPhone)
                || string.IsNullOrWhiteSpace(Password)
                || Password.Length < 6)
                    return false;

                if (EmailOrPhone[0] == '+') // if a phone number
                {
                    return true;
                }
                else // else is an email address
                {
                    string regexPattern =
                                @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" + "@" +
                                @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$";

                    return Regex.IsMatch(EmailOrPhone, regexPattern);
                }
            }
        }
    }
}
