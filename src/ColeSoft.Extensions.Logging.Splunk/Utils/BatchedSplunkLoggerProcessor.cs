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
        private readonly ConcurrentBag<string> events = new ConcurrentBag<string>();
        private readonly Timer timer;
        private readonly Func<IReadOnlyList<string>, Task> emitAction;
        private readonly IDisposable optionsReloadToken;
        private SplunkLoggerOptions currentOptions;

        private bool isDisposed;
        private bool isDisposing;

        public BatchedSplunkLoggerProcessor(IOptionsMonitor<SplunkLoggerOptions> options, Func<IReadOnlyList<string>, Task> emitAction)
        {
            this.emitAction = emitAction ?? throw new ArgumentNullException(nameof(emitAction));

            currentOptions = options.CurrentValue;
            timer = options.CurrentValue.BatchInterval > 0
                ? new Timer(EmitTimeCheck, null, 0, options.CurrentValue.BatchInterval)
                : new Timer(EmitTimeCheck, null, 0, Timeout.Infinite);

            optionsReloadToken = options.OnChange(o =>
            {
                timer.Change(0, options.CurrentValue.BatchInterval > 0 ? options.CurrentValue.BatchInterval : Timeout.Infinite);
                currentOptions = options.CurrentValue;
            });
        }

        public void EnqueueMessage(string message)
        {
            if (!isDisposed && !isDisposing)
            {
                events.Add(message);
                if (events.Count >= currentOptions.BatchSize)
                {
                    Emit();
                }
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposing = true;
                while (events.Count != 0)
                {
                    Emit();
                }

                timer.Dispose();
                optionsReloadToken.Dispose();

                isDisposing = false;
                isDisposed = true;
            }
        }

        private void EmitTimeCheck(object state)
        {
            if (events.Count > 0)
            {
                Emit();
            }
        }

        private void Emit()
        {
            Task.Run(
                async () =>
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
                            events.TryTake(out var item);
                            if (item != null)
                            {
                                emitEvents.Add(item);
                            }

                            if (events.Count == 0 || emitEvents.Count >= currentOptions.BatchSize)
                            {
                                continueExtraction = false;
                            }
                        }
                    }

                    if (emitEvents.Count > 0)
                    {
                        await emitAction(emitEvents);
                    }
                });
        }
    }
}
