using Cassette;
using Cassette.Scripts;
using Cassette.Stylesheets;

namespace H3Control
{
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Universe;

    /// <summary>
    /// Configures the Cassette asset bundles for the web application.
    /// </summary>
    public class CassetteBundleConfiguration : IConfiguration<BundleCollection>
    {
        // Non-existent folder should not be here
        private static readonly string[] JQX =
        {
            "jqxcore.js",
            "jqxdraw.js",
            "jqxgauge.js",
            "jqxexpander.js",
            "jqxmenu.js",
            "jqxnotification.js",
            "jqxbuttons.js",
            "jqxchart.js",
            "jqxdata.js",
            "jqxrating.js",
        };

        public void Configure(BundleCollection bundles)
        {
            NiceTrace.Message("HI! I am CassetteBundleConfiguration. Bundles are " + bundles.Count());

            var files = string.Join(";", JQX.Select(x => x));
            bundles.Add<ScriptBundle>("~/jqwidgets", JQX);


/*
            string[] h3CssFiles = {"H3_Content/h3_commonmark.css", "H3Content/h3.css"};
            bundles.Add<StylesheetBundle>("h3style", h3CssFiles);
*/
            bundles.Add<StylesheetBundle>("~/H3Content");
            bundles.Add<ScriptBundle>("~/H3Content");

/*
            bundles.AddPerSubDirectory<ScriptBundle>("jqwidgets", new FileSearch
            {
                Pattern = string.Join(";", JQX.Select(x => "*" + x)),
                SearchOption = SearchOption.TopDirectoryOnly
            });
*/



            // TODO: Configure your bundles here...
            // Please read http://getcassette.net/documentation/configuration

            // This default configuration treats each file as a separate 'bundle'.
            // In production the content will be minified, but the files are not combined.
            // So you probably want to tweak these defaults!

            // bundles.AddPerIndividualFile<StylesheetBundle>("web/H3Content");
            // bundles.AddPerIndividualFile<ScriptBundle>("Scripts");

            // To combine files, try something like this instead:
            //   bundles.Add<StylesheetBundle>("Content");
            // In production mode, all of ~/Content will be combined into a single bundle.

            // If you want a bundle per folder, try this:
            //   bundles.AddPerSubDirectory<ScriptBundle>("Scripts");
            // Each immediate sub-directory of ~/Scripts will be combined into its own bundle.
            // This is useful when there are lots of scripts for different areas of the website.
        }
    }
}