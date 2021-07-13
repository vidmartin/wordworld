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
        private readonly WordWorldConfig _config;

        public HomeController(IServiceProvider serviceProvider, IOptions<WordWorldConfig> options)
        {
            _serviceProvider = serviceProvider;
            _config = options.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("/play/{board?}")]
        public async Task<IActionResult> Play([FromQuery] string token, [FromQuery] string username,
            [FromRoute] string board,
            [FromServices] PlayerManager playerManager,
            [FromServices] BoardProvider boardProvider,
            [FromServices] LetterBagProvider letterBagProvider,
            [FromServices] WordRaterProvider wordRaterProvider,
            [FromServices] IStringLocalizer<ValidationLocalizer> validationMessageLocalizer)
        {
            var boardInstance = boardProvider.GetBoard(board ?? boardProvider.DefaultBoardKey);

            Player player = default;

            if (token == null)
            {
                // create new player / vytvořit nového hráče

                try
                {
                    player = await playerManager.NewAsync(boardInstance, username, letterBagProvider.GetLetterBag(boardProvider.LetterBagOf(boardInstance)).Pull(_config.LettersPerPlayer));
                }
                catch (ValidationException validationException)
                {
                    ModelState.AddModelError("", validationMessageLocalizer[validationException.ValidationResult.ErrorMessage]);
                    return View("Index");
                }
                catch (AlreadyExistsException)
                {
                    ModelState.AddModelError("", validationMessageLocalizer["username_taken"]);
                    return View("Index");
                }

                return RedirectToAction(ControllerContext.ActionDescriptor.ActionName, new { token = player.Token });
                
                // token = player.Token;
            }
            else
            {
                player = await playerManager.GetAsync(token) ?? throw new PlayerNotFoundException();
            }

            var origin = new Vec2i(boardInstance.Width / 2, boardInstance.Height / 2);

            // zobrazit hrací pole daného hráče
            return View(new PlayModel()
            {
                Token = token,
                Username = player.Username,
                PlayerStatus = PlayerStatus.From(player),
                Origin = origin,
                BoardArray = "",
                BoardRect = new Rect(0, 0, 0, 0),
                BoardSize = new Vec2i(boardInstance.Width, boardInstance.Height),
                CharactersWithScores = wordRaterProvider.GetWordRater(boardProvider.WordRaterOf(boardInstance)).CharMap,                
            });

            // return Content("<b>Hello!</b>");
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
