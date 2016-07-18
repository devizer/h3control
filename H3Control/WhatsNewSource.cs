namespace H3Control
{
    using System;
    using System.Net.Http;

    using CommonMark;

    class WhatsNewSource
    {
        public static string GetMarkDown()
        {
            string url = "https://github.com/devizer/h3control-bin/raw/master/WHATS-NEW.md";
            var httpClient = new HttpClient();
            string ret = httpClient.GetStringAsync(url + "?" + Guid.NewGuid().ToString("N")).Result;
            return ret;
        }

        public static string GetHtml()
        {
            string md = GetMarkDown();
            return MarkdownTo.Html(md);
            var copy = CommonMarkSettings.Default.Clone();
            copy.OutputFormat = OutputFormat.Html;
            copy.AdditionalFeatures = CommonMarkAdditionalFeatures.All;
            copy.RenderSoftLineBreaksAsLineBreaks = true;
            return CommonMark.CommonMarkConverter.Convert(md, copy);
        }

       

        public static string @DraftTemplate = @"
<html lang='en' xmlns='http://www.w3.org/1999/xhtml'>
<head>
<!--
<link rel='stylesheet' href='/Content/reset.css' type='text/css' />
-->
<link rel='stylesheet' href='/Content/commonmark.css' type='text/css' />
</head>
<body>
<div class='markdown'>
${CONTENT}
</div>
</body>
";
    }
}