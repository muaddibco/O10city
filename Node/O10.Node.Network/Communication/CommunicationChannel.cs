using O10.Network.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using O10.Core.Logging;
using O10.Core.Architecture;
using O10.Core.ExtensionMethods;
using O10.Core.Tracking;
using O10.Core;
using O10.Core.Communication;

namespace O10.Network.Communication
{
    [RegisterDefaultImplementation(typeof(ICommunicationChannel), Lifetime = LifetimeManagement.Transient)]
	public class CommunicationChannel : ICommunicationChannel
	{
		public const int TIMEOUT = 5000;
		private readonly ILogger _log;
		private readonly BlockingCollection<byte[]> _packets;
		private readonly BlockingCollection<byte[]> _postedMessages;
		private IBufferManager _bufferManager;
		private readonly SocketAsyncEventArgs _socketReceiveAsyncEventArgs;
		private readonly SocketAsyncEventArgs _socketSendAsyncEventArgs;
		private readonly ManualResetEventSlim _socketConnectedEvent;
		private readonly AutoResetEvent _socketSendEvent;
		private readonly EscapeHelper _escapeHelper;
		private readonly ITrackingService _trackingService;
		private CancellationTokenSource _cancellationTokenSource;
		private CancellationToken _cancellationToken;

		private int _offsetReceive;
		private int _offsetSend;

		private bool _disposed = false; // To detect redundant calls

		private Func<ICommunicationChannel, IPEndPoint, int, bool> _onReceivedExtendedValidation;

		public event EventHandler<EventArgs> SocketClosedEvent;

		public CommunicationChannel(ILoggerService loggerService, ITrackingService trackingService)
		{
			_log = loggerService.GetLogger(GetType().Name);
			_packets = new BlockingCollection<byte[]>();
			_postedMessages = new BlockingCollection<byte[]>();
			_socketReceiveAsyncEventArgs = new SocketAsyncEventArgs();
			_socketReceiveAsyncEventArgs.Completed += Receive_Completed;
			_socketSendAsyncEventArgs = new SocketAsyncEventArgs();
			_socketSendAsyncEventArgs.Completed += Send_Completed;
			_socketConnectedEvent = new ManualResetEventSlim(false);
			_socketSendEvent = new AutoResetEvent(false);

			_escapeHelper = new EscapeHelper();

			_trackingService = trackingService;
		}

		#region ICommunicationChannel implementation

		public IPAddress RemoteIPAddress { get; set; } = IPAddress.None;

		public void PushForParsing(byte[] buf, int offset, int count)
		{
			try
			{
				byte[] packet = new byte[count];
				Buffer.BlockCopy(buf, offset, packet, 0, count);

				_log.Debug(() => packet.ToHexString());

				if (packet != null)
				{
					_trackingService.TrackMetric("ParsingQueueSize", 1);
					_packets.Add(packet);
				}
			}
			catch (Exception ex)
			{
				_log.Error($"Failure during pushing to buffer of Communication Channel with IP {RemoteIPAddress}", ex);
			}
		}

		public void Init(IBufferManager bufferManager)
		{
			_cancellationTokenSource?.Cancel();

			_cancellationTokenSource = new CancellationTokenSource();
			_cancellationToken = _cancellationTokenSource.Token;

			_bufferManager = bufferManager;

			_bufferManager.SetBuffer(_socketReceiveAsyncEventArgs, _socketSendAsyncEventArgs);
			_offsetReceive = _socketReceiveAsyncEventArgs.Offset;
			_offsetSend = _socketSendAsyncEventArgs.Offset;

			StartSend();
		}

