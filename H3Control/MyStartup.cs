using System;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(H3Control.PureOwinStartup))]

namespace H3Control
{
    using System.IO;
    using System.Reflection;
    using System.Web.Http;

    using Common;

    using Microsoft.Owin.FileSystems;
    using Microsoft.Owin.StaticFiles;

    using Universe;

    public class PureOwinStartup
    {
        public void Configuration(IAppBuilder app)
        {
            // app.UseWelcomePage("/owin");

            try
            {
                DeviceDataSource.GetLocal();
            }
            catch{}

            Action<string> trace = dump =>
                FirstRound.Only("OWIN request dump", RoundCounter.Twice, () =>
                {
                    NiceTrace.Message(dump);
                });


            app.Use(typeof(TraceMiddleware), trace);
            app.Use(typeof(WhiteListMiddleware), H3WhiteListConfig.WhiteList);
            

/*
            app.UseWelcomePage(new Microsoft.Owin.Diagnostics.WelcomePageOptions()
            {
                Path = new PathString("/welcome")
            });
*/

            var webPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "web");
            app.UseFileServer(new FileServerOptions()
            {
                RequestPath = PathString.FromUriComponent(""),
                FileSystem = new PhysicalFileSystem(webPath),
                EnableDefaultFiles = true,
                EnableDirectoryBrowsing = true,
            });

            {
                HttpConfiguration config = new HttpConfiguration();
                config.Routes.MapHttpRoute("xmlapi", "api/xml/{controller}/{device}", new {controller = "Device", device = RouteParameter.Optional});
                config.Formatters.XmlFormatter.UseXmlSerializer = true;
                config.Formatters.Remove(config.Formatters.JsonFormatter);
                // config.Formatters.JsonFormatter.UseDataContractJsonSerializer = true;
                app.UseWebApi(config);
            }

            {
                HttpConfiguration config = new HttpConfiguration();

                config.Routes.MapHttpRoute(
                    "jsonapi-ping",
                    "api/json/ping",
                    new
                    {
                        controller = "Device",
                        action = "Ping",
                    });

                config.Routes.MapHttpRoute(
                    "jsonapi",
                    "api/json/{controller}/{device}",
                    new
                    {
                        controller = "Device",
                        action = "GetDevice",
                        device = RouteParameter.Optional
                    });


                config.Formatters.XmlFormatter.UseXmlSerializer = false;
                config.Formatters.Remove(config.Formatters.XmlFormatter);
                app.UseWebApi(config);
            }

            {
                // api/control/cpu-min|max/480
                HttpConfiguration config = new HttpConfiguration();
                config.Routes.MapHttpRoute(
                    "controlapi", 
                    "api/{controller}/{side}/{freq}", 
                    new { controller = "Control", action = "Control",
                          side = RouteParameter.Optional,
                          freq = RouteParameter.Optional
                    });

                config.Formatters.XmlFormatter.UseXmlSerializer = false;
                config.Formatters.Remove(config.Formatters.XmlFormatter);
                app.UseWebApi(config);
            }


            
            app.Run(context =>
            {
                context.Response.ContentType = "text/plain";

                string output = string.Format(
                    "h3control, OS {0}, version {1}",
                    Environment.OSVersion,
                    Assembly.GetEntryAssembly().GetName().Version
                    );

                return context.Response.WriteAsync(output);

            });

            // Console.WriteLine("OWIN configuration completed");

        }
    }
}