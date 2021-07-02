using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordWorldWebApp.Services
{
    public class SimpleLetterBag : LetterBag
    {
        public SimpleLetterBag(string letters)
        {
            Letters = letters;
        }

        public string Letters { get; }        

        public override IEnumerable<char> Pull(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return Letters[_RANDOM.Next(Letters.Length)];
            }
        }
    }
}
