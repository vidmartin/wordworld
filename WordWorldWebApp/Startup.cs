using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WordWorldWebApp.Game;
using WordWorldWebApp.Services;
using JavaScriptEngineSwitcher.V8;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using React.AspNet;
using WordWorldWebApp.HostedServices;
using Microsoft.Extensions.Configuration;
using WordWorldWebApp.Config;

namespace WordWorldWebApp
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<WordWorldConfig>(_configuration.GetSection(WordWorldConfig.CONFIG_KEY));

            services.AddMvc();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddReact();

            services.AddJsEngineSwitcher(options => options.DefaultEngineName = V8JsEngine.EngineName)
                .AddV8();

            DoStuffBasedOnConfig(services);

            services.AddSingleton<PlayerManager>();
            services.AddSingleton<LeaderboardManager>();
            services.AddTransient<MoveChecker>();
            services.AddHostedService<PlayerCleaner>();
        }

        private void DoStuffBasedOnConfig(IServiceCollection services)
        {
            var config = new WordWorldConfig();
            _configuration.Bind(WordWorldConfig.CONFIG_KEY, config);

            services.AddSingleton<BoardProvider>(_ => config.Boards.OrderBy(board => board.Value.Order).Aggregate(new BoardProvider(),
                (boardProvider, boardConfig) => boardProvider.AddBoard(boardConfig.Key,
                    new ThreadSafeBoard(new ArrayBoard(boardConfig.Value.Width, boardConfig.Value.Height)),
                        board => board.UseConfig(boardConfig.Value))));

            services.AddSingleton<WordSetProvider>(_ => config.WordSets.Aggregate(new WordSetProvider(),
                (wordSetProvider, wordSetConfig) => wordSetProvider.AddWordSet(wordSetConfig.Key,
                    new WordSet().UseConfig(wordSetConfig.Value))));

            services.AddSingleton<LetterBagProvider>(p => config.LetterBags.Aggregate(new LetterBagProvider(p),
                (letterBagProvider, letterBagConfig) => letterBagProvider.AddLetterBag(letterBagConfig.Key,
                    new SimpleLetterBag().UseConfig(letterBagConfig.Value))));

            services.AddSingleton<WordRaterProvider>(_ => config.WordRaters.Aggregate(new WordRaterProvider(),
                (wordRaterProvider, wordRaterConfig) => wordRaterProvider.AddWordRater(wordRaterConfig.Key,
                    new WordRater().UseConfig(wordRaterConfig.Value))));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseReact(options => { });

            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
