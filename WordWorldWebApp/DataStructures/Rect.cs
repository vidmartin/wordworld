using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordWorldWebApp.DataStructures
{
    public record Rect(int x, int y, int w, int h)
    {
        public static implicit operator Rect((int, int, int, int) tuple)
        {
            return new Rect(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
        }
    }
}
