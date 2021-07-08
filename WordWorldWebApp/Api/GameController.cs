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

namespace WordWorldWebApp.Api
{
    [Route("game/{action}")]
    [ApiController]
    public class GameController : Controller
    {
        private readonly BoardProvider _boardProvider;
        private readonly PlayerManager _playerManager;
        private readonly WordRaterProvider _wordRaterProvider;
        private readonly LetterBagProvider _letterBagProvider;
        private readonly WordSetProvider _wordSetProvider;
        private readonly MoveChecker _moveChecker;

        public GameController(
            BoardProvider boardProvider,
            PlayerManager playerManager,
            WordRaterProvider wordRaterProvider,
            LetterBagProvider letterBagProvider,
            WordSetProvider wordSetProvider,
            MoveChecker moveChecker)
        {
            _boardProvider = boardProvider;
            _playerManager = playerManager;
            _wordRaterProvider = wordRaterProvider;
            _letterBagProvider = letterBagProvider;
            _wordSetProvider = wordSetProvider;
            _moveChecker = moveChecker;
        }

        [NonAction]
        private async Task<Player> FetchPlayerAsync(string token, bool setAction)
        {
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
            // TODO: ošetřit null

            var board = (await FetchPlayerAsync(token, false)).Board;

            return Ok(ApiResponse.Success(new
            {
                board = await board.ScanAsync((x, y, w, h))
            }));
        }

        public async Task<IActionResult> Status(string token)
        {
            // TODO: ošetřit null

            var player = await FetchPlayerAsync(token, false);

            return Ok(ApiResponse.Success(PlayerStatus.From(player)));
        }

        public async Task<IActionResult> Write(string token, string word, string direction, int x, int y, string used)
        {                                       
            var player = await FetchPlayerAsync(token, true);
            var board = player.Board;

            int[] usedIndices = used?.Split('_')?.Select(int.Parse)?.ToArray(); // ?? player.Inventory.GetIndices(word);

            return await board.DoAsync(() =>
            {
                // if there is a problem, this will throw; otherwise, it returns how many letters were placed by player
                _moveChecker.AssertMoveValidUnsafe(player, (x, y), direction, word, out char[] placedLetters);

                if (placedLetters.Length <= 0)
                {
                    // if no letters from players inventory were used (value returned by AssertMoveValidAsync), something is not right
                    throw new InvalidPlacementException(x, y);
                }

                if (usedIndices == null)
                {
                    throw new LetterNotInInventoryException(); // GetIndices isn't working: TODO: fix

                    // if the caller didn't specify the indices of used letters, we choose a valid array of indices arbitrarily
                    usedIndices = player.Inventory.GetIndices(placedLetters);
                }

                if (!_moveChecker.CheckUsedLetterIndices(usedIndices, placedLetters, player.Inventory))
                {
                    throw new LetterNotInInventoryException();
                }

                Func<XY, string, bool> method = direction switch
                {
                    "x" => board.WriteX,
                    "y" => board.WriteY,

                    _ => throw new ArgumentException()
                };

                if (method((x, y), word))
                {
                    lock (player)
                    {
                        player.Score += _wordRaterProvider.GetWordRater(_boardProvider.WordRaterOf(board)).Rate(placedLetters, word.Length);
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