		public Task<OperationStatus<ICommunicationChannel>> StartConnectionAsync(Socket socket, EndPoint endPoint)
		{
			TaskCompletionSource<OperationStatus<ICommunicationChannel>> taskCompletionSource = new TaskCompletionSource<OperationStatus<ICommunicationChannel>>();

			SocketAsyncEventArgs socketConnectAsyncEventArgs = new SocketAsyncEventArgs();
			socketConnectAsyncEventArgs.Completed += Connect_Completed;
			socketConnectAsyncEventArgs.RemoteEndPoint = endPoint;
			socketConnectAsyncEventArgs.UserToken = taskCompletionSource;

			bool willRaiseEvent = socket.ConnectAsync(socketConnectAsyncEventArgs);
			if (!willRaiseEvent)
			{
				try
				{
					if (socketConnectAsyncEventArgs.SocketError == SocketError.Success)
					{
						AcceptSocket(socket);
						taskCompletionSource.SetResult(new OperationStatus<ICommunicationChannel>
						{
							Succeeded = true,
							Tag = this
						});
					}
					else
					{
						taskCompletionSource.SetResult(new OperationStatus<ICommunicationChannel>
						{
							Succeeded = false,
							Description = socketConnectAsyncEventArgs.SocketError.ToString(),
							Tag = this
						});
					}
				}
				finally
				{
					socketConnectAsyncEventArgs.Completed -= Connect_Completed;
				}
			}

			return taskCompletionSource.Task;
		}

		private void Connect_Completed(object sender, SocketAsyncEventArgs e)
		{
			_log.Info($"Connection to remote endpoint {e.RemoteEndPoint} completed");

			if (e.LastOperation == SocketAsyncOperation.Connect)
			{
				TaskCompletionSource<OperationStatus<ICommunicationChannel>> taskCompletionSource = e.UserToken as TaskCompletionSource<OperationStatus<ICommunicationChannel>>;

				if (e.SocketError == SocketError.Success)
				{
					AcceptSocket(e.ConnectSocket);
					taskCompletionSource.SetResult(new OperationStatus<ICommunicationChannel>
					{
						Succeeded = true,
						Tag = this
					});
				}
				else
				{
					taskCompletionSource.SetResult(new OperationStatus<ICommunicationChannel>
					{
						Succeeded = false,
						Description = e.SocketError.ToString(),
						Tag = this
					});
				}
			}
			else
			{
				throw new ArgumentException("The last operation completed on the socket was not a connect.");
			}
		}
		//TODO: need to ascertain that this is really needed, looks weird
		public void RegisterExtendedValidation(Func<ICommunicationChannel, IPEndPoint, int, bool> onReceivedExtendedValidation)
		{
			_onReceivedExtendedValidation = onReceivedExtendedValidation;
		}

		public void AcceptSocket(Socket acceptSocket)
		{
			_trackingService.TrackMetric("CommunicationChannels", 1);

			_log.Info($"Socket accepted by Communication channel.  Remote endpoint = {IPAddress.Parse(((IPEndPoint)acceptSocket.LocalEndPoint).Address.ToString())}:{((IPEndPoint)acceptSocket.LocalEndPoint).Port.ToString()}");

			if (acceptSocket.Connected)
			{
				RemoteIPAddress = ((IPEndPoint)acceptSocket.RemoteEndPoint).Address;
			}
			_socketReceiveAsyncEventArgs.AcceptSocket = acceptSocket;

			_socketSendAsyncEventArgs.AcceptSocket = acceptSocket;

			_socketConnectedEvent.Set();

			StartReceive();
		}

		public void PostMessage(byte[] message)
		{
			lock (_postedMessages)
			{
				_log.Debug(() => $"Enqueueing message for sending {message.ToHexString()}");

				_postedMessages.Add(message);
			}
		}

		public void Close()
		{
			_cancellationTokenSource?.Cancel();
			_cancellationTokenSource = null;

			CloseClientSocket();
		}

		#endregion ICommunicationChannel implementation

		#region Private functions

		private void Receive_Completed(object sender, SocketAsyncEventArgs e)
		{
			if (e.LastOperation == SocketAsyncOperation.Receive)
			{
				_log.Debug($"Receive_Completed from IP {RemoteIPAddress}");
				ProcessReceive();
			}
			else
			{
				throw new ArgumentException("The last operation completed on the socket was not a receive.");
			}
		}

		private void Send_Completed(object sender, SocketAsyncEventArgs e)
		{
			if (e.LastOperation == SocketAsyncOperation.Send)
			{
				_socketSendEvent.Set();
				_log.Info($"Send_Completed to IP {RemoteIPAddress} with status {e.SocketError}");
			}
			else
			{
				throw new ArgumentException("The last operation completed on the socket was not a send");
			}
		}

