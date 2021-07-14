using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordWorldWebApp.Config;

namespace WordWorldWebApp.Services
{
    public interface IBoardConfigurer
    {
        IBoardConfigurer UseWordSet(string key);
        IBoardConfigurer UseLetterBag(string key);
        IBoardConfigurer UseWordRater(string key);
        IBoardConfigurer UseConfig(BoardConfig config);
    }
}
