using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordWorldWebApp.Utils
{
    public static class StrUtils
    {
        public static int[] GetIndices(this IEnumerable<char> inventory, IEnumerable<char> word)
        {
            return GetIndices(inventory, word.ToHashSet());
        }

        public static bool TryGetIndices(this IEnumerable<char> inventory, ISet<char> word, out int[] result)
        {
            List<int> indices = new List<int>();

            foreach (var curr in inventory.Index())
            {
                if (word.Contains(curr.item))
                {
                    word.Remove(curr.item);
                    indices.Add(curr.index);
                }
            }

            if (word.Any())
            {
                // we coultn't built the whole word => not succesful
                result = null;
                return false;
            }
            else
            {
                result = indices.ToArray();
                return true;
            }
        }
    }
}
