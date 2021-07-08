using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WordWorldWebApp.Exceptions;
using WordWorldWebApp.Game;
using WordWorldWebApp.Utils;

namespace WordWorldWebApp.Services
{
    public class PlayerManager : ILockable
    {
        private class _Player : Player
        {
            public _Player(string token, string username, Board board, DateTime created, IEnumerable<char> letters)
            {
                Token = token;
                Board = board;
                LastAction = Created = created;
                Username = username;

                _inventory = // letters is List<char> l ? l : new List<char>(letters);
                    letters is char[] arr ? arr : letters.ToArray();
            }

            public override string Token { get; }

            public override Board Board { get; }

            public override DateTime Created { get; }

            public override string Username { get; }
        }

        // TODO: use ConcurrentDictionary?
        private readonly Dictionary<string, Player> _playersByToken = new Dictionary<string, Player>();
        private readonly Dictionary<string, Player> _playersByUsername = new Dictionary<string, Player>();

        public void ValidatePlayer(Player player)
        {
            var context = new ValidationContext(player);
            Validator.ValidateObject(player, context);
        }
        
        private Player NewUnsafe(Board board, string username, IEnumerable<char> letters)
        {
            if (username != null && _playersByUsername.ContainsKey(username))
            {
                throw new AlreadyExistsException();
            }

            var player = new _Player(Guid.NewGuid().ToString(), username, board, DateTime.Now, letters);
            ValidatePlayer(player);

            _playersByToken.Add(player.Token, player);
            _playersByUsername.Add(player.Username, player);

            return player;
        }        

        private Player? GetUnsafe(string token)
        {
            if (!_playersByToken.ContainsKey(token))
            {
                // throw new PlayerNotFoundException();
                return null;
            }

            return _playersByToken[token];
        }

        private Player? GetByUsernameUnsafe(string username)
        {
            if (!_playersByUsername.ContainsKey(username))
            {
                // throw new PlayerNotFoundException();
                return null;
            }

            return _playersByUsername[username];
        }

        private bool DeleteUnsafe(string token)
        {
            return _playersByToken.Remove(token) && _playersByUsername.Remove(token);
        }

        private Player[] GetAllPlayersUnsafe()
        {
            return _playersByToken.Values.ToArray();
        }

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public Func<Action, Task> Lock => async action =>
        {
            try
            {
                await _semaphore.WaitAsync();
                action();
            }
            finally
            {
                _semaphore.Release();
            }
        };

        public Player New(Board board, string username, IEnumerable<char> letters)
        {
            try
            {
                _semaphore.Wait();
                return NewUnsafe(board, username, letters);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public Player? Get(string token)
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

        public Player? GetByUsername(string username)
        {
            try
            {
                _semaphore.Wait();
                return GetByUsernameUnsafe(username);
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

        public Player[] GetAllPlayers()
        {
            try
            {
                _semaphore.Wait();
                return GetAllPlayersUnsafe();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<Player> NewAsync(Board board, string username, IEnumerable<char> letters)
        {
            try
            {
                await _semaphore.WaitAsync();
                return NewUnsafe(board, username, letters);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<Player?> GetAsync(string token)
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

        public async Task<Player?> GetByUsernameAsync(string username)
        {
            try
            {
                await _semaphore.WaitAsync();
                return GetByUsernameUnsafe(username);
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

        public async Task<Player[]> GetAllPlayersAsync()
        {
            try
            {
                await _semaphore.WaitAsync();
                return GetAllPlayersUnsafe();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
