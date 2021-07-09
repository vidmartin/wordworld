using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordWorldWebApp.Exceptions;
using WordWorldWebApp.Extensions;
using WordWorldWebApp.Game;

namespace WordWorldWebApp.Services
{
    public class MoveChecker
    {
        private readonly WordSetProvider _wordSetProvider;
        private readonly BoardProvider _boardProvider;

        public MoveChecker(
            WordSetProvider wordSetProvider,
            BoardProvider boardProvider)
        {
            _wordSetProvider = wordSetProvider;
            _boardProvider = boardProvider;
        }

        public void AssertMoveValidUnsafe(Player player, XY position, string direction, string word, out char[] placedLetters)
        {           
            var board = player.Board;
            var wordSet = _wordSetProvider.GetWordSet(_boardProvider.WordSetOf(board));
            var stepper = GetStepper(direction);
            List<char> placed = new(); // keep track of how many letters were placed by player

            // some characters might come before the current word
            string precedingWord = "";
            XY precedingPos = stepper(position, -1);
           
            if (board.At(precedingPos) != ' ')
            {
                char temp = board.At(position);

                try
                {
                    board.Set(position, ' ');

                    precedingWord = GetActualWord(direction, board, precedingPos).Item2;
                }
                finally
                {
                    board.Set(position, temp);
                }
            }

            // some characters might come after the current word
            string succeedingWord = "";
            XY succeedingPos = stepper(position, word.Length);

            if (board.At(succeedingPos) != ' ')
            {
                succeedingWord = Read(board, direction, succeedingPos);
            }

            string actualWord = precedingWord + word + succeedingWord;

            // word must be at least 3 characters long
            if (actualWord.Length < 3)
            {
                throw new WordTooShortException(actualWord);
            }

            // word must be in current word set
            if (!wordSet.ContainsWord(actualWord))
            {
                throw new UnknownWordException(true, actualWord, position.x, position.y, direction);
            }

            for (int i = 0; i < word.Length; i++)
            {
                var pos = stepper(position, i);

                char curr = board.At(pos);
                
                if (curr == ' ')
                {                   
                    try
                    {
                        board.Set(pos, word[i]); // temporarily change character at current position to test unintentionally created words

                        // TODO: unintentional score
                        (XY unintentionalWordStart, string unintentionalWord) = GetActualWord(GetOtherDirection(direction), board, pos);
                        
                        if (unintentionalWord.Length > 1 && !wordSet.ContainsWord(unintentionalWord))
                        {
                            // another word is created unintentionally (perpendicular to tested word) and isn't known, that is not allowed
                            throw new UnknownWordException(false, unintentionalWord, unintentionalWordStart.x, unintentionalWordStart.y, GetOtherDirection(direction));
                        }


                        if (!chdict.ContainsKey(word[i]))
                        {
                            // TODO: this probably isn't required anymore, inventory checking is now handled by CheckUsedLetterIndices
                            throw new LetterNotInInventoryException();
                        }

                        if (--chdict[word[i]] == 0)
                        {
                            chdict.Remove(word[i]);
                        }

                        placed.Add(word[i]);

                        continue;
                    }
                    finally
                    {
                        board.Set(pos, curr);
                    }
                }

                if (curr != word[i])
                {
                    throw new InvalidPlacementException(pos.x, pos.y);
                }
            }

            placedLetters = placed.ToArray();            
        }

        private Func<XY, int, XY> GetStepper(string direction)
        {
            return direction switch
            {
                "x" => new Func<XY, int, XY>((pos, i) => (pos.x + i, pos.y)),
                "y" => new Func<XY, int, XY>((pos, i) => (pos.x, pos.y + i)),

                _ => throw new ArgumentException()
            };
        }

        private string Read(Board board, string direction, XY pos)
        {
            return direction switch
            {
                "x" => board.ReadX(pos),
                "y" => board.ReadY(pos),

                _ => throw new ArgumentException()
            };
        }

        private (XY, string) GetActualWord(string direction, Board board, XY pos)
        {
            XY startPos = direction switch
            {
                "x" => board.StartOfX(pos),
                "y" => board.StartOfY(pos),

                _ => throw new ArgumentException()
            };

            return direction switch
            {
                "x" => (startPos, board.ReadX(startPos)),
                "y" => (startPos, board.ReadY(startPos)),

                _ => throw new ArgumentException()
            };
        }

        private string GetOtherDirection(string direction)
        {
            return direction switch
            {
                "x" => "y",
                "y" => "x",

                _ => throw new ArgumentException()
            };
        }

        /// <summary>
        /// returns true, if indices are valid, false otherwise
        /// </summary>
        public bool CheckUsedLetterIndices(int[] indices, char[] letters, IEnumerable<char> inventory)
        {
            if (indices.Any(i => i < 0 || i > inventory.Count()))
            {
                // all indices must point to element in inventory
                return false;
            }

            // if the set of letters in the inventory at the corresponding indices matches the used letters, indices are valid
            return Enumerable.SequenceEqual(
                indices.Distinct().Select(inventory.ElementAt).OrderBy(ch => ch),
                letters.OrderBy(ch => ch));
        }
    }
}
