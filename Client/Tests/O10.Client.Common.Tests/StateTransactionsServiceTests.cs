//using NSubstitute;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Numerics;
//using System.Threading;
//using O10.Transactions.Core.Parsers;
//using O10.Transactions.Core.Serializers;
//using O10.Client.Common.Communication;
//using O10.Client.Common.Interfaces;
//using O10.Core.Communication;
//using O10.Core.Cryptography;
//using O10.Core.HashCalculations;
//using O10.Core.Identity;
//using O10.Crypto.ConfidentialAssets;
//using O10.Crypto.HashCalculations;
//using O10.Tests.Core.Fixtures;
//using Xunit;
//using O10.Client.Common.Crypto;
//using Chaos.NaCl;
//using O10.Transactions.Core.Parsers.Transactional;
//using O10.Transactions.Core.DataModel.Transactional;
//using System.Threading.Tasks.Dataflow;
//using O10.Transactions.Core.Interfaces;
//using O10.Transactions.Core.DataModel;
//using O10.Client.DataLayer.Services;

//namespace O10.Client.Common.Tests
//{
//    public class StateTransactionsServiceTests
//	{
//		private readonly IDataAccessService _dataAccessService;
//		private readonly ISerializersFactory _serializersFactory;
//		private readonly IBlockParsersRepositoriesRepository _blockParsersRepositoriesRepository;
//		private readonly IHashCalculationsRepository _hashCalculationsRepository;
//		private readonly IHashCalculation _proofOfWorkCalculation;
//		private readonly IIdentityKeyProvider _identityKeyProvider;
//		private readonly IIdentityKeyProvidersRegistry _identityKeyProvidersRegistry;
//		private readonly IStateClientCryptoService _clientCryptoService;
//		private readonly IStateTransactionsService _transactionsService;
//        private readonly ISyncStateProvider _syncStateProvider;

//		private readonly byte[] _secretKey;
//		private readonly IKey _publicKey;
//		private readonly byte[] _lastSyncBlockHash = ConfidentialAssetsHelper.GetRandomSeed();
//		private readonly byte[] _lastBlockHash = ConfidentialAssetsHelper.GetRandomSeed();

//		private Action<IPacketProvider> _checkTransactionAction;
//		private Action<IPacketProvider> _checkRegistryAction;

//		public StateTransactionsServiceTests()
//		{
//            _syncStateProvider = Substitute.For<ISyncStateProvider>();
//			_dataAccessService = Substitute.For<IDataAccessService>();
			
//			_serializersFactory = new SerializersFactory(GetAllInstancesByInterface<ISerializer>());
//			_blockParsersRepositoriesRepository = Substitute.For<IBlockParsersRepositoriesRepository>();

//			_hashCalculationsRepository = Substitute.For<IHashCalculationsRepository>();
//			_proofOfWorkCalculation = new Tiger4HashCalculation();
//			_hashCalculationsRepository.Create(HashType.Keccak256).Returns(new Keccak256HashCalculation());
//			_hashCalculationsRepository.Create(HashType.MurMur).Returns(new MurMurHashCalculation());
//			_hashCalculationsRepository.Create(HashType.Tiger4).Returns(_proofOfWorkCalculation);

//			_identityKeyProvider = Substitute.For<IIdentityKeyProvider>();
//			_identityKeyProvidersRegistry = Substitute.For<IIdentityKeyProvidersRegistry>();
//			_identityKeyProvidersRegistry.GetInstance().ReturnsForAnyArgs(new DefaultKeyProvider());
//			_clientCryptoService = new StateClientCryptoService(_hashCalculationsRepository, _identityKeyProvidersRegistry);

//			_transactionsService = new StateTransactionsService(_hashCalculationsRepository,
//                                                       _identityKeyProvidersRegistry,
//                                                       _serializersFactory,
//                                                       _blockParsersRepositoriesRepository);
//			_transactionsService.SetSyncStateProvider(_syncStateProvider);
//			_transactionsService.Initialize(_clientCryptoService, 1);

