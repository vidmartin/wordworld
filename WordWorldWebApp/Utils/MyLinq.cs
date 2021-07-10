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

        public static IEnumerable<T> FillIn<T>(this IEnumerable<T> self, Predicate<T> condition, IEnumerable<T> other)
        {
            using (var otherEnumerator = other.GetEnumerator())
            {
                foreach (T item in self)
                {
                    if (condition(item))
                    {
                        if (otherEnumerator.MoveNext())
                        {
                            yield return otherEnumerator.Current;
                            continue;
                        }
                        else
                        {
                            throw new ArgumentException("the amount of items in other sequence is less than the amount of matching items in this sequence");
                        }
                    }

                    yield return item;
                }
            }
        }

        public static string Stringify<T>(this IEnumerable<T> self, string joinWith = "")
        {
            return string.Join<T>(joinWith, self);
        }
    }
}
