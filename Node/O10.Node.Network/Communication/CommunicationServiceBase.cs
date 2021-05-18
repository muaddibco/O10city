using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using O10.Network.Interfaces;
using O10.Core;
using O10.Core.Communication;
using O10.Core.Identity;
using O10.Core.Logging;
using O10.Network.Handlers;
using O10.Core.ExtensionMethods;
using Microsoft.Extensions.DependencyInjection;
using O10.Network.Topology;

namespace O10.Network.Communication
{
    public abstract class CommunicationServiceBase : ICommunicationService
	{
		protected readonly ILogger _log;
		private readonly IServiceProvider _serviceProvider;
		protected readonly IBufferManagerFactory _bufferManagerFactory;
		protected readonly IPacketsHandler _packetsHandler;
		protected GenericPool<ICommunicationChannel> _communicationChannelsPool;
		protected readonly List<ICommunicationChannel> _clientConnectedList;
		protected int _connectedSockets = 0;
		protected INodesResolutionService _nodesResolutionService;
		protected readonly BlockingCollection<KeyValuePair<IKey, byte[]>> _messagesQueue;
		protected CancellationTokenSource _cancellationTokenSource;
		protected SocketSettings _settings;

		public CommunicationServiceBase(IServiceProvider serviceProvider,
                                  ILoggerService loggerService,
                                  IBufferManagerFactory bufferManagerFactory,
                                  IPacketsHandler packetsHandler,
                                  INodesResolutionService nodesResolutionService)
		{
			_log = loggerService.GetLogger(GetType().Name);
			_serviceProvider = serviceProvider;
			_bufferManagerFactory = bufferManagerFactory;
			_packetsHandler = packetsHandler;
			_clientConnectedList = new List<ICommunicationChannel>();
			_nodesResolutionService = nodesResolutionService;
			_messagesQueue = new BlockingCollection<KeyValuePair<IKey, byte[]>>();
		}

		#region ICommunicationService implementation

		public abstract string Name { get; }

		/// <summary>
		/// Initializes the server by preallocating reusable buffers and 
		/// context objects.  These objects do not need to be preallocated 
		/// or reused, but it is done this way to illustrate how the API can 
		/// easily be used to create reusable objects to increase server performance.
		/// </summary>
		public virtual void Init(SocketSettings settings)
		{

			_settings = settings;

			// Allocates one large byte buffer which all I/O operations use a piece of.  This guards against memory fragmentation
			IBufferManager bufferManager = _bufferManagerFactory.Create();
			bufferManager.InitBuffer(_settings.BufferSize * _settings.MaxConnections * 2, _settings.BufferSize);
			_communicationChannelsPool = new GenericPool<ICommunicationChannel>(_settings.MaxConnections);

			for (int i = 0; i < _settings.MaxConnections; i++)
			{
				ICommunicationChannel communicationChannel = _serviceProvider.GetService<ICommunicationChannel>();
				communicationChannel.SocketClosedEvent += ClientHandler_SocketClosedEvent;
				communicationChannel.Init(bufferManager);
				_communicationChannelsPool.Push(communicationChannel);
			}
		}

