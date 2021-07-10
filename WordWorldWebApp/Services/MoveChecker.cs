using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WordWorldWebApp.DataStructures;
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

        public class PlacementPossibility
        {
            public string fullWord;
            public List<char> placedLetters;

            public PlacementPossibility(string fullWord, IEnumerable<char> placedLetters)
            {
                this.fullWord = fullWord;
                this.placedLetters = placedLetters is List<char> l ? l : new List<char>(placedLetters);
            }            
        }

        private class _PlacementPossibility : PlacementPossibility
        {
            public TrieNode? node;

            public _PlacementPossibility(string fullWord, IEnumerable<char> placedLetters, TrieNode node)
                : base(fullWord, placedLetters)
            {
                this.node = node;
            }
        }

        private bool DoesAddingACharacterCreateAValidWord(Board board, WordSet wordSet, char placedChar, XY placedPosition, string direction, out string theWord)
        {
            if (placedChar == WordSet.JOKER) { throw new ArgumentException(); }

            char remembered = board.At(placedPosition);

            try
            {
                // temporarily change current character to test for newly created words
                board.Set(placedPosition, placedChar);

                // get the full word
                (XY start, string fullWord) = GetActualWord(direction, board, placedPosition);

                if (fullWord.Length == 1)
                {
                    // a word of length 1 is always considered valid (for there is a valid word in the other direction being built)
                    theWord = null;
                    return true;
                }

                theWord = fullWord;
                return wordSet.ContainsWord(fullWord);
            }
            finally
            {
                board.Set(placedPosition, remembered);
            }
        }

        public PlacementPossibility[] GetPlacementPossibilities(WordSet wordSet, Board board, XY startPosition, string direction, string wordPattern)
        {
            var stepper = GetStepper(direction); // stepper is used to move the point in the given direction

            string succeedingString = "";
            if (board.At(stepper(startPosition, wordPattern.Length)) != ' ')
            {
                // get string of characters after new word
                succeedingString = GetActualWord(direction, board, stepper(startPosition, wordPattern.Length)).Item2;
            }

            string precedingString = "";
            if (board.At(stepper(startPosition, -1)) != ' ')
            {
                // get string of characters before new word (+ move position to its start)
                (startPosition, precedingString) = GetActualWord(direction, board, stepper(startPosition, -1));
            }


            string fullPattern = precedingString + wordPattern + succeedingString;

            _PlacementPossibility[] possibilities = new[] { new _PlacementPossibility("", new char[0], wordSet.Root) };

            for (int i = 0; i < fullPattern.Length; i++)
            {
                var currentPosition = stepper(startPosition, i);
                var currentCharOnBoard = board.At(currentPosition);
                var currentDesiredChar = wordPattern[i];

                if (currentDesiredChar == WordSet.JOKER)
                {
                    // joker - matches any char => multiple different words might be possible

                    // select the children of the current node or nodes, expanding the words by the next letter
                    possibilities = possibilities
                        .SelectMany(possibility
                            => possibility.node.Children.Select(node
                                => new _PlacementPossibility(
                                    possibility.fullWord + node.Letter,
                                    currentCharOnBoard == ' ' ?
                                        possibility.placedLetters.Concat(new[] { node.Letter }) : // if the current cell of the board is empty, we need to put down this letter
                                        possibility.placedLetters, // otherwise, we don't need to put down anything
                                    node
                                )
                            )
                        )
                        .ToArray();

                    if (currentCharOnBoard != ' ')
                    {
                        // if there is already a letter on the board, only consider the nodes that have that letter
                        possibilities = possibilities.Where(possibility => possibility.node.Letter == currentCharOnBoard).ToArray();

                        if (possibilities.Length == 0)
                        {
                            // if there are no valid options for this letter at this position, throw
                            throw new InvalidPlacementException(currentPosition.x, currentPosition.y);
                        }
                    }
                }
                else
                {
                    if (currentCharOnBoard != ' ' && currentCharOnBoard != currentDesiredChar)
                    {
                        // the letter we want at this position doesn't match the letter that already is there; throw!!
                        throw new InvalidPlacementException(currentPosition.x, currentPosition.y);
                    }

                    possibilities = possibilities
                        .Select(possibility =>
                        {
                            // get the child with the given letter
                            possibility.node = possibility.node.GetChild(currentDesiredChar);
                            return possibility;
                        })
                        .Where(possibility => possibility.node != null) // if no matching children exists, GetChild returns null - there nodes must be filtered out
                        .Select(possibility =>
                        {
                            // extend the word
                            possibility.fullWord = possibility.fullWord + possibility.node.Letter;
                            if (currentCharOnBoard == ' ')
                            {
                                // add placed letter, if the current cell is empty
                                possibility.placedLetters.Add(possibility.node.Letter);
                            }
                            return possibility;
                        })
                        .ToArray();
                }

                if (possibilities.Length == 0)
                {
                    // if no matching children were found, the attempted word doesn't exist => throw 
                    throw new UnknownWordException(true, fullPattern, startPosition.x, startPosition.y, direction);
                }

                string uninentionalInvalidWord = null;

                possibilities = possibilities
                    .Where(possibility =>
                    {
                        // only consider nodes, that don't create invalid words in the other direction
                        bool isValid = DoesAddingACharacterCreateAValidWord(board, wordSet, possibility.node.Letter, currentPosition, GetOtherDirection(direction), out string theWord);
                        if (isValid == false) { uninentionalInvalidWord = theWord; } // cache the unintentional word
                        return isValid;
                    })
                    .ToArray();

                if (possibilities.Length == 0)
                {
                    // if there are no nodes left now, it means that a word would be formed in the other direction, that doesn't exist => throw
                    throw new UnknownWordException(false, uninentionalInvalidWord, startPosition.x, startPosition.y, direction);
                }
            }

            // only consider nodes, that are final
            possibilities = possibilities
                .Where(possibility => possibility.node.IsFinal)
                .ToArray();

            if (possibilities.Length == 0)
            {
                // if there are no nodes left, word doesn't exist => throw
                throw new UnknownWordException(true, fullPattern, startPosition.x, startPosition.y, direction);
            }

            return possibilities;
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

        //private class CanTokenBeUsedAs : IEqualityComparer<char>
        //{
        //    public static readonly CanTokenBeUsedAs INSTANCE = new CanTokenBeUsedAs();

        //    public bool Equals(char x, char y)
        //    {
        //        if (x == WordSet.JOKER)
        //        {
        //            return true;
        //        }

        //        return x == y;
        //    }

        //    public int GetHashCode([DisallowNull] char obj)
        //    {
        //        return obj.GetHashCode();
        //    }
        //}
    }
}
