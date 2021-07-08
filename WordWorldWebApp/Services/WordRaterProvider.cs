using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordWorldWebApp.Services
{
    public class WordRaterProvider
    {
        private readonly Dictionary<string, WordRater> _wordRaters = new();

        public WordRaterProvider AddWordRater(string key, WordRater rater)
        {
            _wordRaters[key] = rater;
            return this;
        }

        public WordRater GetWordRater(string key)
        {
            return _wordRaters[key];
        }
    }
}
