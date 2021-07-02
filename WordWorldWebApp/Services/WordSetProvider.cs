using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordWorldWebApp.Services
{
    public class WordSetProvider
    {
        private readonly Dictionary<string, WordSet> _wordSets = new Dictionary<string, WordSet>();

        public WordSetProvider AddWordSet(string key, WordSet wordSet)
        {
            _wordSets[key] = wordSet;

            return this;
        }

        public WordSet GetWordSet(string key)
        {
            return _wordSets[key];
        }
    }
}
