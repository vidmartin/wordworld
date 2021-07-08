using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WordWorldWebApp.Utils
{
    public interface ILockable
    {
        /// <summary>
        /// if this returns a non-null value, this delegate will be used; otherwise, it will be handled in some other way
        /// </summary>
        Func<Action, Task> Lock { get; }
    }
}
