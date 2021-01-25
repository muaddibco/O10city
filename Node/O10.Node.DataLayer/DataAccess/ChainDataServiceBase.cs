using System.Collections.Generic;
using System.Threading;
using O10.Transactions.Core.Enums;
using O10.Node.DataLayer.DataServices;
using O10.Node.DataLayer.DataServices.Keys;
using O10.Core.Logging;
using O10.Core.Models;
using O10.Core.Translators;
using O10.Node.DataLayer.Exceptions;

namespace O10.Node.DataLayer.DataAccess
{
    public abstract class ChainDataServiceBase<T> : IChainDataService where T : INodeDataAccessService
    {
        protected ChainDataServiceBase(INodeDataAccessServiceRepository dataAccessServiceRepository,
                                       ITranslatorsRepository translatorsRepository,
                                       ILoggerService loggerService)
        {
            Service = (T)dataAccessServiceRepository.GetInstance(PacketType);
            TranslatorsRepository = translatorsRepository;
            Logger = loggerService.GetLogger(GetType().Name);
        }

        protected T Service { get; }
        protected ITranslatorsRepository TranslatorsRepository { get; }
        protected ILogger Logger { get; }

        public abstract PacketType PacketType { get; }
        public IChainDataServicesManager ChainDataServicesManager { protected get; set; }

        public abstract void Add(PacketBase item);
        public abstract IEnumerable<PacketBase> Get(IDataKey key);
        public abstract void Initialize(CancellationToken cancellationToken);

        public virtual ulong GetScalar(IDataKey dataKey)
        {
            if (dataKey == null)
            {
                throw new System.ArgumentNullException(nameof(dataKey));
            }

            throw new DataKeyNotSupportedException(dataKey);
        }
    }
}