		public virtual void Start()
		{
			_cancellationTokenSource?.Cancel();

			_cancellationTokenSource = new CancellationTokenSource();

			Task.Factory.StartNew(() =>
			{
				ProcessMessagesQueue(_cancellationTokenSource.Token);
			}, _cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
		}

		public void PostMessage(IKey destination, IPacketProvider message)
		{
			if (destination == null)
			{
				throw new ArgumentNullException(nameof(destination));
			}

			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			byte[] messageBytes = message.GetBytes();

			_log.Debug(() => $"Enqueuing to single destination {destination.Value.ToHexString()} message {messageBytes.ToHexString()}");

			_messagesQueue.Add(new KeyValuePair<IKey, byte[]>(destination, messageBytes));
		}

		public void PostMessage(IEnumerable<IKey> destinations, IPacketProvider message)
		{
			if (destinations == null)
			{
				throw new ArgumentNullException(nameof(destinations));
			}

			if (message == null)
			{
				throw new ArgumentNullException(nameof(message));
			}

			foreach (IKey destination in destinations)
			{
				byte[] messageBytes = message.GetBytes();

				_log.Debug(() => $"Enqueuing to one of multiple destinations {destination.Value.ToHexString()} message {messageBytes.ToHexString()}");
				_messagesQueue.Add(new KeyValuePair<IKey, byte[]>(destination, messageBytes));
			}
		}

		public void Stop()
		{
			_cancellationTokenSource?.Cancel();
			_cancellationTokenSource = null;
		}

		#endregion ICommunicationService implementation

		#region Private Functions

		protected abstract Socket CreateSocket();

		private void ClientHandler_SocketClosedEvent(object sender, EventArgs e)
		{
			ICommunicationChannel communicationChannel = (ICommunicationChannel)sender;
			ReleaseClientHandler(communicationChannel);
		}

		protected virtual void ReleaseClientHandler(ICommunicationChannel communicationChannel)
		{
			if (_clientConnectedList.Contains(communicationChannel))
			{
				_clientConnectedList.Remove(communicationChannel);
			}

			_communicationChannelsPool.Push(communicationChannel);

			Interlocked.Decrement(ref _connectedSockets);
			_log.Info($"Socket with IP {communicationChannel.RemoteIPAddress} disconnected. {_connectedSockets} client(s) connected.");
		}

		protected void InitializeCommunicationChannel(Socket socket)
		{
			ICommunicationChannel communicationChannel = _communicationChannelsPool.Pop();

			Int32 numberOfConnectedSockets = Interlocked.Increment(ref _connectedSockets);
			_log.Info($"Initializing communication channel for IP {socket.LocalEndPoint}, total concurrent accepted sockets is {numberOfConnectedSockets}");

			try
			{
				communicationChannel.AcceptSocket(socket);
				_clientConnectedList.Add(communicationChannel);
			}
			catch (Exception ex)
			{
				_log.Error("Failed to accept connection by communication channel", ex);
				ReleaseClientHandler(communicationChannel);
			}

		}

		private void ProcessMessagesQueue(CancellationToken token)
		{
			foreach (var messageByKey in _messagesQueue.GetConsumingEnumerable(token))
			{
				IPAddress address = _nodesResolutionService.ResolveNodeAddress(messageByKey.Key);

				ICommunicationChannel communicationChannel = GetChannel(address);

				if (communicationChannel != null)
				{
					communicationChannel.PostMessage(messageByKey.Value);
				}
				else
				{
					_log.Error($"Failed to obtain the channel to address {address}");
				}
			}
		}

		protected ICommunicationChannel GetChannel(IPAddress address)
		{
			//TODO: implement double check with lock for sake of better performance
			lock (_communicationChannelsPool)
			{
				ICommunicationChannel communicationChannel = FindChannel(address);
				if (communicationChannel == null)
				{
					AutoResetEvent autoResetEvent = new AutoResetEvent(false);
					CreateChannel(new IPEndPoint(address, _settings.RemotePort)).ContinueWith((t, e) =>
					{
						if (t.IsCompleted && !t.IsCanceled && !t.IsFaulted && t.Result.Succeeded)
						{
							_clientConnectedList.Add(t.Result.Tag);
						}
						else
						{
							_communicationChannelsPool.Push(t.Result.Tag);
						}

						(e as AutoResetEvent)?.Set();
					}, autoResetEvent, TaskScheduler.Current);

					autoResetEvent.WaitOne(CommunicationChannel.TIMEOUT);

					communicationChannel = FindChannel(address);
				}

				return communicationChannel;
			}
		}

		protected virtual ICommunicationChannel FindChannel(IPAddress address) => _clientConnectedList.FirstOrDefault(c => c.RemoteIPAddress.Equals(address));

		protected Task<OperationStatus<ICommunicationChannel>> CreateChannel(EndPoint endPoint)
		{
			if (_communicationChannelsPool.Count > 0)
			{
				ICommunicationChannel communicationChannel = _communicationChannelsPool.Pop();
				Interlocked.Increment(ref _connectedSockets);

				Socket socket = CreateSocket();

				return communicationChannel.StartConnectionAsync(socket, endPoint);
			}
			else
			{
				_log.Error("No more room for communication channels left");
			}

			return null;
		}


		#endregion Private Functions
	}
}
