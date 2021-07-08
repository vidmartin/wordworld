using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordWorldWebApp.Utils;

namespace WordWorldWebApp.Game
{
    public abstract class Player
    {
        public abstract string Token { get; }

        public abstract Board Board { get; }

        public abstract DateTime Created { get; }

        public DateTime LastAction { get; set; }

        protected char[] _inventory;        

        public IEnumerable<char> Inventory => _inventory;

        public bool ReplaceLetters(ISet<int> oldIndices, IEnumerable<char> newLetters)
        {
            if (oldIndices.Any(i => i < 0 || i >= _inventory.Length))
            {
                return false;
            }

            _inventory = _inventory.Index()
                .Where(curr => !oldIndices.Contains(curr.index))
                .Select(curr => curr.item)
                .ToArray();

            _inventory = _inventory
                .Concat(newLetters)
                .ToArray();

            return true;
        }

        public bool ReplaceLetters(int[] oldIndices, IEnumerable<char> newLetters)
        {
            return ReplaceLetters(oldIndices.ToHashSet(), newLetters);
        }

        public int Score { get; set; } = 0;       
        
        public abstract string Username { get; }
    }
}
