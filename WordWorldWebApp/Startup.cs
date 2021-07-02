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

namespace WordWorldWebApp
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddReact();

            services.AddJsEngineSwitcher(options => options.DefaultEngineName = V8JsEngine.EngineName)
                .AddV8();

            services.AddSingleton<BoardProvider>(_ => new BoardProvider()
                .AddBoard("english1", new ThreadSafeBoard(new ArrayBoard(1000, 1000)), options => options
                    .UseLetterBag("english")
                    .UseWordSet("english"))
                .SetDefaultBoard("english1"));

            services.AddSingleton<PlayerManager>();

            services.AddSingleton<WordRater>(_ => new WordRater()
                .LoadDefaultCharMap());

            services.AddSingleton<WordSetProvider>(_ => new WordSetProvider()
                .AddWordSet("english", new WordSet().SetLetterRange("a-z").LoadFromFile("./dict/english.txt")));

            services.AddSingleton<LetterBagProvider>(p => new LetterBagProvider(p)
                .AddLetterBag("english", new SimpleLetterBag("eeeeeeeeeeeetttttttttaaaaaaaaoooooooiiiiiiinnnnnnnsssssshhhhhhrrrrrrddddllllcccuuummmwwwfffggyyppbbvklxqz")));

            services.AddTransient<MoveChecker>();
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
