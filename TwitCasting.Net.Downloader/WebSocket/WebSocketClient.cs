using System;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitCasting.Net.Downloader.EventArgs;

namespace TwitCasting.Net.Downloader.WebSocket
{
    internal class WebSocketClient
    {
        private const int OutgoingChunkSize = 8192; // 8 KiB
        private const int IncomingChunkSize = 32768; // 32 KiB

        public IWebSocketOptions Options { get; }

        public WebSocketState State => ClientWebSocket.State;

        private ClientWebSocket ClientWebSocket { get; set; }

        private CancellationTokenSource SocketTokenSource { get; set; }
        private CancellationToken SocketToken { get; set; }

        private CancellationTokenSource ReceiverTokenSource { get; set; }
        private CancellationToken ReceiverToken { get; set; }

        private SemaphoreSlim Locker { get; }

        private Task ReadTask { get; set; }

        private volatile bool IsClientClosed = false;
        private volatile bool IsConnected = false;
        private volatile bool IsDisposed = false;

        private WebSocketClient(IWebSocketOptions options)
        {
            _connected = new AsyncEvent(EventErrorHandler, "WS_CONNECT");
            _disconnected = new AsyncEvent<WebSocketCloseEventArgs>(EventErrorHandler, "WS_DISCONNECT");
            _messageReceived = new AsyncEvent<WebSocketMessageEventArgs>(EventErrorHandler, "WS_MESSAGE");
            _errored = new AsyncEvent<WebSocketErrorEventArgs>(EventErrorHandler, "WS_ERROR");

            Options = options;
            ClientWebSocket = new ClientWebSocket
            {
                Options =
                {
                    KeepAliveInterval = TimeSpan.Zero
                }
            };

            if (options.WebProxy != null)
                ClientWebSocket.Options.Proxy = options.WebProxy;

            if (options.Headers.Any())
                foreach (var (k, v) in options.Headers)
                    ClientWebSocket.Options.SetRequestHeader(k, v);

            SocketTokenSource = null;
            SocketToken = CancellationToken.None;
            ReceiverTokenSource = null;
            ReceiverToken = CancellationToken.None;
            Locker = new SemaphoreSlim(1);
        }

        public static WebSocketClient CreateNew(IWebSocketOptions options)
        {
            return new WebSocketClient(options);
        }

        public bool AddDefaultHeader(string name, string value)
        {
            Options.Headers[name] = value;
            return true;
        }

        public bool RemoveDefaultHeader(string name)
            => Options.Headers.Remove(name);

        public async Task ConnectAsync(System.Uri uri)
        {
            try
            {
                await DisconnectAsync().ConfigureAwait(false);
            }
            catch
            {
                // ignored
            }

            await Locker.WaitAsync().ConfigureAwait(false);

            try
            {
                SocketTokenSource?.Dispose();
                ReceiverTokenSource?.Dispose();

                SocketTokenSource = new CancellationTokenSource();
                SocketToken = SocketTokenSource.Token;
                ReceiverTokenSource = new CancellationTokenSource();
                ReceiverToken = ReceiverTokenSource.Token;

                IsClientClosed = false;

                ClientWebSocket?.Dispose();
                ClientWebSocket = new ClientWebSocket
                {
                    Options =
                    {
                        KeepAliveInterval = TimeSpan.Zero,
                    }
                };

                if (Options.WebProxy != null)
                    ClientWebSocket.Options.Proxy = Options.WebProxy;

                if (Options.Headers.Any())
                    foreach (var (k, v) in Options.Headers)
                        ClientWebSocket.Options.SetRequestHeader(k, v);

                await ClientWebSocket.ConnectAsync(uri, SocketToken).ConfigureAwait(false);
                ReadTask = Task.Run(ReadAsync, ReceiverToken);
            }
            finally
            {
                Locker.Release();
            }
        }

        public async Task DisconnectAsync(int code = 1000, string message = "")
        {
            // ReSharper disable once MethodSupportsCancellation
            await Locker.WaitAsync().ConfigureAwait(false);

            try
            {
                IsClientClosed = true;

                if (ClientWebSocket != null)
                    await ClientWebSocket.CloseOutputAsync((WebSocketCloseStatus)code, message, CancellationToken.None)
                        .ConfigureAwait(false);

                if (ReadTask != null)
                    await ReadTask.ConfigureAwait(false);

                if (IsConnected)
                    IsConnected = false;

                SocketTokenSource?.Cancel();
                SocketTokenSource?.Dispose();

                ReceiverTokenSource?.Cancel();
                ReceiverTokenSource?.Dispose();
            }
            catch
            {
                // ignored
            }
            finally
            {
                Locker.Release();
            }
        }

