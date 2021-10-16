using System.Collections.Generic;
using System.Threading;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataServices;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Core.Logging;
using O10.Core.Translators;
using O10.Transactions.Core.Ledgers;
using O10.Core.Models;
using O10.Core.Identity;
using System.Threading.Tasks;

namespace O10.Node.DataLayer.DataAccess
{
    public abstract class ChainDataServiceBase<T> : IChainDataService where T : INodeDataAccessService
    {
        protected ChainDataServiceBase(INodeDataAccessServiceRepository dataAccessServiceRepository,
                                       ITranslatorsRepository translatorsRepository,
                                       IIdentityKeyProvidersRegistry identityKeyProvidersRegistry,
                                       ILoggerService loggerService)
        {
            Service = (T)dataAccessServiceRepository.GetInstance(LedgerType);
            TranslatorsRepository = translatorsRepository;
            IdentityKeyProvider = identityKeyProvidersRegistry.GetInstance();
            Logger = loggerService.GetLogger(GetType().Name);
        }

        protected T Service { get; }
        protected ITranslatorsRepository TranslatorsRepository { get; }
        protected IIdentityKeyProvider IdentityKeyProvider { get; }
        protected ILogger Logger { get; }
        protected CancellationToken CancellationToken { get; private set; }
        public abstract LedgerType LedgerType { get; }

        public abstract TaskCompletionWrapper<IPacketBase> Add(IPacketBase item);
        public abstract Task<IEnumerable<IPacketBase>> Get(IDataKey key, CancellationToken cancellationToken);
        
        public virtual async Task Initialize(CancellationToken cancellationToken)
        {
            CancellationToken = cancellationToken;
        }

        public abstract void AddDataKey(IDataKey key, IDataKey newKey);
    }
}
