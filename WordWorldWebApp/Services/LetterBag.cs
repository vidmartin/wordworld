using System;
using System.Collections.Generic;

namespace WordWorldWebApp.Services
{
    public abstract class LetterBag
    {
        public IServiceProvider ServiceProvider { get; set; }

        public abstract IEnumerable<char> Pull(int count);

        protected static readonly Random _RANDOM = new Random();

    }
}