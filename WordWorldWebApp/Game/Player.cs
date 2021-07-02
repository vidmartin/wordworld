using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordWorldWebApp.Game
{
    public abstract class Player
    {
        public abstract string Token { get; }

        public abstract Board Board { get; }

        public abstract DateTime Created { get; }

        public DateTime LastAction { get; set; }

        protected List<char> _inventory;        

        public IEnumerable<char> Inventory => _inventory;

        /// <summary>
        /// if all 'oldLetters' exist in the player's inventory, they will be replaced with 'newLetters' and true will be
        /// returned. otherwise, false will be returned.
        /// </summary>
        public bool ReplaceLetters(IEnumerable<char> oldLetters, IEnumerable<char> newLetters)
        {
            if (!oldLetters.All(_inventory.Contains))
            {
                return false;
            }

            foreach (char ch in oldLetters)
            {
                _inventory.Remove(ch);
            }

            foreach (char ch in newLetters)
            {
                _inventory.Add(ch);
            }

            return true;
        }

        public int Score { get; set; } = 0;        
    }
}
