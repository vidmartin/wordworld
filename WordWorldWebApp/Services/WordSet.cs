using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordWorldWebApp.DataStructures;

namespace WordWorldWebApp.Services
{
    public class WordSet
    {
        public const char JOKER = '*';

        private string _letterRange;
        private TrieNode _root;

        public TrieNode Root => _root;

        public WordSet SetLetterRange(string letterRange)
        {
            _letterRange = letterRange;

            return this;
        }

        public WordSet LoadFromFile(string filename)
        {
            _root = new TrieNode('^', _letterRange, 15);

            foreach (var line in File.ReadLines(filename).Select(s => s.Trim().Trim('\0')))
            { 
                if (string.IsNullOrWhiteSpace(line) || line.Any(ch => char.IsWhiteSpace(ch)))
                {
                    continue;
                }

                if (line.Any(ch => _root.GetArrayIndex(ch, -1) == -1))
                {
                    continue;
                }

                var curr = _root;
                foreach (char ch in line.Trim())
                {                   
                    curr.AddChild(char.ToLower(ch), out curr);
                }

                curr.SetFinal(true);
            }

            return this;
        }

        public bool ContainsWord(string word)
        {
            return _root.ContainsWord(word.ToLower());
        }

        public string[] Match(string pattern)
        {
            return _root.GetMatchingWords(pattern, JOKER);
        }

        private static readonly Random _RANDOM = new Random();

        public string RandomWord()
        {
            var builder = new StringBuilder("");

            var curr = _root;

            while (true)
            {
                var children = curr.Children;

                int random = _RANDOM.Next((curr.IsFinal && builder.Length >= 3) ? children.Length + 1 : children.Length);                

                if (random >= children.Length)
                {
                    return builder.ToString();
                }

                curr = children[random];
                builder.Append(curr.Letter);
            }
        }
    }
}