		private void StartReceive()
		{
			_log.Debug($"Start receive from IP {RemoteIPAddress}");

			if (!_socketReceiveAsyncEventArgs.AcceptSocket.Connected)
			{

			}

			//if (_socketReceiveAsyncEventArgs.AcceptSocket.Connected)
			{
				try
				{
					bool willRaiseEvent = _socketReceiveAsyncEventArgs.AcceptSocket.ReceiveAsync(_socketReceiveAsyncEventArgs);

					if (!willRaiseEvent)
					{
						_log.Debug($"Going to ProcessReceive from IP {RemoteIPAddress}");

						ProcessReceive();
					}
				}
				catch (Exception ex)
				{
					_log.Error($"Failure during StartReceive from IP {RemoteIPAddress}", ex);
					throw;
				}
			}
			//else
			//{
			//    SocketIsDisconnectedException socketIsDisconnectedException = new SocketIsDisconnectedException((IPEndPoint)_socketReceiveAsyncEventArgs.AcceptSocket.RemoteEndPoint);
			//    _log.Error("Unexpected socket disconnection", socketIsDisconnectedException);
			//    throw socketIsDisconnectedException;
			//}
		}

		private void ProcessReceive()
		{
			if (_socketReceiveAsyncEventArgs.SocketError != SocketError.Success)
			{
				_trackingService.TrackMetric("CommunicationErrors", 1);
				_log.Error($"ProcessReceive ended with SocketError={_socketReceiveAsyncEventArgs.SocketError} from IP {RemoteIPAddress}");

				CloseClientSocket();

				return;
			}

			Int32 remainingBytesToProcess = _socketReceiveAsyncEventArgs.BytesTransferred;
			_trackingService.TrackMetric("BytesReceived", remainingBytesToProcess);

			if (_onReceivedExtendedValidation?.Invoke(this, _socketReceiveAsyncEventArgs.RemoteEndPoint as IPEndPoint, remainingBytesToProcess) ?? true)
			{
				if (remainingBytesToProcess > 0)
				{
					_log.Debug($"ProcessReceive from IP {RemoteIPAddress}. remainingBytesToProcess = {remainingBytesToProcess}");

					PushForParsing(_socketReceiveAsyncEventArgs.Buffer, _socketReceiveAsyncEventArgs.Offset, _socketReceiveAsyncEventArgs.BytesTransferred);
				}

				StartReceive();
			}
		}

		private void CloseClientSocket()
		{
			_socketConnectedEvent.Reset();
			_trackingService.TrackMetric("CommunicationChannels", -1);
			_log.Info($"Closing client socket with IP {RemoteIPAddress}");

			try
			{
				_log.Debug($"Trying shutdown Socket with IP {RemoteIPAddress}");
				_socketReceiveAsyncEventArgs?.AcceptSocket?.Shutdown(SocketShutdown.Both);
			}
			catch (Exception ex)
			{
				_log.Warning($"Socket shutdown failed for IP {RemoteIPAddress}", ex);
			}

			_socketReceiveAsyncEventArgs?.AcceptSocket?.Close();

			SocketClosedEvent?.Invoke(this, null);
		}


