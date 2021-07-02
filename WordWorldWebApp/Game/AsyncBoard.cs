using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordWorldWebApp.Game
{
    public abstract class AsyncBoard : Board
    {
        public abstract Task<string> ReadXAsync(XY pos);
        public abstract Task<string> ReadYAsync(XY pos);
        public abstract Task<bool> WriteXAsync(XY pos, string s);
        public abstract Task<bool> WriteYAsync(XY pos, string s);
        public abstract Task<XY> StartOfXAsync(XY pos);
        public abstract Task<XY> StartOfYAsync(XY pos);
        public abstract Task<XY> EndOfXAsync(XY pos);
        public abstract Task<XY> EndOfYAsync(XY pos);
        public abstract Task<bool> DeleteXAsync(XY pos);
        public abstract Task<bool> DeleteYAsync(XY pos);
        public abstract Task<char> AtAsync(XY pos);
        public abstract Task SetAsync(XY pos, char ch);
    }
}
