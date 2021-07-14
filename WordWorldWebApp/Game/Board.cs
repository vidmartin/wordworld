using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordWorldWebApp.Utils;

namespace WordWorldWebApp.Game
{
    public abstract class Board : ILockable
    {
        public abstract int Width { get; }
        public abstract int Height { get; }

        public Func<Action, Task> Lock => null;

        public abstract string ReadX(XY pos);
        public abstract string ReadY(XY pos);
        public abstract bool WriteX(XY pos, string s);
        public abstract bool WriteY(XY pos, string s);
        public abstract XY StartOfX(XY pos);
        public abstract XY StartOfY(XY pos);
        public abstract XY EndOfX(XY pos);
        public abstract XY EndOfY(XY pos);
        public abstract bool DeleteX(XY pos);
        public abstract bool DeleteY(XY pos);
        public abstract char At(XY pos);
        public abstract void Set(XY pos, char ch);
    }
}
