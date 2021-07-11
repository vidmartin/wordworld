using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordWorldWebApp.Config;
using WordWorldWebApp.Game;

namespace WordWorldWebApp.Services
{
    /// <summary>
    /// manages game boards; should be singleton
    /// </summary>
    public class BoardProvider
    {
        private readonly Dictionary<string, Board> _boards = new Dictionary<string, Board>();
        private readonly Dictionary<Board, string> _wordSets = new Dictionary<Board, string>();
        private readonly Dictionary<Board, string> _letterBags = new Dictionary<Board, string>();
        private readonly Dictionary<Board, string> _wordRaters = new Dictionary<Board, string>();
       
        public string DefaultBoardKey { get; set; }

        private class _BoardConfigurer : IBoardConfigurer
        {
            private readonly BoardProvider _parent;
            private readonly Board _board;

            public _BoardConfigurer(BoardProvider parent, Board board)
            {
                _parent = parent;
                _board = board;
            }

            public IBoardConfigurer UseDisplayName(string displayName)
            {
                _board.DisplayName = displayName;

                return this;
            }

            public IBoardConfigurer UseLetterBag(string key)
            {
                _parent._letterBags[_board] = key;

                return this;
            }

            public IBoardConfigurer UseWordRater(string key)
            {
                _parent._wordRaters[_board] = key;

                return this;
            }

            public IBoardConfigurer UseWordSet(string key)
            {
                _parent._wordSets[_board] = key;

                return this;
            }

            public IBoardConfigurer UseConfig(BoardConfig config)
            {
                return this.UseDisplayName(config.DisplayName)
                    .UseLetterBag(config.LetterBag)
                    .UseWordRater(config.WordRater)
                    .UseWordSet(config.WordSet);
            }
        }

        public BoardProvider AddBoard(string key, Board board, Action<IBoardConfigurer> configure)
        {
            _boards[key] = board;

            configure(new _BoardConfigurer(this, board));

            return this;
        }

        public BoardProvider SetDefaultBoard(string defaultBoardKey)
        {
            DefaultBoardKey = defaultBoardKey;

            return this;
        }

        public Board GetBoard(string key)
        {
            return _boards[key];
        }

        public IEnumerable<string> EnumerateBoards()
        {
            return _boards.Keys;
        }

        public string WordSetOf(Board board) => _wordSets[board];
        public string LetterBagOf(Board board) => _letterBags[board];
        public string WordRaterOf(Board board) => _wordRaters[board];
    }
}
