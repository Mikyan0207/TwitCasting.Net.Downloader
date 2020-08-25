using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TwitCasting.Net.Downloader.EventArgs;

namespace TwitCasting.Net.Downloader.Common
{
    public sealed class AsyncEvent
    {
        private readonly object _lock = new object();

        private List<AsyncEventHandler> Handlers { get; }

        private Action<string, Exception> ErrorHandler { get; }

        private string EventName { get; }

        public AsyncEvent(Action<string, Exception> errorHandler, string eventName)
        {
            ErrorHandler = errorHandler;
            EventName = eventName;
            Handlers = new List<AsyncEventHandler>();
        }

        public void Register(AsyncEventHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "Event Handler cannot be null.");

            lock (_lock)
                Handlers.Add(handler);
        }

        public void Unregister(AsyncEventHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "Event Handler cannot be null.");

            lock (_lock)
                Handlers.Remove(handler);
        }

        public async Task InvokeAsync()
        {
            AsyncEventHandler[] handlers = null;

            lock (_lock)
                handlers = Handlers.ToArray();

            if (!handlers.Any())
                return;

            foreach (var handler in handlers)
            {
                try
                {
                    await handler().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    ErrorHandler(EventName, e);
                }
            }
        }
    }

    public sealed class AsyncEvent<T> where T : AsyncEventArgs
    {
        private readonly object _lock = new object();

        private List<AsyncEventHandler<T>> Handlers { get; }

        private Action<string, Exception> ErrorHandler { get; }

        private string EventName { get; }

        public AsyncEvent(Action<string, Exception> errorHandler, string eventName)
        {
            ErrorHandler = errorHandler;
            EventName = eventName;
            Handlers = new List<AsyncEventHandler<T>>();
        }

        public void Register(AsyncEventHandler<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "Event Handler cannot be null.");

            lock (_lock)
                Handlers.Add(handler);
        }

        public void Unregister(AsyncEventHandler<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "Event Handler cannot be null.");

            lock (_lock)
                Handlers.Remove(handler);
        }

        public async Task InvokeAsync(T e)
        {
            AsyncEventHandler<T>[] handlers = null;

            lock (_lock)
                handlers = Handlers.ToArray();

            if (!handlers.Any())
                return;

            var exceptions = new List<Exception>(handlers.Length);

            foreach (var handler in handlers)
            {
                try
                {
                    var task = handler(e);

                    if (task != null)
                        await task.ConfigureAwait(false);

                    if (e.Handled)
                        break;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
            {
                ErrorHandler(EventName,
                    new AggregateException(
                        "Exceptions occured within one or more event handlers. Check InnerExceptions for details.",
                        exceptions));
            }
        }
    }
}