//            ((TransactionsServiceBase)_transactionsService).PipeOutTransactions.LinkTo(new ActionBlock<Tuple<IPacketProvider, IPacketProvider>>(p =>
//            {
//                _checkTransactionAction.Invoke(p.Item1);
//                _checkRegistryAction.Invoke(p.Item2);
//			}));

//            _syncStateProvider.GetLastSyncBlock().ReturnsForAnyArgs(new SyncBlockModel(1, _lastSyncBlockHash));

//			_secretKey = ConfidentialAssetsHelper.GetRandomSeed();
//			Ed25519.KeyPairFromSeed(out byte[] publicKey, out byte[] expandedSecretKey, _secretKey);
//			_publicKey = new Key32(ConfidentialAssetsHelper.GetPublicKey(publicKey));

//			_clientCryptoService.Initialize(_secretKey);
//		}
		
//        [Fact]
//        public void IssueBlindedAssetTest()
//        {
//            ManualResetEvent transactionChecked = new ManualResetEvent(false);
//            ManualResetEvent witnessChecked = new ManualResetEvent(false);

//            byte[] assetId = ConfidentialAssetsHelper.GetRandomSeed();
//            byte[] groupId = ConfidentialAssetsHelper.GetRandomSeed();

//            _checkTransactionAction = new Action<IPacketProvider>(t =>
//            {
//                byte[] bytes = t.GetBytes();

//                IssueBlindedAssetParser parser = new IssueBlindedAssetParser(_identityKeyProvidersRegistry);
//                IssueBlindedAsset packet = (IssueBlindedAsset)parser.Parse(bytes);
                
//                _clientCryptoService.GetBoundedCommitment(assetId, out byte[] assetCommitment, out byte[] keyImage, out RingSignature ringSignature);

//                Assert.Equal(assetCommitment, packet.AssetCommitment);
//                Assert.Equal(keyImage, packet.KeyImage);
//                Assert.Equal(ringSignature.C, packet.UniquencessProof.C);
//                Assert.Equal(ringSignature.R, packet.UniquencessProof.R);

//                transactionChecked.Set();
//            });

//            _checkRegistryAction = new Action<IPacketProvider>(r =>
//            {
//                byte[] bytes = r.GetBytes();
//                witnessChecked.Set();
//            });

//            _transactionsService.IssueBlindedAsset(assetId, groupId, out byte[] originatingCommitment);

//            if(!transactionChecked.WaitOne(3000))
//            {
//                throw new TimeoutException();
//            }

//            if(!witnessChecked.WaitOne(3000))
//            {
//                throw new TimeoutException();
//            }
//        }

//		//[Fact]
//		//public void transferAssetToStealthTest()
//		//{
//  //          bool retval = _transactionsService.transferAssetToStealth(
//  //              Array.Empty<byte>(), 
//  //              new Entities.ConfidentialAccount());

//  //          _networkSynchronizer
//  //              .WhenForAnyArgs(t =>
//  //              t.SendData(null, null));

//  //          Assert.True(retval);
//  //      }


//        private I[] GetAllInstancesByInterface<I>()
//        {
//            var typeOfInterface = typeof(I);
//            var types = AppDomain.CurrentDomain.GetAssemblies()
//                .SelectMany(s =>
//                    s.GetTypes())
//                .Where(p =>
//                    typeOfInterface.IsAssignableFrom(p) && p.IsClass && !p.IsAbstract);

//            List<object> list = new List<object>();

//            foreach (var type in types)
//            {
//                object o = Activator.CreateInstance(type);

//                list.Add(o);
//            }

//            return list.Cast<I>().ToArray();
//        }

//		private byte[] GetPowHash(byte[] hash, ulong nonce)
//		{
//			BigInteger bigInteger = new BigInteger(hash);
//			bigInteger += nonce;
//			byte[] hashNonce = bigInteger.ToByteArray();
//			byte[] powHash = _proofOfWorkCalculation.CalculateHash(hashNonce);
//			return powHash;
//		}
//	}
//}