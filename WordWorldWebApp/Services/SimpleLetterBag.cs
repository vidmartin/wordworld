using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordWorldWebApp.Config;

namespace WordWorldWebApp.Services
{
    public class SimpleLetterBag : LetterBag
    {      
        public SimpleLetterBag UseLetters(string letters)
        {
            this.Letters = letters;

            return this;
        }

        public SimpleLetterBag UseConfig(LetterBagConfig config)
        {
            return this.UseLetters(config.Letters);
        }

        public string Letters { get; set; }        

        public override IEnumerable<char> Pull(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return Letters[_RANDOM.Next(Letters.Length)];
            }
        }
    }
}