        public async Task SendMessageAsync(string message)
        {
            if (ClientWebSocket == null)
                return;

            var bytes = Encoding.UTF8.GetBytes(message);

            await Locker.WaitAsync().ConfigureAwait(false);

            try
            {
                var len = bytes.Length;
                var segments = len / OutgoingChunkSize;

                if (len % OutgoingChunkSize != 0)
                    segments++;

                for (var i = 0; i < segments; i++)
                {
                    var start = OutgoingChunkSize + i;
                    var length = Math.Min(OutgoingChunkSize, len - start);

                    await ClientWebSocket.SendAsync(new ArraySegment<byte>(bytes, start, length),
                        WebSocketMessageType.Text, i == segments - 1, CancellationToken.None).ConfigureAwait(false);
                }
            }
            finally
            {
                Locker.Release();
            }
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            DisconnectAsync().GetAwaiter().GetResult();

            SocketTokenSource.Dispose();
            ReceiverTokenSource.Dispose();
        }

        internal async Task ReadAsync()
        {
            await Task.Yield();

            var buffer = new ArraySegment<byte>(new byte[IncomingChunkSize]);

            try
            {
                await using var stream = new MemoryStream();
                while (!ReceiverToken.IsCancellationRequested)
                {
                    WebSocketReceiveResult result;

                    do
                    {
                        result = await ClientWebSocket.ReceiveAsync(buffer, CancellationToken.None)
                            .ConfigureAwait(false);

                        if (result.MessageType == WebSocketMessageType.Close)
                            break;

                        stream.Write(buffer.Array, 0, result.Count);
                    }
                    while (!result.EndOfMessage);

                    var resultBytes = new byte[stream.Length];
                    stream.Position = 0;
                    stream.Read(resultBytes, 0, resultBytes.Length);
                    stream.Position = 0;
                    stream.SetLength(0);

                    if (!IsConnected)
                    {
                        IsConnected = true;
                        await _connected.InvokeAsync().ConfigureAwait(false);
                    }

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        using var reader = new StreamReader(stream, Encoding.UTF8);

                        await _messageReceived
                            .InvokeAsync(
                                new WebSocketTextMessageEventArgs(Encoding.UTF8.GetString(resultBytes)))
                            .ConfigureAwait(false);
                    }
                    else if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        await _messageReceived.InvokeAsync(new WebSocketBinaryMessageEventArgs(resultBytes))
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        if (!IsClientClosed)
                        {
                            // ReSharper disable once PossibleInvalidOperationException
                            var code = result.CloseStatus.Value;
                            code = code == WebSocketCloseStatus.NormalClosure ||
                                   code == WebSocketCloseStatus.EndpointUnavailable
                                ? (WebSocketCloseStatus)4000
                                : code;

                            await ClientWebSocket
                                .CloseOutputAsync(code, result.CloseStatusDescription, CancellationToken.None)
                                .ConfigureAwait(false);
                        }

                        await _disconnected.InvokeAsync(new WebSocketCloseEventArgs((int)(result.CloseStatus ?? 0), result.CloseStatusDescription)).ConfigureAwait(false);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                await _errored.InvokeAsync(new WebSocketErrorEventArgs(ex)).ConfigureAwait(false);
                await _disconnected.InvokeAsync(new WebSocketCloseEventArgs(-1, "")).ConfigureAwait(false);
            }

            _ = DisconnectAsync().ConfigureAwait(false);
        }

        #region AsyncEvents

        public event AsyncEventHandler Connected
        {
            add => _connected.Register(value);
            remove => _connected.Unregister(value);
        }

        private readonly AsyncEvent _connected;

        public event AsyncEventHandler<WebSocketCloseEventArgs> Disconnected
        {
            add => _disconnected.Register(value);
            remove => _disconnected.Unregister(value);
        }

        private readonly AsyncEvent<WebSocketCloseEventArgs> _disconnected;

        public event AsyncEventHandler<WebSocketMessageEventArgs> MessageReceived
        {
            add => _messageReceived.Register(value);
            remove => _messageReceived.Unregister(value);
        }

        private readonly AsyncEvent<WebSocketMessageEventArgs> _messageReceived;

        public event AsyncEventHandler<WebSocketErrorEventArgs> Errored
        {
            add => _errored.Register(value);
            remove => _errored.Unregister(value);
        }

        private readonly AsyncEvent<WebSocketErrorEventArgs> _errored;

        private void EventErrorHandler(string evname, Exception ex)
        {
            if (evname.ToLowerInvariant() == "ws_error")
                Console.WriteLine($"WebSocket Error: {ex.GetType()} in {evname}!");
            else
                _errored.InvokeAsync(new WebSocketErrorEventArgs(ex)).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        #endregion AsyncEvents
    }
}