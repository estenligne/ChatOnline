using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace XamApp.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        private bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        private string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged; // see INotifyPropertyChanged
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null && !string.IsNullOrEmpty(propertyName))
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected Task DisplayAlert(string title, string message, string cancel)
        {
            return Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }

        public enum Type
        {
            Email,
            Password,
            PhoneNumber,
            ProfileName,
            JoinToken,
        }

        public static bool IsValid(Type Type, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            int length = value.Length;

            if (value[0] == ' ' || value[length - 1] == ' ')
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
                    return 6 <= length && length <= 99;

                case Type.ProfileName:
                    return 5 <= length && length <= 63;

                case Type.JoinToken:
                    return 5 <= length && length <= 70;

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
