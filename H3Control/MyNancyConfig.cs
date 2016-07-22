namespace H3Control
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Text.RegularExpressions;

    using Nancy;
    using Nancy.Bootstrapper;
    using Nancy.Conventions;
    using Nancy.Diagnostics;
    using Nancy.Responses;
    using Nancy.Security;
    using Nancy.Serialization.JsonNet;
    using Nancy.TinyIoc;
    using Nancy.ViewEngines.Razor;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json.Serialization;

    using Universe;
    using Universe.NancyCaching;


    public class MyRazorConfig : IRazorConfiguration
    {
        public IEnumerable<string> GetAssemblyNames()
        {
            yield return Assembly.GetExecutingAssembly().GetName().Name;
            yield return "Cassette";
        }

        public IEnumerable<string> GetDefaultNamespaces()
        {
            yield return typeof (H3Main).Namespace;
        }

        public bool AutoIncludeModelNamespace
        {
            get { return true; }
        }
    }

    public class H3NancyBootstrapper : DefaultNancyBootstrapper
    {
        public H3NancyBootstrapper()
        {
            var l1 = Debug.Listeners.OfType<TraceListener>().ToList();
            var l2 = Trace.Listeners.OfType<TextWriterTraceListener>().ToList();
            // foreach (var l in l1) Debug.Listeners.Remove(l);
            foreach (var l in l2) Trace.Listeners.Remove(l);
        }

        protected override void ConfigureApplicationContainer(Nancy.TinyIoc.TinyIoCContainer container)
        {
            base.ConfigureApplicationContainer(container);
            // container.Register(typeof(JsonSerializer), typeof(JsonNetSerializer));
        }

        protected override DiagnosticsConfiguration DiagnosticsConfiguration
        {
            get
            {
                return new DiagnosticsConfiguration { Password = "pass"};
            }
        }

        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            StaticConfiguration.DisableErrorTraces = false;

            pipelines.OnError += (context, exception) =>
            {
                NiceTrace.Message("Exception thrown during request handling of " + context.Request.Url + Environment.NewLine + exception + Environment.NewLine);
                return null;
            };

            Func<NancyContext, bool> isAuthenticationRequired = delegate(NancyContext nancyContext)
            {
                // We ask authentication only for default page
                return H3PasswordConfig.IsStricted && nancyContext.Request.Url.Path == "/";
            };

            Func<string, string, IUserIdentity> tryAuthenticate = (login, password) =>
            {
                return HashExtentions.SHA1(password).Equals(H3PasswordConfig.Hash)
                    ? new BasicAuthenticationExtentions.SuperSimpleNancyIdentity(login)
                    : null;
            };

            pipelines.RequireBasicAuthentication(
                isAuthenticationRequired,
                tryAuthenticate,
                context => "H3 Control is restricted by Administrator. Press Escape for readonly access");

            pipelines.BindExpiration();

            pipelines.BindUserAgentInfo();
        }


        protected override byte[] FavIcon
        {
            get
            {
                var s = typeof (H3Main).Assembly.GetManifestResourceStream("H3Control.favicon.ico");
                if (s == null)
                    throw new InvalidOperationException("Resource 'H3Control.web.favicon.ico' not found");

                using (s)
                {
                    MemoryStream mem = new MemoryStream();
                    s.CopyTo(mem);
                    return mem.ToArray();
                }

            }
        }

        protected override IRootPathProvider RootPathProvider
        {
            get { return new H3RootPathProvider(); }
        }

        protected override void ConfigureConventions(NancyConventions nancyConventions)
        {
            base.ConfigureConventions(nancyConventions);

            var rootPath = RootPathProvider.GetRootPath();
            var root = new DirectoryInfo(rootPath);
            var folders = root.GetDirectories();
            foreach (var directoryInfo in folders)
            {
                var name = directoryInfo.Name;
                // Without expired
                // nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory(n, n));

                // With expired
                NiceTrace.Message("ADDED static Folder: " + name);
                var responseBuilder = StaticContentConventionBuilderAddOn
                    .AddDirectoryWithExpiresHeader(name, TimeSpan.FromDays(10));

                nancyConventions.StaticContentsConventions.Add(responseBuilder);
            }

            var files = root.GetFiles();
            var exclude = new[] {".cshtml"};
            foreach (var fileInfo in files)
            {
                string name = fileInfo.Name;
                if (!exclude.Any(x => x.Equals(Path.GetExtension(name), StringComparison.InvariantCultureIgnoreCase)))
                {
                    NiceTrace.Message("ADDED static File: " + name);
                    nancyConventions
                        .StaticContentsConventions
                        .Add(StaticContentConventionBuilder.AddFile("/" + name, name));
                }
            }

        }
    }


    public class H3RootPathProvider : IRootPathProvider
    {
        public string GetRootPath()
        {
            // return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var webPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "web");
            return webPath;
        }
    }
}
