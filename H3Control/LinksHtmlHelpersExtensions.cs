namespace H3Control
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using Nancy.ViewEngines.Razor;

    public static class LinksHtmlHelpersExtensions
    {
        public static CssLinks Css(this HtmlHelpers helpers, NancyRazorViewBase<dynamic> view)
        {
            var path = view.Path ?? view.Request.Path;
            return new CssLinks(path);
        }

        public static CssLinks Css(this HtmlHelpers helpers, NancyRazorViewBase view)
        {
            var path = view.Path ?? view.Request.Path;
            return new CssLinks(path);
        }

        public static ScriptLinks Script(this HtmlHelpers helpers, NancyRazorViewBase<dynamic> view)
        {
            var path = view.Path ?? view.Request.Path;
            return new ScriptLinks(path);
        }

        public static ScriptLinks Script(this HtmlHelpers helpers, NancyRazorViewBase view)
        {
            var path = view.Path ?? view.Request.Path;
            return new ScriptLinks(path);
        }

        public static HrefLinks HRef(this HtmlHelpers helpers, NancyRazorViewBase<dynamic> view)
        {
            var path = view.Path ?? view.Request.Path;
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
                var dir = new MyRootPathProvider().GetRootPath();
                List<string> parts = new List<string>();
                parts.Add(dir);
                if (!relUri.StartsWith("/") && _basePath != "" && _basePath != "/") parts.Add(_basePath);
                parts.Add(relUri);
                string absFile = Path.Combine(parts.ToArray());

                string changed = "";
                var info = new FileInfo(absFile);
                if (info.Exists)
                    changed = "?___" + info.LastWriteTimeUtc.ToString("yyyy'-'MMMM'-'dd'('HH'-'mm'-'ss.fff')'", new CultureInfo("en-US"));

                return relUri + changed;
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