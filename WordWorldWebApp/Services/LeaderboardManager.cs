using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordWorldWebApp.Game;

namespace WordWorldWebApp.Services
{
    /// <summary>
    /// manages the top players, that are to be shown on the leaderboard; should be singleton
    /// </summary>
    public class LeaderboardManager
    {
        /// <summary>
        /// to save resources: when a request generates an answer, the following requests within this timespan will reuse that answer instead of generating a new one
        /// </summary>
        public static readonly TimeSpan CACHE_TIME = TimeSpan.FromSeconds(0.4);
        private readonly PlayerManager _playerManager;

        public LeaderboardManager(PlayerManager playerManager)
        {
            _playerManager = playerManager;
        }

        private Player[] _cachedLeaderboard;
        private DateTime _cachedLeaderboardDateTime;

        /// <summary>
        /// returns players sorted in descending order by score; after returning a new answer, it will be cached for some time to save resources
        /// </summary>
        /// <returns></returns>
        public async Task<Player[]> GetLeaderboardAsync()
        {
            if (_cachedLeaderboard == null && DateTime.Now - _cachedLeaderboardDateTime > CACHE_TIME)
            {
                _cachedLeaderboard = (await _playerManager.GetAllPlayersAsync())
                    .OrderByDescending(player => player.Score)
                    .ToArray();

                _cachedLeaderboardDateTime = DateTime.Now;
            }

            return _cachedLeaderboard;
        }
    }
}
