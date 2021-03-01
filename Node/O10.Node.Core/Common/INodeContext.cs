using System;
using System.Collections.Generic;
using O10.Core.Cryptography;
using O10.Core.Identity;
using O10.Core.States;

namespace O10.Node.Core.Common
{
    public interface INodeContext : IState, IDisposable
    {
        IKey AccountKey { get; }

        //List<SynchronizationGroupParticipant> SyncGroupParticipants { get; }

        void Initialize(ISigningService signingService);

        ISigningService SigningService { get; }
    }
}
