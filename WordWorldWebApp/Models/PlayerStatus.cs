using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordWorldWebApp.Game;

namespace WordWorldWebApp.Models
{
    public class PlayerStatus
    {
        public int Score { get; set; }

        public string Inventory { get; set; }

        public static PlayerStatus From(Player player)
        {
            return new PlayerStatus()
            {
                Score = player.Score,
                Inventory = string.Join("", player.Inventory)                
            };
        }
    }
}
