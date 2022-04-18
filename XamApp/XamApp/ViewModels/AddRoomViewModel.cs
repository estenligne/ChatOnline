using Xamarin.Forms;

namespace XamApp.ViewModels
{
    public class AddRoomViewModel : BaseViewModel
    {
        private string accountID;
        private string groupName;
        private string joinToken;

        public string AccountID
        {
            get { return accountID; }
            set
            {
                if (SetProperty(ref accountID, value))
                {
                    OnUserEntry(Type.AccountID, value);
                    OnPropertyChanged(nameof(AccountIDColor));
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
                    OnUserEntry(Type.GroupName, value);
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

        public Color AccountIDColor => IsValid(Type.AccountID, AccountID) ? Color.Green : Color.Black;

        public Color GroupNameColor => IsValid(Type.GroupName, GroupName) ? Color.Green : Color.Black;

        public Color JoinTokenColor => IsValid(Type.JoinToken, JoinToken) ? Color.Green : Color.Black;

        public bool CanAdd => !IsBusy && (
            IsValid(Type.AccountID, AccountID) !=
            IsValid(Type.GroupName, GroupName) !=
            IsValid(Type.JoinToken, JoinToken));

        public void UpdateButton()
        {
            OnPropertyChanged(nameof(CanAdd));
        }

        private void OnUserEntry(Type type, string value)
        {
            if (value != null)
            {
                if (type != Type.AccountID) AccountID = null;
                if (type != Type.GroupName) GroupName = null;
                if (type != Type.JoinToken) JoinToken = null;
            }
            UpdateButton();
        }
    }
}
