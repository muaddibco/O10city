using O10.Core.Architecture;
using O10.Core.Translators;
using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.Ledgers.Registry;
using O10.Transactions.Core.Ledgers.Registry.Transactions;
using System;

namespace O10.Gateway.Common.Mappers
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class EvidenceToRegistryTranslator : TranslatorBase<EvidenceDescriptor, RegistryPacket>
    {
        public override RegistryPacket Translate(EvidenceDescriptor descriptor)
        {
            if (descriptor is null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            var packet = new RegistryPacket
            {
                Body = new RegisterTransaction
                {
                    ReferencedLedgerType = descriptor.LedgerType,
                    ReferencedAction = descriptor.ActionType,
                    Parameters = descriptor.Parameters
                }
            };

            return packet;
        }
    }
}
