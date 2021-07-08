using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordWorldWebApp.Utils
{
    public static class TryHard
    {
        public static bool Try<T>(Func<T> func, out T result, Predicate<Exception> exceptionsToCatch = null)
        {
            try
            {
                result = func();
                return true;
            }
            catch (Exception ex)
            {
                if (exceptionsToCatch?.Invoke(ex) == false)
                {
                    throw;
                }

                result = default;
                return false;
            }
        }

        public struct TryResult<T>
        {
            public bool success;
            public T result;

            public TryResult(bool success, T result)
            {
                this.success = success;
                this.result = result;
            }
        }

        public static async Task<TryResult<T>> TryAsync<T>(Func<Task<T>> func, Predicate<Exception> exceptionsToCatch = null)
        {
            try
            {
                return new TryResult<T>(true, await func());
            }
            catch (Exception ex)
            {
                if (exceptionsToCatch?.Invoke(ex) == false)
                {
                    throw;
                }

                return new TryResult<T>(false, default);
            }
        }

        public static Task<TryResult<T>> TryAsync<T>(Task<T> func, Predicate<Exception> exceptionsToCatch = null)
        {
            return TryAsync(() => func, exceptionsToCatch);
        }
    }
}
