using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WordWorldWebApp.Utils;

namespace WordWorldWebApp.Extensions
{
    public static class LockableExtensions
    {
        // TODO: better way?

        public static Task DoAsync(this ILockable self, Action action)
        {
            var source = new TaskCompletionSource();

            Task.Run(() =>
            {
                try
                {
                    lock (self) { action(); }

                    source.SetResult();
                }
                catch (Exception ex)
                {
                    source.SetException(ex);
                }
            });

            return source.Task;
        }

        public static Task<T> DoAsync<T>(this ILockable self, Func<T> func)
        {
            var source = new TaskCompletionSource<T>();

            Task.Run(() =>
            {
                try
                {
                    T result = default;
                    lock (self) { result = func(); }

                    source.SetResult(result);
                }
                catch (Exception ex)
                {
                    source.SetException(ex);
                }
            });

            return source.Task;
        }

        public static Task<T> DoAsyncAsync<T>(this ILockable self, Func<Task<T>> func)
        {
            var source = new TaskCompletionSource<T>();

            Task.Run(async () =>
            {
                try
                {
                    Task<T> result = default;
                    lock (self) { result = func(); }

                    source.SetResult(await result);
                }
                catch (Exception ex)
                {
                    source.SetException(ex);
                }
            });

            return source.Task;
        }
    }
}
