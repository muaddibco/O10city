using O10.Core.Architecture;

namespace O10.Client.IdentityProvider.External
{
    [ExtensionPoint]
    public interface IExternalIdpDataValidator
    {
        string Name { get; }

        void Validate(object request);
    }
}
