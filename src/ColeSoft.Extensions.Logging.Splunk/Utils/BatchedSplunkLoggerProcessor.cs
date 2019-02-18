using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace ColeSoft.Extensions.Logging.Splunk.Utils
{
    internal sealed class BatchedSplunkLoggerProcessor : IDisposable
    {
        private readonly ConcurrentQueue<string> events = new ConcurrentQueue<string>();
        private readonly AutoResetEvent wh = new AutoResetEvent(false);
        private readonly AutoResetEvent complete = new AutoResetEvent(false);
        private readonly Func<IReadOnlyList<string>, Task> emitAction;
        private readonly IDisposable optionsReloadToken;

        private SplunkLoggerOptions currentOptions;
        private bool isDisposed;
        private bool isDisposing;

        public BatchedSplunkLoggerProcessor(IOptionsMonitor<SplunkLoggerOptions> options, Func<IReadOnlyList<string>, Task> emitAction)
        {
            this.emitAction = emitAction ?? throw new ArgumentNullException(nameof(emitAction));

            currentOptions = options.CurrentValue;
            optionsReloadToken = options.OnChange(
                o =>
                {
                    currentOptions = options.CurrentValue;
                });

            Task.Factory.StartNew(EmitAsync, TaskCreationOptions.LongRunning);
        }

        public void EnqueueMessage(string message)
        {
            if (!isDisposed && !isDisposing)
            {
                events.Enqueue(message);
                if (events.Count >= currentOptions.BatchSize)
                {
                    wh.Set();
                }
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposing = true;

                wh.Set();
                complete.WaitOne();

                optionsReloadToken.Dispose();
                wh.Dispose();
                complete.Dispose();

                isDisposing = false;
                isDisposed = true;
            }
        }

        private async Task EmitAsync()
        {
            List<string> GatherEvents()
            {
                var continueExtraction = true;
                var emitEvents = new List<string>();
                while (continueExtraction)
                {
                    if (events.Count == 0)
                    {
                        continueExtraction = false;
                    }
                    else
                    {
                        if (events.TryDequeue(out var item))
                        {
                            emitEvents.Add(item);
                        }

                        if (events.Count == 0 || emitEvents.Count >= currentOptions.BatchSize)
                        {
                            continueExtraction = false;
                        }
                    }
                }

                return emitEvents;
            }

            while (true)
            {
                var emitEvents = GatherEvents();

                if (emitEvents.Count > 0)
                {
                    await emitAction(emitEvents);
                }

                if (!isDisposing && events.Count < currentOptions.BatchSize)
                {
                    wh.WaitOne(currentOptions.BatchInterval != 0 ? currentOptions.BatchInterval : -1);
                }
                else if (isDisposing && events.Count == 0)
                {
                    break;
                }
            }

            complete.Set();
        }
    }
}
