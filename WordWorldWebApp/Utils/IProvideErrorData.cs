using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordWorldWebApp.Utils
{
    public interface IProvideErrorData
    {
        /// <summary>
        /// get error data associated with this exception to be returned via REST API.
        /// </summary>
        object GetErrorData();
    }
}
