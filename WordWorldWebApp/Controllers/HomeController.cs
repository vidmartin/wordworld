using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordWorldWebApp.Services;
using Microsoft.Extensions.DependencyInjection;
using WordWorldWebApp.Models;
using WordWorldWebApp.Game;
using WordWorldWebApp.Extensions;
using WordWorldWebApp.DataStructures;
using Microsoft.AspNetCore.Mvc.Filters;
using WordWorldWebApp.Exceptions;
using System.ComponentModel.DataAnnotations;
using WordWorldWebApp.Config;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;

namespace WordWorldWebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly PlayerManager _playerManager;
        private readonly BoardProvider _boardProvider;
        private readonly LetterBagProvider _letterBagProvider;
        private readonly WordRaterProvider _wordRaterProvider;
        private readonly WordWorldConfig _config;

        public HomeController(IServiceProvider serviceProvider, IOptions<WordWorldConfig> options,
            PlayerManager playerManager,
            BoardProvider boardProvider,
            LetterBagProvider letterBagProvider,
            WordRaterProvider wordRaterProvider)
        {
            _serviceProvider = serviceProvider;
            _playerManager = playerManager;
            _boardProvider = boardProvider;
            _letterBagProvider = letterBagProvider;
            _wordRaterProvider = wordRaterProvider;
            _config = options.Value;
        }

        public IActionResult Index()
        {
            return View(new PlayerCreateModel());
        }

        [Route("/play")]
        [HttpPost]
        public async Task<IActionResult> Play(PlayerCreateModel playerCreateModel, [FromServices] IStringLocalizer<ValidationLocalizer> validationMessageLocalizer)
        {
            // create new player

            if (!ModelState.IsValid)
            {
                return View("Index", playerCreateModel);
            }

            Player player = default;

            try
            {
                Board boardInstance = _boardProvider.GetBoard(playerCreateModel.Board);
                player = await _playerManager.NewAsync(boardInstance, playerCreateModel.Username, _letterBagProvider.GetLetterBag(_boardProvider.LetterBagOf(boardInstance)).Pull(_config.LettersPerPlayer));

                return RedirectToAction("Play", new { token = player.Token, username = player.Username, board = playerCreateModel.Board });
            }
            catch (Exception e)
            {
                ModelState.AddModelError("", validationMessageLocalizer[e switch
                {
                    BoardNotFoundException => "board_not_found",
                    AlreadyExistsException => "username_taken",
                    ValidationException validationException => validationException.ValidationResult.ErrorMessage,
                    _ => throw e
                }]);

                return View("Index", playerCreateModel);
            }            
        }
        
        [Route("/play/{board?}")]
        [HttpGet]
        public async Task<IActionResult> Play([FromQuery] string token, [FromQuery] string username, [FromRoute] string board)
        {
            var boardInstance = _boardProvider.GetBoard(board ?? _boardProvider.DefaultBoardKey);

            Player player = await _playerManager.GetAsync(token) ?? throw new PlayerNotFoundException();

            return GameView(player);

            // return Content("<b>Hello!</b>");
        }

        private IActionResult GameView(Player player)
        {
            var origin = new Vec2i(player.Board.Width / 2, player.Board.Height / 2);

            // zobrazit hrací pole daného hráče
            return View(new PlayModel()
            {
                Token = player.Token,
                Username = player.Username,
                PlayerStatus = PlayerStatus.From(player),
                Origin = origin,
                BoardArray = "",
                BoardRect = new Rect(0, 0, 0, 0),
                BoardSize = new Vec2i(player.Board.Width, player.Board.Height),
                CharactersWithScores = _wordRaterProvider.GetWordRater(_boardProvider.WordRaterOf(player.Board)).CharMap,
            });
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                // pokud hráč, deska, atd. nebylo nalezeno
                // TODO: ošetřit robustněji
                if (context.Exception is KeyNotFoundException)
                {
                    context.ExceptionHandled = true;
                    context.Result = BadRequest();
                }

                // pokud nějaký důležitý argument nebyl v requestu
                else if (context.Exception is ArgumentNullException)
                {
                    context.ExceptionHandled = true;
                    context.Result = BadRequest();
                }

                // pokud hráč se zadaným uživatelským jménem již existuje
                else if (context.Exception is AlreadyExistsException)
                {
                    context.ExceptionHandled = true;
                    context.Result = BadRequest();
                }
            }
        }
    }
}
