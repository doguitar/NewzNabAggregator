using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NewzNabAggregator.Common;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.IO;
using System.Linq;

namespace NewzNabAggregator.Web
{
    public class Startup
    {
        public IConfiguration Configuration
        {
            get;
        }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var baseDir = Configuration.GetValue<string>("baseDir");
            var dbPath = Configuration.GetValue<string>("db") ?? $"{baseDir}{Path.DirectorySeparatorChar}db";
            var newznabDetails = from s in Configuration.GetSection("newznab").GetChildren()
                                 select new
                                 {
                                     url = s.GetValue<string>("url"),
                                     token = s.GetValue<string>("token"),
                                     name = s.GetValue<string>("name")
                                 };
            var tokenDetails = from s in Configuration.GetSection("tokens").GetChildren()
                               select new
                               {
                                   token = s.GetValue<string>("token"),
                                   name = s.GetValue<string>("name")
                               };

            services
                .AddSingleton(newznabDetails.Select(d => new NewzNabInfo
                {
                    Uri = new Uri(d.url),
                    Token = d.token,
                    Name = d.name
                }).ToArray())
                .AddSingleton(tokenDetails.Select(t => new TokenInfo { Token = t.token, Name = t.name }).ToDictionary(t => t.Token, t => t))
                .AddSingleton(newznabDetails.Select(d => new NewzNab(new Uri(d.url), d.token)).ToArray()).AddSingleton(new Synchronizer<NewzNabAggregator.Database.Database>(new NewzNabAggregator.Database.Database(dbPath)))
                .AddSwaggerGen(delegate (SwaggerGenOptions c)
                {
                    c.SwaggerDoc("NewzNabAggregator", new OpenApiInfo
                    {
                        Title = "NewzNabAggregator API",
                        Version = "1",
                        Description = string.Empty,
                        Contact = null
                    });
                })
                .AddControllers()
                .AddJsonOptions(delegate (JsonOptions options)
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(delegate (IEndpointRouteBuilder endpoints)
            {
                endpoints.MapControllers();
            });
            app.UseSwagger();
            app.UseSwaggerUI(delegate (SwaggerUIOptions c)
            {
                c.SwaggerEndpoint("/swagger/NewzNabAggregator/swagger.json", "NewzNabAggregator");
                c.RoutePrefix = string.Empty;
            });
        }
    }
}
