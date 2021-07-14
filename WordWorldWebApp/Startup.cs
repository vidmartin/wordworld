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
using System.Globalization;
using Microsoft.AspNetCore.Localization;

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

            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en"),
                    new CultureInfo("cs")
                };

                options.DefaultRequestCulture = new RequestCulture(supportedCultures[0]);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            services.AddMvc()
                .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization(options =>
                {
                    // make it so that all types share one type for retrieving localized validation messages (dummy type)
                    options.DataAnnotationLocalizerProvider = (type, factory) => factory.Create(typeof(ValidationLocalizer));             
                });
            
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

            services.AddSingleton<BoardProvider>(p => config.Boards.OrderBy(board => board.Value.Order).Aggregate(
                ActivatorUtilities.CreateInstance<BoardProvider>(p),
                (boardProvider, boardConfig) => boardProvider.AddBoard(boardConfig.Key,
                    new ThreadSafeBoard(new ArrayBoard(boardConfig.Value.Width, boardConfig.Value.Height)),
                        board => board.UseConfig(boardConfig.Value))));

            services.AddSingleton<WordSetProvider>(p => config.WordSets.Aggregate(
                ActivatorUtilities.CreateInstance<WordSetProvider>(p),
                (wordSetProvider, wordSetConfig) => wordSetProvider.AddWordSet(wordSetConfig.Key,
                    new WordSet().UseConfig(wordSetConfig.Value))));

            services.AddSingleton<LetterBagProvider>(p => config.LetterBags.Aggregate(
                ActivatorUtilities.CreateInstance<LetterBagProvider>(p),
                (letterBagProvider, letterBagConfig) => letterBagProvider.AddLetterBag(letterBagConfig.Key,
                    new SimpleLetterBag().UseConfig(letterBagConfig.Value))));

            services.AddSingleton<WordRaterProvider>(p => config.WordRaters.Aggregate(
                ActivatorUtilities.CreateInstance<WordRaterProvider>(p),
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

            app.UseRequestLocalization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
