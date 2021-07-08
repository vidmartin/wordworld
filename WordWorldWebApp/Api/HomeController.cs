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

namespace WordWorldWebApp.Api
{
    public class HomeController : Controller
    {
        private readonly IServiceProvider _serviceProvider;

        public const int START_LETTERS = 10;

        public HomeController(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IActionResult Index()
        {
            return Content("<b>Index</b>");

            // return View();
        }

        [Route("/play/{board?}")]
        public async Task<IActionResult> Play([FromQuery] string token, [FromRoute] string board,
            [FromServices] PlayerManager playerManager,
            [FromServices] BoardProvider boardProvider,
            [FromServices] LetterBagProvider letterBagProvider,
            [FromServices] WordRaterProvider wordRaterProvider)
        {
            var boardInstance = boardProvider.GetBoard(board ?? boardProvider.DefaultBoardKey);

            Player player;

            if (token == null)
            {
                // create new player - vytvořit nového hráče                
                player = await playerManager.NewAsync(boardInstance, letterBagProvider.GetLetterBag(boardProvider.LetterBagOf(boardInstance)).Pull(START_LETTERS));

                return RedirectToAction(ControllerContext.ActionDescriptor.ActionName, new { token = player.Token });
                
                // token = player.Token;
            }
            else
            {
                player = await playerManager.GetAsync(token);
            }

            var origin = new Vec2i(boardInstance.Width / 2, boardInstance.Height / 2);

            // zobrazit hrací pole daného hráče
            return View(new PlayModel()
            {
                Token = token,
                PlayerStatus = PlayerStatus.From(player),
                Origin = origin,
                BoardArray = "",
                BoardRect = new Rect(0, 0, 0, 0),
                BoardSize = new Vec2i(boardInstance.Width, boardInstance.Height),
                CharactersWithScores = wordRaterProvider.GetWordRater(boardProvider.WordRaterOf(boardInstance)).CharMap,
                Language = boardProvider.WordSetOf(boardInstance) // viz startup
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
                    context.Result = Unauthorized();
                }
            }
        }
    }
}
