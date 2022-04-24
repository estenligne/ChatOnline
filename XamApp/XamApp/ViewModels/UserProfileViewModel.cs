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

        public bool IsCurrentUser { get; set; }
        public bool IsButtonAtBotton => CanEdit;

        public async Task OnAppearing()
        {
            if (!IsBusy)
            {
                IsBusy = true;

                User currentUser = await DataStore.Instance.GetUserAsync();

                if (currentUser != null)
                {
                    if (user.Id == 0)
                    {
                        user.Id = currentUser.Id;
                        IsCurrentUser = user.Id == currentUser.Id;
                    }
                }
                else if (user.Id == 0)
                {
                    canEdit = true;
                    IsCurrentUser = true;
                }
                OnPropertyChanged(nameof(IsCurrentUser));

                var response = await HTTPClient.GetAsync(null, "/api/UserProfile?id=" + user.Id);

                if (response.IsSuccessStatusCode)
                {
                    user = await HTTPClient.ReadAsAsync<UserProfileDTO>(response);
                    OnPropertyChanged(nameof(Name));
                    OnPropertyChanged(nameof(About));
                    OnPropertyChanged(nameof(Button2));
                    OnPropertyChanged(nameof(ImageFile));
                }
                else
                {
                    await DisplayAlert("ERROR", await HTTPClient.GetResponseError(response), "OK");
                }
                IsBusy = false;
            }

        }

        public MemoryStream imageChosen;
        public MemoryStream ImageChosen
        {
            get { return imageChosen; }
            set
            {
                if (imageChosen != null)
                    imageChosen.Dispose();

                imageChosen = value;
                OnPropertyChanged(nameof(ImageFile));
            }
        }

        public ImageSource ImageFile
        {
            get
            {
                if (imageChosen != null)
                {
                    Stream stream = new MemoryStream(imageChosen.ToArray());
                    return ImageSource.FromStream(() => stream);
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
        public bool canEdit;
        public bool CanEdit
        {
            get { return canEdit; }
            set
            {
                canEdit = value;
                OnPropertyChanged(nameof(CanEdit));
                OnPropertyChanged(nameof(CannotEdit));
                OnPropertyChanged(nameof(Button1));
                OnPropertyChanged(nameof(Button2));
                OnPropertyChanged(nameof(IsButtonAtBotton));

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

        public string Button1 => ButtonText();

        public string OnbuttonClicked;
        public string ButtonText()
        {
            if (!canEdit)
                return OnbuttonClicked = "Edit";
            else if (user.Id == 0)
                return OnbuttonClicked = "";
            else
                return OnbuttonClicked = "View";
        }

        public string Button2 => CanEdit ? "Save" : user.Id == 0 ? "Create" : "";

        public bool CanSave => IsValid(Type.ProfileName, Name);

        public Color NameColor => IsValid(Type.ProfileName, Name) ? Color.Green : Color.Black;

        public void UpdateButton()
        {
            OnPropertyChanged(nameof(CanSave));
        }
    }
}
