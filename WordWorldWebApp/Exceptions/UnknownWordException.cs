using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordWorldWebApp.Exceptions
{
    public class UnknownWordException : Exception
    {
        public UnknownWordException(bool wasIntentional, string word, int x, int y, string direction)
        {
            WasIntentional = wasIntentional;
            Word = word;
            X = x;
            Y = y;
            Direction = direction;
        }

        public bool WasIntentional { get; set; }

        public string Word { get; set; }

        public int X { get; set; }

        public int Y { get; set; }

        public string Direction { get; set; }

    }
}
