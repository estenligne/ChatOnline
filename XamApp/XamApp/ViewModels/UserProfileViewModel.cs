using Xamarin.Forms;

namespace XamApp.ViewModels
{
    public class UserProfileViewModel : BaseViewModel
    {
        private string name;
        private string about;
        public long userProfileId;

        public string Name
        {
            get { return name; }
            set
            {
                if (SetProperty(ref name, value))
                {
                    OnPropertyChanged(nameof(NameColor));
                    UpdateButton();
                }
            }
        }

        public string About
        {
            get { return about; }
            set
            {
                if (SetProperty(ref about, value))
                {
                    UpdateButton();
                }
            }
        }

        public string ButtonText => userProfileId == 0 ? "Create" : "Save";

        public bool CanSave => IsValid(Type.ProfileName, Name);

        public Color NameColor => IsValid(Type.ProfileName, Name) ? Color.Green : Color.Black;

        public void UpdateButton()
        {
            OnPropertyChanged(nameof(CanSave));
        }
    }
}
