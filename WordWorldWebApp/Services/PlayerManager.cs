using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WordWorldWebApp.Exceptions;
using WordWorldWebApp.Game;

namespace WordWorldWebApp.Services
{
    public class PlayerManager
    {
        private class _Player : Player
        {
            public _Player(string token, Board board, DateTime created, IEnumerable<char> letters)
            {
                Token = token;
                Board = board;
                Created = created;

                _inventory = letters is List<char> l ? l : new List<char>(letters);
            }

            public override string Token { get; }

            public override Board Board { get; }

            public override DateTime Created { get; }
        }

        private readonly Dictionary<string, Player> _players = new Dictionary<string, Player>();

        private Player NewUnsafe(Board board, IEnumerable<char> letters)
        {
            var player = new _Player(Guid.NewGuid().ToString(), board, DateTime.Now, letters);

            _players.Add(player.Token, player);

            return player;
        }        

        private Player GetUnsafe(string token)
        {
            if (!_players.ContainsKey(token))
            {
                throw new PlayerNotFoundException();
            }

            return _players[token];
        }

        private bool DeleteUnsafe(string token)
        {
            return _players.Remove(token);
        }

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public Player New(Board board, IEnumerable<char> letters)
        {
            try
            {
                _semaphore.Wait();
                return NewUnsafe(board, letters);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public Player Get(string token)
        {
            try
            {
                _semaphore.Wait();
                return GetUnsafe(token);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public bool Delete(string token)
        {
            try
            {
                _semaphore.Wait();
                return DeleteUnsafe(token);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<Player> NewAsync(Board board, IEnumerable<char> letters)
        {
            try
            {
                await _semaphore.WaitAsync();
                return NewUnsafe(board, letters);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<Player> GetAsync(string token)
        {
            try
            {
                await _semaphore.WaitAsync();
                return GetUnsafe(token);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<bool> DeleteAsync(string token)
        {
            try
            {
                await _semaphore.WaitAsync();
                return DeleteUnsafe(token);
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
