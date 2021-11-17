using O10.Core.Identity;
using O10.Crypto.Models;
using System;

namespace O10.Client.Stealth.Egress
{
    public class PropagationArgument
    {
        public PropagationArgument(IKey prevDestinationKey, IKey prevTransactionKey, Action<StealthTransactionBase> preSigningAction = null)
        {
            PrevDestinationKey = prevDestinationKey;
            PrevTransactionKey = prevTransactionKey;
            PreSigningAction = preSigningAction;
        }

        public IKey PrevDestinationKey { get; set; }

        public IKey PrevTransactionKey { get; set; }

        public Action<StealthTransactionBase> PreSigningAction { get; }
    }
}
