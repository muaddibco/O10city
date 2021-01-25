using O10.Core.Architecture;

namespace O10.Client.Web.Portal.ExternalIdps.Validators
{
    [ExtensionPoint]
    public interface IExternalIdpDataValidator
    {
        string Name { get; }

        void Validate(object request);
    }
}
