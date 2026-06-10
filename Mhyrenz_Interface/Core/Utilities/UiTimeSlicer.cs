using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Mhyrenz_Interface.Core.Utilities
{
    public static class UiTimeSlicer
    {
        public static async Task RunAsync<T>(
            IEnumerable<T> items,
            Action<T> action = null,
            int maxMillisecondsPerSlice = 8,
            DispatcherPriority yieldPriority = DispatcherPriority.Background)
        {
            var stopwatch = Stopwatch.StartNew();

            foreach (var item in items)
            {
                action?.Invoke(item);

                if (stopwatch.ElapsedMilliseconds < maxMillisecondsPerSlice)
                    continue;

                stopwatch.Restart();

                await App.Current.Dispatcher.InvokeAsync(
                    () => { },
                    yieldPriority);
            }
        }
    }
}
