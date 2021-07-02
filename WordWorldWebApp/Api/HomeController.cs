﻿using Microsoft.AspNetCore.Mvc;
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

        public async Task<IActionResult> Play([FromQuery] string token, [FromQuery] string board,
            [FromServices] PlayerManager playerManager,
            [FromServices] BoardProvider boardProvider,
            [FromServices] LetterBagProvider letterBagProvider)
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
                BoardSize = new Vec2i(boardInstance.Width, boardInstance.Height)
            });

            // return Content("<b>Hello!</b>");
        }
    }
}
