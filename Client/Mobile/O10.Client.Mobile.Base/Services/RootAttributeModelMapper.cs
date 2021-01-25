using O10.Client.DataLayer.Model;
using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Core.Translators;
using O10.Client.Mobile.Base.Models;

namespace O10.Client.Mobile.Base.Services
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class RootAttributeModelMapper : TranslatorBase<UserRootAttribute, RootAttributeModel>
    {
        public override RootAttributeModel Translate(UserRootAttribute a)
        {
            return new RootAttributeModel
            {
                AttributeId = a.UserAttributeId,
                Content = a.Content,
                AssetId = a.AssetId,
                IsActive = !a.IsOverriden,
                Issuer = a.Source,
                Key = $"{a.Source}-{a.AssetId.ToHexString()}"
            };
        }
    }
}
