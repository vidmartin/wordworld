using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordWorldWebApp.Services
{
    public class WordRater
    {
        private Dictionary<char, int> _charScores;

        public WordRater LoadCharMap(string charmap)
        {
            _charScores = new Dictionary<char, int>();

            foreach (string s in charmap.ToLower().Split(','))
            {
                string[] c = s.Split(':');

                _charScores[char.Parse(c[0])] = int.Parse(c[1]);
            }

            return this;
        }

        public WordRater LoadDefaultCharMap()
            => this.LoadCharMap("a:2,b:6,c:5,d:4,e:1,f:6,g:6,h:3,i:2,j:15,k:10,l:4,m:5,n:3,o:2,p:6,q:15,r:3,s:3,t:2,u:5,v:8,w:6,x:13,y:6,z:20");

        public int Rate(char ch)
        {
            return _charScores[char.ToLower(ch)];
        }

        public int Rate(string s)
        {
            return s.Sum(Rate) * s.Length;
        }
    }
}
