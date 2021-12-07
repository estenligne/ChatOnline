using Xamarin.Forms;

namespace XamApp.ViewModels
{
    public class AddRoomViewModel : BaseViewModel
    {
        private string accountId;
        private string groupName;
        private string joinToken;

        public string AccountId
        {
            get { return accountId; }
            set
            {
                if (SetProperty(ref accountId, value))
                {
                    OnUserEntry(Type.Numeric, value);
                    OnPropertyChanged(nameof(AccountIdColor));
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

        public Color AccountIdColor => IsValid(Type.Numeric, AccountId) ? Color.Green : Color.Black;

        public Color GroupNameColor => IsValid(Type.ProfileName, GroupName) ? Color.Green : Color.Black;

        public Color JoinTokenColor => IsValid(Type.JoinToken, JoinToken) ? Color.Green : Color.Black;

        public bool CanAdd => !IsBusy && (
            IsValid(Type.Numeric, AccountId) !=
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
                if (type != Type.Numeric) AccountId = null;
                if (type != Type.ProfileName) GroupName = null;
                if (type != Type.JoinToken) JoinToken = null;
            }
            UpdateButton();
        }
    }
}
