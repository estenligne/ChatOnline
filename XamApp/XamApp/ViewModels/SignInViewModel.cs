﻿using Xamarin.Forms;

namespace XamApp.ViewModels
{
    public class SignInViewModel : BaseViewModel
    {
        private bool doSignIn;
        public bool DoSignIn => doSignIn;
        public bool DoRegister => !doSignIn;

        public void UpdateChoice(bool doSignIn)
        {
            this.doSignIn = doSignIn;
            OnPropertyChanged(nameof(DoSignIn));
            OnPropertyChanged(nameof(DoRegister));
            Title = (DoSignIn ? "Sign In" : "Register") + " to EstEnLigne";
        }

        private void UpdateButtons()
        {
            OnPropertyChanged(nameof(CanSignIn));
            OnPropertyChanged(nameof(CanRegister));
        }

        private string email;
        private string phoneNumber;
        private string password;
        private string passwordConfirm;

        public string Email
        {
            get { return email; }
            set
            {
                if (SetProperty(ref email, value))
                {
                    UpdateButtons();
                    OnPropertyChanged(nameof(EmailColor));
                    OnPropertyChanged(nameof(PhoneNumberAllow));
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
                    OnPropertyChanged(nameof(EmailAllow));
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

        public bool ValidIdentity => !IsBusy && (IsValid(Type.Email, Email) || IsValid(Type.PhoneNumber, PhoneNumber));

        public bool CanSignIn => ValidIdentity && IsValid(Type.Password, Password);

        public bool CanRegister => CanSignIn && Password == PasswordConfirm;

        public bool EmailAllow => string.IsNullOrEmpty(PhoneNumber);

        public bool PhoneNumberAllow => string.IsNullOrEmpty(Email);

        public Color EmailColor => IsValid(Type.Email, Email) ? Color.Green : Color.Black;

        public Color PhoneNumberColor => IsValid(Type.PhoneNumber, PhoneNumber) ? Color.Green : Color.Black;

        public Color PasswordColor => IsValid(Type.Password, Password) ? Color.Green : Color.Black;

        public Color PasswordConfirmColor => Password == PasswordConfirm ? Color.Green : Color.Black;
    }
}
