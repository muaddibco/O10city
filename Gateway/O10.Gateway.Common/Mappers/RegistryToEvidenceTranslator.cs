using O10.Core.Architecture;
using O10.Core.Translators;
using O10.Transactions.Core.Accessors;
using O10.Transactions.Core.Ledgers.Registry.Transactions;

namespace O10.Gateway.Common.Mappers
{
    [RegisterExtension(typeof(ITranslator), Lifetime = LifetimeManagement.Singleton)]
    public class RegistryToEvidenceTranslator : TranslatorBase<RegisterTransaction, EvidenceDescriptor>
    {
        public override EvidenceDescriptor Translate(RegisterTransaction registerTransaction)
        {
            if (registerTransaction is null)
            {
                throw new System.ArgumentNullException(nameof(registerTransaction));
            }

            return new EvidenceDescriptor(registerTransaction.ReferencedLedgerType, registerTransaction.ReferencedAction, registerTransaction.Parameters);
        }
    }
}
