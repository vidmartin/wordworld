using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WordWorldWebApp.Services;
using WordWorldWebApp.Extensions;
using Microsoft.Extensions.Logging;

namespace WordWorldWebApp.HostedServices
{
    public class PlayerCleaner : IHostedService, IDisposable
    {
        public static readonly TimeSpan PLAYER_TIMEOUT = TimeSpan.FromMinutes(1); // TODO: load from config file
        public static readonly TimeSpan CHECK_INVERVAL = TimeSpan.FromMinutes(1); // TODO: load from config file

        private readonly PlayerManager _playerManager;
        private readonly ILogger<PlayerCleaner> _logger;

        public PlayerCleaner(PlayerManager playerManager, ILogger<PlayerCleaner> logger)
        {
            _playerManager = playerManager;
            _logger = logger;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }

        private Task _loopTask;
        private CancellationTokenSource _cancellationTokenSource;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_loopTask != null && _loopTask.Status != TaskStatus.Canceled)
            {
                throw new InvalidOperationException();
            }

            _cancellationTokenSource = new CancellationTokenSource();
            _loopTask = Task.Run(() => LoopAsync(_cancellationTokenSource.Token));

            return Task.CompletedTask;
        }

        private async Task LoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"PlayerCleaner hosted service loop entered");

                while (true)
                {
                    await Task.Delay(CHECK_INVERVAL);

                    await _playerManager.DoAsyncAsync(async () =>
                    {
                        _logger.LogInformation("cleaning inactive players...");

                        var players = await _playerManager.GetAllPlayersAsync();

                        int forgottenPlayas = 0;
                        foreach (var player in players)
                        {
                            if (DateTime.Now - player.LastAction >= PLAYER_TIMEOUT)
                            {
                                _logger.LogDebug($"player {player} is inactive. let them be forever forgotten...");

                                await _playerManager.DeleteAsync(player.Token);
                                forgottenPlayas += 1;
                            }
                        }

                        _logger.LogInformation($"{forgottenPlayas} inactive players found and removed");
                    });
                }
            }
            catch (OperationCanceledException)
            {               
                _logger.LogInformation($"PlayerCleaner hosted service loop exited");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "an exception was thrown in PlayerRemover");
                throw;
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource.Cancel();

            return Task.CompletedTask;
        }
    }
}
