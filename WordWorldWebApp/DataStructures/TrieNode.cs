﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordWorldWebApp.DataStructures
{
    public class TrieNode
    {
        private static (char, char)[] ParseLetterRanges(string letterRanges)
        {
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

        private int GetArrayIndex(char ch)
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

            throw new ArgumentException();
        }

        public int AlphabetLength => _letterRanges.Sum(tuple => tuple.Item2 - tuple.Item1 + 1);

        public TrieNode GetChild(char letter)
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

                if (curr.node._next == null)
                {
                    matchingNode = curr.node._next = MakeChild(letter);

                    if (curr.i + 2 >= HowManyChildrenToMakeArray)
                    {
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
    }
}
