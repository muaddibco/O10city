using Microsoft.AspNetCore.SignalR.Client;
using O10.Core.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace O10.Client.Common.Communication
{
    public class SignalrHubConnection
    {
        private readonly Uri _uri;
        private readonly string _context;
        private readonly ILogger _logger;
        private readonly CancellationToken _cancellationToken;
        private readonly Func<Task>? _initAction;
        private HubConnection? _hubConnection;
        
        public SignalrHubConnection(Uri uri, string context, ILogger logger, CancellationToken cancellationToken, Func<Task>? initAction = null)
        {
            _uri = uri;
            _context = context;
            _logger = logger;
            _cancellationToken = cancellationToken;
            _initAction = initAction;
            _cancellationToken.Register(async () =>
            {
                _logger.Info("SignalR connection stopping...");

                try
                {
                    if (_hubConnection != null)
                    {
                        await _hubConnection.StopAsync().ConfigureAwait(false);
                        await _hubConnection.DisposeAsync().ConfigureAwait(false);
                        _hubConnection = null;
                    }

                    _logger.Info("SignalR connection stopped");
                }
                catch (Exception ex)
                {
                    _logger.Error("SignalR connection stopping failed", ex);
                }
            });
        }

        public async Task BuildHubConnection()
        {
            await DestroyHubConnection();

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_uri)
                .WithAutomaticReconnect()
                .Build();

            _logger.Info($"Created instance of hubConnection to URI {_uri}");

            _hubConnection.Closed += OnHubConnectionClose;
            _hubConnection.Reconnecting += OnHubReconnecting;
            _hubConnection.Reconnected += OnHubReconnected;
        }

        private async Task OnHubReconnected(string connectionId)
        {
            _logger.Info($"A new connection {connectionId} established with {_uri}");

            if(_initAction != null)
            {
                await _initAction.Invoke().ConfigureAwait(false);
            }
        }

        private Task OnHubReconnecting(Exception arg)
        {
            if(arg == null)
            {
                _logger.Info($"Reconnecting to {_uri} ...");
            }
            else
            {
                _logger.Error($"Reconnecting to {_uri} due to the error", arg);
            }

            return Task.CompletedTask;
        }

        public async Task StartHubConnection()
        {
            if(_hubConnection == null)
            {
                return;
            }

            _logger.Info($"**** starting {nameof(StartHubConnection)}");
            _logger.Info($"SignalR Hub connection starting...");
            await (await _hubConnection.StartAsync(_cancellationToken).ContinueWith<Task>(async t =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    if (_initAction != null)
                    {
                        await _initAction.Invoke().ConfigureAwait(false);
                    }

                    _logger.Info($"**** SignalR Hub connection started");
                }
                else
                {
                    _logger.Error($"**** Failure during establishing connection with Gateway. Reconnecting...");
                    if (t.Exception != null && t.Exception.InnerExceptions != null)
                    {
                        foreach (Exception exception in t.Exception.InnerExceptions)
                        {
                            _logger.Error($" Failure during establishing connection with Gateway", exception);
                        }
                    }

                    await RetryStartHubConnection().ConfigureAwait(false);
                }
            }, TaskScheduler.Default).ConfigureAwait(false)).ConfigureAwait(false);
            _logger.Info($"SignalR Hub connection starting completed");
        }

        public async Task DestroyHubConnection()
        {
            _logger.LogIfDebug(() => $"Closing hub started...");
            try
            {
                if (_hubConnection != null)
                {
                    _hubConnection.Closed -= OnHubConnectionClose;
                    _hubConnection.Reconnecting -= OnHubReconnecting;
                    _hubConnection.Reconnected -= OnHubReconnected;

                    if (_hubConnection.State != HubConnectionState.Disconnected)
                    {
                        await _hubConnection.StopAsync().ConfigureAwait(false);
                    }

                    await _hubConnection.DisposeAsync().ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Closing hub failed", ex);
            }
            finally
            {
                _logger.LogIfDebug(() => $"Closing hub completed");
            }

            _hubConnection = null;
        }

        public IDisposable On<T1>(string methodName, Action<T1> handler) => _hubConnection.On<T1>(methodName, w => handler(w));

        public async Task AddToGroup(string groupName, CancellationToken cancellationToken)
        {
            await _hubConnection.InvokeAsync("AddToGroup", groupName, cancellationToken);
        }

        public async Task RemoveFromGroup(string groupName, CancellationToken cancellationToken)
        {
            await _hubConnection.InvokeAsync("RemoveFromGroup", groupName, cancellationToken);
        }

        private async Task OnHubConnectionClose(Exception error)
        {
            if (error != null)
            {
                _logger.Error($"!!!! SignalR connection closed with error '{error?.Message}', reconnecting: {!_cancellationToken.IsCancellationRequested}", error);
            }
            else
            {
                _logger.Warning($"SignalR connection closed intentionally from the server side");
            }

            if (!_cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(new Random().Next(0, 5) * 1000, _cancellationToken).ConfigureAwait(false);

                if (!_cancellationToken.IsCancellationRequested)
                {
                    await StartHubConnection().ConfigureAwait(false);
                }
            }
        }

        private async Task RetryStartHubConnection()
        {
            await BuildHubConnection();

            await Task.Delay(1000).ConfigureAwait(false);
            await StartHubConnection().ConfigureAwait(false);
        }
    }
}
