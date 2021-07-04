using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordWorldWebApp.Utils
{
    public static class MyLinq
    {
        public static IEnumerable<(T item, int index)> Index<T>(this IEnumerable<T> self)
        {
            int i = 0;
            foreach (T item in self)
            {
                try
                {
                    yield return (item, i);
                }
                finally
                {
                    i += 1;
                }
            }
        }
    }
}
