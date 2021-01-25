using System.Collections.Generic;

namespace O10.Client.Mobile.Base.ViewModels.Elements
{
    public class AssociatedAttributesViewModel : List<AssociatedAttributeViewModel>
    {
        public AssociatedAttributesViewModel(IEnumerable<AssociatedAttributeViewModel> collection)
            : base(collection)
        {

        }

        public string IssuerName { get; set; }
    }
}
