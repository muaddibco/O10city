using Prism.Mvvm;

namespace O10.Client.Mobile.Base.ViewModels.Elements
{
    public class AssociatedAttributeViewModel : BindableBase
    {
        public string SchemeName { get; set; }
        public string Alias { get; set; }
        public string Content { get; set; }
    }
}
