using System.Collections.Generic;
using System.Threading;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataServices;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Core.Logging;
using O10.Core.Translators;
using O10.Node.DataLayer.Exceptions;
using O10.Transactions.Core.Ledgers;
using O10.Core.Models;
using O10.Core.Identity;

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

        public abstract LedgerType LedgerType { get; }

        public abstract TaskCompletionWrapper<IPacketBase> Add(IPacketBase item);
        public abstract IEnumerable<IPacketBase> Get(IDataKey key);
        public abstract void Initialize(CancellationToken cancellationToken);

        //TODO: not clear why does this virtual exists?
        public virtual long GetScalar(IDataKey dataKey)
        {
            if (dataKey == null)
            {
                throw new System.ArgumentNullException(nameof(dataKey));
            }

            throw new DataKeyNotSupportedException(dataKey);
        }

        public abstract void AddDataKey(IDataKey key, IDataKey newKey);
    }
}
