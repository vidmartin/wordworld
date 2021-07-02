using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordWorldWebApp.Exceptions
{
    public class WordTooShortException : Exception
    {
        public WordTooShortException(string word)
        {
            Word = word;
        }

        public string Word { get; set; }
    }
}
