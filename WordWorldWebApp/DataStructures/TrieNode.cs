using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordWorldWebApp.DataStructures
{
    public class TrieNode
    {
        private static (char, char)[] ParseLetterRanges(string letterRanges)
        {
            // převést řezězec ve formátu např. "a-z,č-ž" na odpovídající seznam dvojic znaků, aby bylo každému znaku možno přiřadit unikátní číselnou hodnotu, které půjdou po sobě

            List<(char, char)> list = new List<(char, char)>();

            foreach (string s in letterRanges.Split(','))
            {
                string[] p = s.Split('-');

                list.Add((char.Parse(p[0]), char.Parse(p[1])));
            }

            return list.ToArray();
        }

        public TrieNode(char letter, string letterRanges, int howManyChildrenToMakeArray)
            : this(letter, ParseLetterRanges(letterRanges), howManyChildrenToMakeArray)
        { }
                 

        public TrieNode(char letter, (char, char)[] letterRanges, int howManyChildrenToMakeArray)
        {
            HowManyChildrenToMakeArray = howManyChildrenToMakeArray;

            _letterRanges = letterRanges;

            Letter = letter;
        }

        public char Letter { get; }

        private TrieNode _next = null;
        private TrieNode _child = null;
        private TrieNode[] _children = null;

        public bool IsFinal { get; private set; } = false;

        public void SetFinal(bool final) => IsFinal = final;

        public int HowManyChildrenToMakeArray { get; }        

        private readonly (char, char)[] _letterRanges;

        /// <summary>
        /// returns the index of the character in the letter range (each character in the letter range will get a unique index).
        /// if the character isn't in the range: if default value is supplied, that will be returned; otherwise, an ArgumentException will be thrown.
        /// </summary>
        public int GetArrayIndex(char ch, int? defaultValue = null)
        {
            int curr = 0;

            foreach (var tuple in _letterRanges)
            {
                if (ch >= tuple.Item1 && ch <= tuple.Item2)
                {
                    return ch - tuple.Item1 + curr;
                }

                curr += tuple.Item2 - tuple.Item1 + 1; // add length of current range
            }

            return defaultValue ?? throw new ArgumentException();
        }

        public int AlphabetLength => _letterRanges.Sum(tuple => tuple.Item2 - tuple.Item1 + 1);

        public TrieNode? GetChild(char letter)
        {
            if (_children != null)
            {
                return _children[GetArrayIndex(letter)];
            }

            return _child?.TraverseSiblings().FirstOrDefault(node => node.Letter == letter);
        }
        
        public TrieNode[] Children
        {
            get
            {
                if (_children != null)
                {
                    return _children.Where(n => n != null).ToArray();
                }
                else if (_child != null)
                {
                    return _child.TraverseSiblings().ToArray();
                }
                else
                {
                    return new TrieNode[0];
                }
            }
        }

        public IEnumerable<TrieNode> TraverseSiblings()
        {
            var curr = this;

            while (curr != null)
            {
                yield return curr;
                curr = curr._next;
            }
        }
        
        private TrieNode MakeChild(char ch)
        {
            return new TrieNode(ch, _letterRanges, HowManyChildrenToMakeArray);
        }

        public bool AddChild(char letter, out TrieNode matchingNode)
        {
            if (_children != null)
            {
                matchingNode = _children[GetArrayIndex(letter)];

                if (matchingNode == null)
                {
                    matchingNode = _children[GetArrayIndex(letter)] = MakeChild(letter);
                    return true;
                }

                return false;
            }

            if (_child == null)
            {
                matchingNode = _child = MakeChild(letter);
                return true;
            }

            foreach (var curr in _child.TraverseSiblings().Select((node, i) => (node: node, i: i)))
            {
                if (curr.node.Letter == letter)
                {
                    matchingNode = curr.node;
                    return false;
                }

                // add node at the end of the linked list of children
                if (curr.node._next == null)
                {
                    matchingNode = curr.node._next = MakeChild(letter);

                    if (curr.i + 2 >= HowManyChildrenToMakeArray)
                    {
                        // at some point, we stop using the linked list and start using an array, to keep some balance between time and memory this uses
                        InitArray();
                    }

                    return true;
                }
            }

            throw new InvalidOperationException();
        }

        public bool AddChild(char letter) => AddChild(letter, out _);

        private void InitArray()
        {
            _children = new TrieNode[AlphabetLength];

            foreach (var node in _child.TraverseSiblings())
            {
                _children[GetArrayIndex(node.Letter)] = node;
            }            
        }

        /// <summary>
        /// returns, whether this tree or subtree contains the given word
        /// </summary>
        public bool ContainsWord(string word)
        {
            var curr = this;

            foreach (char ch in word)
            {
                curr = curr.GetChild(ch);

                if (curr == null) { return false; }
            }

            return curr.IsFinal;
        }

        /// <summary>
        /// returns all words that match the given pattern - normal letters and a wildcard, which matches any character.
        /// </summary>
        public string[] GetMatchingWords(string pattern, char wildcard)
        {
            var curr = new[] { (node: this, word: this.Letter.ToString()) };

            foreach (char ch in pattern)
            {
                if (ch == wildcard)
                {
                    // wildcard matches all children
                    curr = curr.SelectMany(tuple => tuple.node.Children.Select(child => (node: child, word: tuple.word + child.Letter))).ToArray();
                    continue;
                }

                // for every node, select single child with the given letter
                curr = curr.Select(tuple => (node: tuple.node.GetChild(ch), word: tuple.word))
                    .Where(tuple => tuple.node != null)
                    .Select(tuple => (node: tuple.node, word: tuple.word + tuple.node.Letter))
                    .ToArray();

                if (curr.Length == 0)
                {
                    return new string[0];
                }
            }

            return curr.Where(tuple => tuple.node.IsFinal)
                .Select(tuple => tuple.word)
                .ToArray();                
        }
    }
}
