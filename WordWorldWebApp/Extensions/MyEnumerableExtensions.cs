using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordWorldWebApp.Extensions
{
    public static class MyEnumerableExtensions
    {
        public static IDictionary<TKey, int> CountKeys<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keyFunc)
        {
            var dict = new Dictionary<TKey, int>();

            foreach (var item in source)
            {
                var key = keyFunc(item);

                dict[key] = dict.TryGetValue(key, out int i) ? i + 1 : 1;
            }

            return dict;
        }

        public static IDictionary<T, int> CountKeys<T>(this IEnumerable<T> source)
            => CountKeys(source, item => item);
    }
}
