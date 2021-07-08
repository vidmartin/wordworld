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
                    .UseWordSet("english")
                    .UseWordRater("english"))
                .AddBoard("czech1", new ThreadSafeBoard(new ArrayBoard(1000, 1000)), options => options
                    .UseLetterBag("czech")
                    .UseWordSet("czech")
                    .UseWordRater("czech"))
                .SetDefaultBoard("english1"));

            services.AddSingleton<PlayerManager>();
            
            services.AddSingleton<WordRater>(_ => new WordRater()
                .LoadDefaultCharMap());

            services.AddSingleton<WordSetProvider>(_ => new WordSetProvider()
                .AddWordSet("english", new WordSet().SetLetterRange("a-z").LoadFromFile("./dict/english.txt"))
                .AddWordSet("czech", new WordSet().SetLetterRange("a-z,·-û").LoadFromFile("./dict/czech.txt")));

            services.AddSingleton<LetterBagProvider>(p => new LetterBagProvider(p)
                .AddLetterBag("english", new SimpleLetterBag("eeeeeeeeeeeetttttttttaaaaaaaaoooooooiiiiiiinnnnnnnsssssshhhhhhrrrrrrddddllllcccuuummmwwwfffggyyppbbvklxqz"))
                .AddLetterBag("czech", new SimpleLetterBag("ooooooooeeeeeeeennnnnnnaaaaaaattttttvvvvvsssssiiiillllkkkkrrrrddddpppÌÌÌmmmuuu···zzjjyyÏÏccbbÈÈh¯˝ûËö˘fgÚxùÛÔwq")));

            services.AddSingleton<WordRaterProvider>(p => new WordRaterProvider()
                .AddWordRater("english", new WordRater().LoadDefaultCharMap())
                .AddWordRater("czech", new WordRater().LoadCharMap("a:2,·:4,b:6,c:6,Ë:10,d:3,Ô:20,e:2,È:7,Ï:6,f:15,g:15,h:8,i:2,Ì:3,j:5,k:3,l:2,m:3,n:2,Ú:20,o:1,Û:20,p:3,q:20,r:3,¯:8,s:2,ö:10,t:2,ù:20,u:3,˙:15,˘:10,v:2,w:20,x:20,y:5,˝:10,z:5,û:10")));

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
