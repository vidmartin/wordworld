using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordWorldWebApp.Utils;
using static WordWorldWebApp.Services.MoveChecker;

namespace WordWorldWebApp.Exceptions
{
    public class AmbiguousJokerException : Exception, IProvideErrorData
    {
        private readonly PlacementPossibility[] _possibilities;

        public AmbiguousJokerException(PlacementPossibility[] possibilities)
        {
            _possibilities = possibilities;
        }

        public object GetErrorData()
        {
            return new
            {
                possibilities = _possibilities.Select(possibility => possibility.fullWord)
            };
        }
    }
}
