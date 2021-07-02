using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace WordWorldWebApp.Services
{
    public class WordSetLetterBag : LetterBag
    {
        public WordSetLetterBag(string wordSet)
        {
            WordSet = wordSet;
        }

        public string WordSet { get; }

        public override IEnumerable<char> Pull(int count)
        {
            for (int i = 0; i < count; i++)
            {
                string word = ServiceProvider.GetService<WordSetProvider>().GetWordSet(WordSet).RandomWord();

                yield return word[_RANDOM.Next(word.Length)];
            }
        }
    }
}