		private void StartSend()
		{
			Task.Factory.StartNew(() =>
			{
				byte[] currentPostMessage = null;
				int postMessageRemainedBytes = 0;

				foreach (byte[] msg in _postedMessages.GetConsumingEnumerable())
				{
					_socketConnectedEvent.Wait();

					try
					{
						try
						{
							_log.Debug(() => $"Message being sent {msg.ToHexString()}");

							currentPostMessage = _escapeHelper.GetEscapedPacketBytes(msg);

							_log.Debug(() => $"Escaped message being sent {currentPostMessage.ToHexString()}");

							postMessageRemainedBytes = currentPostMessage.Length;
						}
						catch (Exception ex)
						{
							_log.Error("Failed to escape message while sending", ex);
						}

						while (postMessageRemainedBytes > 0)
						{
							int length = _bufferManager.BufferSize;
							if (postMessageRemainedBytes <= _bufferManager.BufferSize)
							{
								length = postMessageRemainedBytes;
							}

							_trackingService.TrackMetric("BytesSent", length);

							_socketSendAsyncEventArgs.SetBuffer(_offsetSend, length);
							Buffer.BlockCopy(currentPostMessage, currentPostMessage.Length - postMessageRemainedBytes, _socketSendAsyncEventArgs.Buffer, _offsetSend, length);

							_log.Debug(() => $"Sending bytes: {_socketSendAsyncEventArgs.Buffer.ToHexString(_socketSendAsyncEventArgs.Offset, length)}");

							_socketSendEvent.Reset();
							bool willRaiseEvent = _socketSendAsyncEventArgs.AcceptSocket.SendAsync(_socketSendAsyncEventArgs);

							if (willRaiseEvent)
							{
								if (!_socketSendEvent.WaitOne(TIMEOUT))
								{
									_trackingService.TrackMetric("CommunicationErrors", 1);
									CloseClientSocket();
									continue;
								}
							}
							else
							{
								_log.Info($"Send_Completed to IP {RemoteIPAddress} with status {_socketSendAsyncEventArgs.SocketError}");
							}

							postMessageRemainedBytes -= _socketSendAsyncEventArgs.BytesTransferred;

							if (_socketSendAsyncEventArgs.SocketError != SocketError.Success)
							{
								_trackingService.TrackMetric("CommunicationErrors", 1);
								CloseClientSocket();
							}
						}
					}
					catch (Exception ex)
					{
						_log.Error($"Failure during StartSend to IP {RemoteIPAddress}", ex);
						CloseClientSocket();
					}
				}
			}, _cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Current);
		}

		#endregion Private functions

		#region Parsing functionality

		private static bool CheckPacketStart(ref bool lastPrevBufByteIsDle, byte[] currentBuf, ref int offset)
		{
			bool packetStartFound = false;
			if (lastPrevBufByteIsDle && currentBuf[0] == Globals.STX)
			{
				packetStartFound = true;
				offset++;
			}
			else
			{
				for (; offset < currentBuf.Length - 1; offset++)
				{
					if (currentBuf[offset] == Globals.DLE && currentBuf[offset + 1] == Globals.STX)
					{
						packetStartFound = true;
						offset += 2;
						break;
					}
				}

				if (!packetStartFound)
				{
					offset++;
					lastPrevBufByteIsDle = currentBuf[currentBuf.Length - 1] == Globals.DLE;
				}
			}

			return packetStartFound;
		}

		private bool TryFetchLength(byte[] buffer, int bufLen, out uint length)
		{
			byte[] lenBytes = new byte[4];
			byte lenFilled = 0;
			bool dle = false;

			for (int i = 0; i < bufLen && lenFilled < 4; i++)
			{
				if (buffer[i] == Globals.DLE)
				{
					dle = true;
				}
				else
				{
					byte v = buffer[i];
					if (dle)
					{
						dle = false;
						v -= Globals.DLE;
					}

					lenBytes[lenFilled++] = v;
				}
			}

			if (lenFilled == 4)
			{
				length = ((uint)lenBytes[3] << 24) + ((uint)lenBytes[2] << 16) + ((uint)lenBytes[1] << 8) + (uint)lenBytes[0];
				return true;
			}

			length = 0;
			return false;
		}

		private bool TryGetPacketLength(ref int offset, byte[] currentBuf, out uint packetLengthExpected, out uint packetLengthRemained, byte[] tempLengthBuf, ref byte tempLengthBufSize)
		{
			packetLengthExpected = 0;
			packetLengthRemained = 0;

			bool lengthIsSet = false;

			do
			{
				if (offset < currentBuf.Length)
				{
					tempLengthBuf[tempLengthBufSize++] = currentBuf[offset++];
				}

			} while (currentBuf.Length > offset && tempLengthBufSize < 8);

			if (tempLengthBufSize == 8)
			{
				lengthIsSet = TryFetchLength(tempLengthBuf, tempLengthBufSize, out packetLengthExpected);

				if (lengthIsSet)
				{
					packetLengthRemained = packetLengthExpected;
				}
			}

			return lengthIsSet;
		}

		#endregion Parsing functionality

		#region IDisposable Support

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_escapeHelper?.Dispose();
				}

				_disposed = true;
			}
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			Dispose(true);
		}

		#endregion
	}
}
