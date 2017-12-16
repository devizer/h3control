namespace H3Control
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Nancy;
    using Nancy.ViewEngines.Razor;


    public class MyRazorConfig : IRazorConfiguration
    {
        public IEnumerable<string> GetAssemblyNames()
        {
            yield return Assembly.GetExecutingAssembly().GetName().Name;
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
