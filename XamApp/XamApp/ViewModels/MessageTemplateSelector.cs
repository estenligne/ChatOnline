using Xamarin.Forms;

namespace XamApp.ViewModels
{
    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate EventDataTemplate { get; set; }
        public DataTemplate MessageDataTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            Message message = (Message)item;
            return message.IsAnEvent ? EventDataTemplate : MessageDataTemplate;
        }
    }
}
