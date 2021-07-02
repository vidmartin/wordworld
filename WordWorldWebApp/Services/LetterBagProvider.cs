using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordWorldWebApp.Game;

namespace WordWorldWebApp.Services
{
    public class LetterBagProvider
    {
        public LetterBagProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private readonly Dictionary<string, LetterBag> _letterBags = new Dictionary<string, LetterBag>();
        private readonly IServiceProvider _serviceProvider;

        public LetterBagProvider AddLetterBag(string key, LetterBag letterBag)
        {
            _letterBags[key] = letterBag;

            return this;
        }

        public LetterBagProvider AddLetterBag(string key, Func<IServiceProvider, LetterBag> letterBagFactory)
        {
            _letterBags[key] = letterBagFactory(_serviceProvider);

            return this;
        }

        public LetterBag GetLetterBag(string key)
        {
            var bag = _letterBags[key];
            bag.ServiceProvider = _serviceProvider;

            return bag;
        }
    }
}
