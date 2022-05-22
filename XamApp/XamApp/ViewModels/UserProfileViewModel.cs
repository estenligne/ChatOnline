using Global.Helpers;
using Global.Models;
using System.IO;
using System.Threading.Tasks;
using XamApp.Models;
using XamApp.Services;
using Xamarin.Forms;

namespace XamApp.ViewModels
{
    public class UserProfileViewModel : BaseViewModel
    {
        public UserProfileDTO user = new UserProfileDTO();

        public async Task OnAppearing()
        {
            if (!IsBusy && user.Name == null)
            {
                IsBusy = true;

                if (OnCreate)
                {
                    CanEdit = true;
                    IsCurrentUser = true;
                }
                else
                {
                    var response = await HTTPClient.GetAsync(null, "/api/UserProfile?id=" + user.Id);

                    if (response.IsSuccessStatusCode)
                    {
                        user = await HTTPClient.ReadAsAsync<UserProfileDTO>(response);
                        OnPropertyChanged(nameof(Name));
                        OnPropertyChanged(nameof(About));
                        OnPropertyChanged(nameof(CanSave));
                        OnPropertyChanged(nameof(ImageFile));
                        OnPropertyChanged(nameof(BottomButtonText));
                    }
                    else
                    {
                        await DisplayAlert("Error", await HTTPClient.GetResponseError(response), "OK");
                    }
                    response.Dispose();
                }
                IsBusy = false;
            }
        }

        private FileDTO imageChosen;
        public FileDTO ImageChosen
        {
            get { return imageChosen; }
            set
            {
                imageChosen = value;
                OnPropertyChanged(nameof(ImageFile));
            }
        }

        public ImageSource ImageFile
        {
            get
            {
                if (ImageChosen != null)
                {
                    return ImageSource.FromStream(() => new MemoryStream(ImageChosen.Content, false));
                }
                else if (user.PhotoFile == null)
                {
                    return ImageSource.FromFile("userimage.png");
                }
                else
                {
                    var uri = HTTPClient.GetFileUri(user.PhotoFile.Name);
                    return ImageSource.FromUri(uri);
                }
            }
        }

        private bool isCurrentUser;
        public bool IsCurrentUser
        {
            get { return isCurrentUser; }
            set
            {
                isCurrentUser = value;
                OnPropertyChanged(nameof(IsCurrentUser));
                OnPropertyChanged(nameof(ToolbarButtonText));
            }
        }

        private bool canEdit;
        public bool CanEdit
        {
            get { return canEdit; }
            set
            {
                canEdit = value;
                OnPropertyChanged(nameof(CanEdit));
                OnPropertyChanged(nameof(CannotEdit));
                OnPropertyChanged(nameof(ToolbarButtonText));
                OnPropertyChanged(nameof(BottomButtonText));
            }
        }

        public bool CannotEdit => !CanEdit;

        public string Name
        {
            get { return user.Name; }
            set
            {
                if (user.Name != value)
                {
                    user.Name = value;
                    OnPropertyChanged(nameof(Name));
                    OnPropertyChanged(nameof(NameColor));
                    UpdateButton();
                }
            }
        }

        public string About
        {
            get { return user.About; }
            set
            {
                if (user.About != value)
                {
                    user.About = value;
                    OnPropertyChanged(nameof(About));
                    UpdateButton();
                }
            }
        }

        public bool OnCreate => user.Id == 0;

        public bool HideToolbarButton => OnCreate || !IsCurrentUser;

        public string ToolbarButtonText => HideToolbarButton ? "" : CanEdit ? "View" : "Edit";

        public string BottomButtonText => CanEdit ? "Save" : OnCreate ? "Create" : "";

        public bool CanSave => IsValid(Type.ProfileName, Name);

        public Color NameColor => IsValid(Type.ProfileName, Name) ? Color.Green : Color.Black;

        public void UpdateButton()
        {
            OnPropertyChanged(nameof(CanSave));
        }
    }
}
