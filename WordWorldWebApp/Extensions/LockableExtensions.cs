using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WordWorldWebApp.Utils;

namespace WordWorldWebApp.Extensions
{
    public static class LockableExtensions
    {
        // TODO: make locking non-blocking somehow?

        public static Task DoAsync(this ILockable self, Action action)
        {
            if (self.Lock != null)
            {
                return self.Lock(action);
            }

            lock (self)
            {
                action();
            }

            return Task.CompletedTask;
        }

        public static Task<T> DoAsync<T>(this ILockable self, Func<T> func)
        {
            T result = default;

            if (self.Lock != null)
            {
                return self.Lock(() => result = func()).ContinueWith(task => result);
            }

            lock(self)
            {
                result = func();
            }

            return Task.FromResult(result);
        }

        public static async Task DoAsyncAsync(this ILockable self, Func<Task> action)
        {
            if (self.Lock != null)
            {
                await self.Lock(async () => await action());
                return;
            }

            try
            {
                Monitor.Enter(self);
                await action();
            }
            finally
            {
                Monitor.Exit(self);
            }
        }

        public static async Task<T> DoAsyncAsync<T>(this ILockable self, Func<Task<T>> func)
        {
            T result = default;

            if (self.Lock != null)
            {
                return await self.Lock(async () => result = await func()).ContinueWith(task => result);
            }

            try
            {
                Monitor.Enter(self);
                return await func();
            }
            finally
            {
                Monitor.Exit(self);
            }
        }
    }
}
