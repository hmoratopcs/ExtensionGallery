﻿using System;
using System.Linq;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Diagnostics;
using Microsoft.AspNet.Diagnostics.Entity;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Security.Cookies;
using Microsoft.Data.Entity;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Microsoft.Framework.Logging.Console;
using ExtensionGallery2.Models;
using Microsoft.AspNet.StaticFiles;
using Microsoft.AspNet.FileSystems;

namespace ExtensionGallery2
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Setup configuration sources.
            Configuration = new Configuration()
                .AddJsonFile("config.json")
                .AddEnvironmentVariables();
        }

        public IConfiguration Configuration { get; set; }

        // This method gets called by the runtime.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add EF services to the services container.
            //services.AddEntityFramework(Configuration)
            //    .AddSqlServer()
            //    .AddDbContext<ApplicationDbContext>();

            //// Add Identity services to the services container.
            //services.AddDefaultIdentity<ApplicationDbContext, ApplicationUser, IdentityRole>(Configuration);

            // Add MVC services to the services container.
            services.AddMvc();

            // Uncomment the following line to add Web API servcies which makes it easier to port Web API 2 controllers.
            // You need to add Microsoft.AspNet.Mvc.WebApiCompatShim package to project.json
            // services.AddWebApiConventions();

        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory)
        {
            // Configure the HTTP request pipeline.
            // Add the console logger.
            loggerfactory.AddConsole();

            // Add the following to the request pipeline only in development environment.
            if (string.Equals(env.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
            {
                app.UseBrowserLink();
                app.UseErrorPage(ErrorPageOptions.ShowAll);
                app.UseDatabaseErrorPage(DatabaseErrorPageOptions.ShowAll);
            }
            else
            {
                // Add Error handling middleware which catches all application specific errors and
                // send the request to the following path or controller action.
                app.UseErrorHandler("/Home/Error");
            }

            // Add static files to the request pipeline.
            var options = new StaticFileOptions();
            options.ContentTypeProvider = new CustomContentTypeProvider();
            options.ServeUnknownFileTypes = false;
            options.OnPrepareResponse = _ =>
            {
                string[] valid = new[] { ".js", ".css", ".ico", ".png", ".gif", ".jpg", ".svg", ".woff", ".woff2", ".ttf", ".eot" };
                string ext = System.IO.Path.GetExtension(_.File.Name).ToLowerInvariant();
                if (valid.Contains(ext))
                {
                    _.Context.Response.Headers.Add("cache-control", new string[] { "max-age=31536000" });
                }
            };
            //app.UseStaticFiles(options);

            // Add cookie-based authentication to the request pipeline.
            app.UseIdentity();

            // Add MVC to the request pipeline.
            app.UseMvc(routes =>
            {
                //routes.MapRoute(
                //    name: "extension",
                //    template: "home/extension/{id}/{version}");

                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action}/{id?}/{version?}");

                //defaults: new { controller = "Home", action = "Index" });

                // Uncomment the following line to add a route for porting Web API 2 controllers.
                // routes.MapWebApiRoute("DefaultApi", "api/{controller}/{id?}");
            });
        }
    }

    public class CustomContentTypeProvider : FileExtensionContentTypeProvider
    {
        public CustomContentTypeProvider()
        {
            Mappings[".html"] = "text/html;charset=utf-8";
            Mappings[".js"] = "text/javascript";
            Mappings[".ico"] = "image/x-icon";
        }
    }
}