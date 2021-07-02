using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WordWorldWebApp.Game
{
    /// <summary>
    /// TSWABPAABM <br/>
    /// a (hopefully) Thread-Safe Wrapper Around a Board Providing Asynchronous Alternatives to Board Methods<br />
    /// ((doufejme) vláknově bezpečný obalovač desky poskytující asynchronní alternativy k metodám desky)
    /// </summary>
    public class ThreadSafeBoard : AsyncBoard
    {
        private readonly Board _board;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public ThreadSafeBoard(Board innerBoard)
        {
            _board = innerBoard;
        }
        public override int Width => _board.Width;

        public override int Height => _board.Height;

        public override char At(XY pos)
        {
            try
            {
                _semaphore.Wait();
                return _board.At(pos);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override async Task<char> AtAsync(XY pos)
        {
            try
            {
                await _semaphore.WaitAsync();
                return _board.At(pos);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override void Set(XY pos, char ch)
        {
            try
            {
                _semaphore.Wait();
                _board.Set(pos, ch);
                return;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override async Task SetAsync(XY pos, char ch)
        {
            try
            {
                await _semaphore.WaitAsync();
                _board.Set(pos, ch);
                return;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override bool DeleteX(XY pos)
        {
            try
            {
                _semaphore.Wait();
                return _board.DeleteX(pos);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override async Task<bool> DeleteXAsync(XY pos)
        {
            try
            {
                await _semaphore.WaitAsync();
                return _board.DeleteX(pos);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override bool DeleteY(XY pos)
        {
            try
            {
                _semaphore.Wait();
                return _board.DeleteY(pos);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override async Task<bool> DeleteYAsync(XY pos)
        {
            try
            {
                await _semaphore.WaitAsync();
                return _board.DeleteY(pos);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override XY EndOfX(XY pos)
        {
            try
            {
                _semaphore.Wait();
                return _board.EndOfX(pos);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override async Task<XY> EndOfXAsync(XY pos)
        {
            try
            {
                await _semaphore.WaitAsync();
                return _board.EndOfX(pos);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override XY EndOfY(XY pos)
        {
            try
            {
                _semaphore.Wait();
                return _board.EndOfY(pos);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override async Task<XY> EndOfYAsync(XY pos)
        {
            try
            {
                await _semaphore.WaitAsync();
                return _board.EndOfY(pos);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override string ReadX(XY pos)
        {
            try
            {
                _semaphore.Wait();
                return _board.ReadX(pos);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override async Task<string> ReadXAsync(XY pos)
        {
            try
            {
                await _semaphore.WaitAsync();
                return _board.ReadX(pos);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override string ReadY(XY pos)
        {
            try
            {
                _semaphore.Wait();
                return _board.ReadY(pos);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override async Task<string> ReadYAsync(XY pos)
        {
            try
            {
                await _semaphore.WaitAsync();
                return _board.ReadY(pos);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override XY StartOfX(XY pos)
        {
            try
            {
                _semaphore.Wait();
                return _board.StartOfX(pos);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override async Task<XY> StartOfXAsync(XY pos)
        {
            try
            {
                await _semaphore.WaitAsync();
                return _board.StartOfX(pos);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override XY StartOfY(XY pos)
        {
            try
            {
                _semaphore.Wait();
                return _board.StartOfY(pos);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override async Task<XY> StartOfYAsync(XY pos)
        {
            try
            {
                await _semaphore.WaitAsync();
                return _board.StartOfY(pos);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override bool WriteX(XY pos, string s)
        {
            try
            {
                _semaphore.Wait();
                return _board.WriteX(pos, s);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override async Task<bool> WriteXAsync(XY pos, string s)
        {
            try
            {
                await _semaphore.WaitAsync();
                return _board.WriteX(pos, s);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override bool WriteY(XY pos, string s)
        {
            try
            {
                _semaphore.Wait();
                return _board.WriteY(pos, s);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public override async Task<bool> WriteYAsync(XY pos, string s)
        {
            try
            {
                await _semaphore.WaitAsync();
                return _board.WriteY(pos, s);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
