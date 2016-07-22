namespace H3Control
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Nancy.ViewEngines.Razor;

    public static class LinksHtmlHelpersExtensions
    {
        public static CssLinks Css(this HtmlHelpers<dynamic> helpers, NancyRazorViewBase<dynamic> view)
        {
            var url = view.Url;
            var rc = url.RenderContext;
            var url2 = rc.Context.Request.Url;
            var path = url2.Path;
            return new CssLinks(path);
        }

        public static CssLinks Css(this HtmlHelpers<dynamic> helpers, NancyRazorViewBase view)
        {
            var url = view.Url;
            var rc = url.RenderContext;
            var url2 = rc.Context.Request.Url;
            var path = url2.Path;
            return new CssLinks(path);
        }

        public static ScriptLinks Script(this HtmlHelpers<dynamic> helpers, NancyRazorViewBase<dynamic> view)
        {
            var url = view.Url;
            var rc = url.RenderContext;
            var url2 = rc.Context.Request.Url;
            var path = url2.Path;
            return new ScriptLinks(path);
        }

        public static ScriptLinks Script(this HtmlHelpers<dynamic> helpers, NancyRazorViewBase view)
        {
            var url = view.Url;
            var rc = url.RenderContext;
            var url2 = rc.Context.Request.Url;
            var path = url2.Path;
            return new ScriptLinks(path);
        }

        public static HrefLinks HRef(this HtmlHelpers<dynamic> helpers, NancyRazorViewBase<dynamic> view)
        {
            var url = view.Url;
            var rc = url.RenderContext;
            var url2 = rc.Context.Request.Url;
            var path = url2.Path;
            return new HrefLinks(path);
        }

    }

    public class HrefLinks
    {
        private readonly string _basePath;

        public HrefLinks(string basePath)
        {
            _basePath = basePath;
        }

        public string this[string relUri]
        {
            get
            {
                string fake = "http://host" + _basePath;
                Uri uriBase = new Uri("http://host" + _basePath);
                Uri uri = new Uri(uriBase, relUri);
                var uriPath = uri.LocalPath;
                var root = new H3RootPathProvider().GetRootPath();
                List<string> parts = new List<string>();
                parts.Add(root);
                bool isWin = Environment.OSVersion.Platform == PlatformID.Win32NT;
                string append = uriPath.TrimStart('/');
                parts.Add(isWin ? append.Replace("/", "\\") : append);
                string absFile = Path.Combine(parts.ToArray());

                string changed = "";
                var info = new FileInfo(absFile);
                string prefix = relUri.Contains("?") ? "&" : "?";
                if (info.Exists)
                    changed = prefix + "___" + info.LastWriteTimeUtc.ToString("yyyy'-'MMMM'-'dd'('HH'-'mm'-'ss.fff')'", new CultureInfo("en-US"));

                var ret = relUri + changed;
                return ret;
            }


        }
    }

    public class ScriptLinks
    {
        private readonly string _basePath;

        public ScriptLinks(string basePath)
        {
            _basePath = basePath;
        }

        public NonEncodedHtmlString this[string relUri]
        {
            get
            {
                var href = new HrefLinks(_basePath)[relUri];
                return "<script src='" + href + "'></script>";
            }


        }
    }

    public class ScriptLinksAsString
    {
        private readonly string _basePath;

        public ScriptLinksAsString(string basePath)
        {
            _basePath = basePath;
        }

        public string this[string relUri]
        {
            get
            {
                var href = new HrefLinks(_basePath)[relUri];
                return "<script src='" + href + "'></script>";
            }


        }
    }

    public class CssLinks
    {
        private readonly string _basePath;

        public CssLinks(string basePath)
        {
            _basePath = basePath;
        }

        public NonEncodedHtmlString this[string relUri]
        {
            get
            {
                var href = new HrefLinks(_basePath)[relUri];
                return "<link rel='stylesheet' href='" + href + "' type='text/css' />";
            }


        }
    }

    public class CssLinksAsString
    {
        private readonly string _basePath;

        public CssLinksAsString(string basePath)
        {
            _basePath = basePath;
        }

        public string this[string relUri]
        {
            get
            {
                var href = new HrefLinks(_basePath)[relUri];
                return "<link rel='stylesheet' href='" + href + "' type='text/css' />";
            }


        }
    }

}