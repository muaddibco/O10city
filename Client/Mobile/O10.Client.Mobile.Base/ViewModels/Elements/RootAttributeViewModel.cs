using Prism.Mvvm;
using System;
using O10.Client.Mobile.Base.Enums;

namespace O10.Client.Mobile.Base.ViewModels.Elements
{
    public class RootAttributeViewModel : BindableBase
    {
        private string _issuer;
        private string _attributeSchemeName;
        private AttributeState _attributeState;
        private string _assetId;
        private DateTime _creationTime;
        private DateTime _confirmationTime;
        private DateTime _lastUpdateTime;

        public long AttributeId { get; set; }

        public string AssetId
        {
            get => _assetId;
            set
            {
                SetProperty(ref _assetId, value);
            }
        }

        public string Issuer
        {
            get => _issuer;
            set
            {
                SetProperty(ref _issuer, value);
            }
        }

        public string AttributeSchemeName
        {
            get => _attributeSchemeName;
            set
            {
                SetProperty(ref _attributeSchemeName, value);
            }
        }

        public string Content { get; set; }

        public AttributeState AttributeState
        {
            get => _attributeState;
            set
            {
                SetProperty(ref _attributeState, value);
            }
        }

        public DateTime CreationTime
        {
            get => _creationTime;
            set
            {
                SetProperty(ref _creationTime, value);
            }
        }

        public DateTime ConfirmationTime
        {
            get => _confirmationTime;
            set
            {
                SetProperty(ref _confirmationTime, value);
            }
        }

        public DateTime LastUpdateTime
        {
            get => _lastUpdateTime;
            set
            {
                SetProperty(ref _lastUpdateTime, value);
            }
        }
    }
}
