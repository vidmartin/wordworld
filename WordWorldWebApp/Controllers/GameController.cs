using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WordWorldWebApp.Exceptions;
using WordWorldWebApp.Extensions;
using WordWorldWebApp.Game;
using WordWorldWebApp.Models;
using WordWorldWebApp.Services;
using WordWorldWebApp.Utils;
using static WordWorldWebApp.Services.MoveChecker;

namespace WordWorldWebApp.Controllers
{
    [Route("game/{action}")]
    [ApiController]
    public class GameController : Controller
    {
        public const int PLAYERS_ON_LEADERBOARD_COUNT = 5;

        private readonly BoardProvider _boardProvider;
        private readonly PlayerManager _playerManager;
        private readonly WordRaterProvider _wordRaterProvider;
        private readonly LetterBagProvider _letterBagProvider;
        private readonly WordSetProvider _wordSetProvider;
        private readonly MoveChecker _moveChecker;
        private readonly LeaderboardManager _leaderboardManager;

        public GameController(
            BoardProvider boardProvider,
            PlayerManager playerManager,
            WordRaterProvider wordRaterProvider,
            LetterBagProvider letterBagProvider,
            WordSetProvider wordSetProvider,
            MoveChecker moveChecker,
            LeaderboardManager leaderboardManager)
        {
            _boardProvider = boardProvider;
            _playerManager = playerManager;
            _wordRaterProvider = wordRaterProvider;
            _letterBagProvider = letterBagProvider;
            _wordSetProvider = wordSetProvider;
            _moveChecker = moveChecker;
            _leaderboardManager = leaderboardManager;
        }

        [NonAction]
        private async Task<Player> FetchPlayerAsync(string token, bool setAction)
        {
            if (token == null)
            {
                throw new ActionArgumentException();
            }

            var player = await _playerManager.GetAsync(token);

            if (player == null)
            {
                throw new PlayerNotFoundException();
            }

            if (setAction)
            {
                lock (player) { player.LastAction = DateTime.Now; }
            }

            return player;
        }

        public async Task<IActionResult> Scan(string token, int x, int y, int w, int h)
        {
            var board = (await FetchPlayerAsync(token, false)).Board;

            return Ok(ApiResponse.Success(new
            {
                board = await board.ScanAsync((x, y, w, h))
            }));
        }

        public async Task<IActionResult> Status(string token)
        {
            var player = await FetchPlayerAsync(token, false);

            return Ok(ApiResponse.Success(PlayerStatus.From(player)));
        }

        /// <summary>
        /// an action that attempts to write the given word as the given player onto the corresponding board
        /// </summary>
        /// <param name="token">player's token</param>
        /// <param name="word">the word that is supposed to be written; can contain jokers</param>
        /// <param name="direction">the direction in which the word is supposed to be written</param>
        /// <param name="x">the x coordinate of the first letter placed</param>
        /// <param name="y">the y coordinate of the first letter placed</param>
        /// <param name="used">the indexes of the used letters in the player's inventory</param>
        /// <param name="spec">if the word contains jokers, multiple different words might be possible; this optional argument is used to specify, which word is meant</param>
        /// <returns></returns>
        public async Task<IActionResult> Write(string token, string word, string direction, int x, int y, string used, string spec = null)
        {                                       
            var player = await FetchPlayerAsync(token, true);
            var board = player.Board;
            var wordSet = _wordSetProvider.GetWordSet(_boardProvider.WordSetOf(board));

            // TODO: handle FormatException
            int[] usedIndices = used?.Split('_')?.Select(int.Parse)?.ToArray(); // ?? player.Inventory.GetIndices(word);

            return await board.DoAsync(() =>
            {
                // if there is a problem, this will throw; otherwise, it returns how many letters were placed by player
                var possibilities = _moveChecker.GetPlacementPossibilities(wordSet, board, (x, y), direction, word);

                if (possibilities.Length == 0)
                {
                    // GetPlacementPossibilities should never return an empty array
                    throw new InvalidOperationException();
                }

                // only consider the possibilities, in which the player actually placed a letter
                possibilities = possibilities.Where(possibility => possibility.placedLetters.Any()).ToArray();

                if (possibilities.Length == 0)
                {
                    // if the array of possibilities is empty now, it means that the player didn't place any letters => error
                    throw new InvalidPlacementException(x, y);
                }

                PlacementPossibility chosenPossibility = spec == null ?
                    possibilities.SingleOrDefault() :
                    possibilities.SingleOrDefault(possibility => possibility.fullWord == spec);

                if (chosenPossibility == null)
                {
                    // if there is more than one possibility, the player has to choose one unambiguously using the "spec" parameter
                    throw new AmbiguousJokerException(possibilities);
                }

                if (usedIndices == null)
                {
                    throw new ActionArgumentException(); // GetIndices isn't working: TODO: fix

                    // if the caller didn't specify the indices of used letters, we choose a valid array of indices arbitrarily
                    usedIndices = player.Inventory.GetIndices(chosenPossibility.placedLetters);
                }

                if (!_moveChecker.CheckUsedLetterIndices(usedIndices, chosenPossibility.placedLetters, player.Inventory))
                {
                    throw new LetterNotInInventoryException();
                }

                Func<XY, string, bool> method = direction switch
                {
                    "x" => board.WriteX,
                    "y" => board.WriteY,

                    _ => throw new ActionArgumentException()
                };

                if (method((x, y), word))
                {
                    lock (player)
                    {
                        player.Score += _wordRaterProvider.GetWordRater(_boardProvider.WordRaterOf(board)).Rate(chosenPossibility.placedLetters, word.Length);
                        player.ReplaceLetters(usedIndices,
                            _letterBagProvider.GetLetterBag(_boardProvider.LetterBagOf(board)).Pull(usedIndices.Length)); // DONE???: fix this (word length doesn't always match the count of used letters)
                    }

                    return Ok(ApiResponse.Success(PlayerStatus.From(player)));
                }

                throw new NotImplementedException();
            });
        }

        /// <summary>
        /// check whether username is available
        /// </summary>
        public async Task<IActionResult> Available(string username)
        {
            return Ok(ApiResponse.Success(new
            {
                result = (await _playerManager.GetByUsernameAsync(username)) == null
            }));
        }

        /// <summary>
        /// return top players on the board of the current player
        /// </summary>
        public async Task<IActionResult> Leaderboard(string token)
        {
            var player = await FetchPlayerAsync(token, false);

            var leaderboard = (await _leaderboardManager.GetLeaderboardAsync())
                .Where(p => p.Board == player.Board)
                .Take(PLAYERS_ON_LEADERBOARD_COUNT);

            return Ok(ApiResponse.Success(new
            {
                players = leaderboard.Select(p => new { score = p.Score, username = p.Username })
            }));
        }


        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                var response = ApiResponse.Fail(context.Exception);

                if (response == null)
                {
                    // if a response cannot be generated for the given exception, deem the exception as unhandled
                    return;
                }

                context.Result = Ok(response);
                context.ExceptionHandled = true;
                return;
            }
        }
    }
}
