using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordWorldWebApp.Game
{
    public struct XY
    {
        public int x;
        public int y;

        public XY(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static implicit operator XY((int, int) tuple)
        {
            return new XY(tuple.Item1, tuple.Item2);
        }

        public override bool Equals(object obj)
        {
            if (obj is XY other)
            {
                return this == other;
            }

            return false;
        }

        public static bool operator ==(XY a, XY b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(XY a, XY b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }
    }
}
