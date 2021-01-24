using System.ComponentModel;
using Xamarin.Forms;
using XamApp.ViewModels;

namespace XamApp.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}