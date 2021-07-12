using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordWorldWebApp.Config
{
    public class WordWorldConfig
    {
        public const string CONFIG_KEY = "wordWorld";

        public Dictionary<string, BoardConfig> Boards { get; set; }
        public Dictionary<string, WordRaterConfig> WordRaters { get; set; }
        public Dictionary<string, LetterBagConfig> LetterBags { get; set; }
        public Dictionary<string, WordSetConfig> WordSets { get; set; }

        public int MinWordLength { get; set; }
        public int PlayersOnLeaderboardCount { get; set; }
        public int LettersPerPlayer { get; set; }

        public TimeSpan PlayerActivityTimeout { get; set; }
        public TimeSpan PlayerActivityCheckInterval { get; set; }
    }
}
