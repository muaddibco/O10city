using Microsoft.AspNetCore.SignalR.Client;
using O10.Core.Logging;
using System;
using System.Collections.Generic;
using System.Text;
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
        private HubConnection _hubConnection;
        
        public SignalrHubConnection(Uri uri, string context, ILogger logger, CancellationToken cancellationToken)
        {
            _uri = uri;
            _context = context;
            _logger = logger;
            _cancellationToken = cancellationToken;
            _cancellationToken.Register(async () =>
            {
                _logger.Info($"[{_context}]: SignalR connection stopping...");

                try
                {
                    await _hubConnection.StopAsync().ConfigureAwait(false);
                    await _hubConnection.DisposeAsync().ConfigureAwait(false);
                    _hubConnection = null;
                    _logger.Info($"[{_context}]: SignalR connection stopped");
                }
                catch (Exception ex)
                {
                    _logger.Error($"[{_context}]: SignalR connection stopping failed", ex);
                }
            });
        }

        public IDisposable On<T1>(string methodName, Action<T1> handler) => _hubConnection.On<T1>(methodName, w => handler(w));

        public async Task BuildHubConnection()
        {
            await DestroyHubConnection();

            _hubConnection = new HubConnectionBuilder().WithUrl(_uri).Build();

            _logger.Info($"[{_context}]: created instance of hubConnection to URI {_uri}");

            _hubConnection.Closed += OnHubConnectionClose;

        }

        public async Task StartHubConnection()
        {
            _logger.Info($"[{_context}]: **** starting {nameof(StartHubConnection)}");
            _logger.Info($"[{_context}]: SignalRPacketsProvider hubConnection connecting...");
            await (await _hubConnection.StartAsync(_cancellationToken).ContinueWith<Task>(async t =>
            {
                if (t.IsCompletedSuccessfully)
                {
                    _logger.Info($"[{_context}]: **** SignalRPacketsProvider hubConnection connected");
                }
                else
                {
                    _logger.Error($"[{_context}]: **** Failure during establishing connection with Gateway. Reconnecting...");
                    if (t.Exception != null && t.Exception.InnerExceptions != null)
                    {
                        foreach (Exception exception in t.Exception.InnerExceptions)
                        {
                            _logger.Error($"[{_context}]: Failure during establishing connection with Gateway", exception);
                        }
                    }

                    await RetryStartHubConnection().ConfigureAwait(false);
                }
            }, TaskScheduler.Default).ConfigureAwait(false)).ConfigureAwait(false);
            _logger.Info($"[{_context}]: SignalRPacketsProvider hubConnection completed");
        }

        public async Task DestroyHubConnection()
        {
            _logger.LogIfDebug(() => $"[{_context}]: closing hub started...");
            try
            {
                if (_hubConnection != null)
                {
                    _hubConnection.Closed -= OnHubConnectionClose;

                    if (_hubConnection.State != HubConnectionState.Disconnected)
                    {
                        await _hubConnection.StopAsync();
                    }

                    await _hubConnection.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"[{_context}]: closing hub failed", ex);
            }
            finally
            {
                _logger.LogIfDebug(() => $"[{_context}]: closing hub completed");
            }

            _hubConnection = null;
        }

        private async Task OnHubConnectionClose(Exception error)
        {
            _logger.Error($"[{_context}]: !!!! SignalRPacketsProvider hubConnection closed with error '{error?.Message}', reconnecting: {!_cancellationToken.IsCancellationRequested}", error);

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
