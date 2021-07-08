using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordWorldWebApp.DataStructures;
using WordWorldWebApp.Game;

namespace WordWorldWebApp.Models
{
    public class PlayModel
    {
        public string Token { get; set; }

        public PlayerStatus PlayerStatus { get; set; }

        public string BoardArray { get; set; }

        public Rect BoardRect { get; set; }

        public Vec2i Origin { get; set; }

        public Vec2i BoardSize { get; set; }

        public Dictionary<char, int> CharactersWithScores { get; set; }

        public string Language { get; set; }
    }
}
