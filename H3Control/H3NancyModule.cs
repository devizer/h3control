namespace H3Control
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;

    using Common;

    using Controllers;
    using Links;

    using Nancy;
    using Nancy.Security;

    using Simple.Owin;

    using Universe;
    using Universe.NancyCaching;

    public class H3NancyModule : NancyModule
    {
        public H3NancyModule()
        {

            Stopwatch sw = null;
            Before += delegate(NancyContext ctx)
            {
                // NiceTrace.Message("Before {0}", ctx.Request.Url);
                sw = Stopwatch.StartNew();
                return null;
            };

            After += delegate(NancyContext ctx)
            {
                FirstRound.Only("Request: " + ctx.Request.Url.Path, RoundCounter.Twice, () =>
                {
                    if (sw != null)
                        NiceTrace.Message(
                            "Request handled in {0:0000.000} secs: {1}",
                            sw.ElapsedMilliseconds / 1000m,
                            ctx.Request.Url.Path);
                });
            };
            
            Get["/"] = _ =>
            {
                this.Expires(scope:CachingScope.None);
                var model = new DeviceController().GetDevice("me");
                model.HasChangeAccess = !H3PasswordConfig.IsStricted || Context.CurrentUser.IsAuthenticated();
                var jsonDevice = JSonExtentions.ToNewtonJSon(model, isIntended: !H3Environment.IsRelease);

                List<PsProcessInfo> plist;
                try
                {
                    plist = ProcessesController.Select(H3Environment.ProcessesDefaults.Order, H3Environment.ProcessesDefaults.TopN, Context.GetUserAgent().Is_IE8_OrBelow);
                }
                catch
                {
                    plist = new List<PsProcessInfo>();
                }
                var jsonProcesses = JSonExtentions.ToNewtonJSon(plist, isIntended: !H3Environment.IsRelease);
                return View["default", new {JSonDevice = jsonDevice, JSonProcesses = jsonProcesses}];
            };


            Func<string, Response> asBadRequest = (message) =>
            {
                var msg = ("Bad Request. " + message).Trim();
                var response = (Response)("Bad Request. " + message).Trim();
                response.StatusCode = HttpStatusCode.BadRequest;
                response.ReasonPhrase = msg;
                return response;
            };

            Get["Ver"] = _ =>
            {
                return Response.AsText(H3Environment.VerAsPublic);
            };

            Get["api/json/processes/by-{column}/top-{top}"] = parameters =>
            {
                this.Expires(scope: CachingScope.None);
                try
                {
                    int topN;
                    if (!int.TryParse((string)parameters.top, out topN))
                        return asBadRequest("Invalid parameter. {top} argument is invalid or absent");

                    PsSortOrder order;
                    if (!Enum.TryParse((string)parameters.column, true, out order))
                        return asBadRequest("Invalid parameter. {column} argument is invalid or absent");

                    var browser = this.Context.GetUserAgent();
                    var isIe8OrBelow = browser.Is_IE8_OrBelow;


                    var list = ProcessesController.Select(order, topN, isIe8OrBelow);

                    return Response.AsJson(new { Processes = list });
                    return new { Processes = list };
                }
                catch (Exception ex)
                {
                    NiceTrace.Message("PsListener_OnLinux.Select() failed:{0}{1}", Environment.NewLine, ex);
                    return Response.AsJson(new {Error = ex.Get()});
                }
            };

            Get["api/json/device/{device}"] = parameters =>
            {
                this.Expires(scope: CachingScope.None);
                string device = (string)parameters.device;
                var model = new DeviceController().GetDevice(device);
                model.HasChangeAccess = !H3PasswordConfig.IsStricted || Context.CurrentUser.IsAuthenticated();
                return Response.AsJson(model);
            };

            Post["flush/kernel/cache"] = parameters =>
            {
                this.Expires(scope: CachingScope.None);
                LinuxKernelCache.Flush("by user request");
                return Response.AsJson(new {OK = true});
            };

            Post["api/control/{side}/{freq}"] = parameters =>
            {
                if (Context.CurrentUser.IsAuthenticated())
                    NiceTrace.Message("CONTROL action recieved from by user '{0}': {1}", Context.CurrentUser.UserName, Request.Url.Path);

                if (H3PasswordConfig.IsStricted && !Context.CurrentUser.IsAuthenticated())
                    return 403;

                var side = (string) parameters.side;
                var freq = (string) parameters.freq;
                var ret = new ControlController().Control(side, freq);
                return Response.AsJson(ret); ;
            };

            Get["/whatsnew/markdown"] = parameters =>
            {
                this.Expires(scope: CachingScope.None);
                var ret = Response.AsText(NewVerListener.WhatsNewMd);
                ret.ContentType = "text/plain";
                return ret;
            };

            Get["/whatsnew/html-include"] = parameters =>
            {
                this.Expires(scope: CachingScope.None);
                var md = NewVerListener.WhatsNewMd;
                var html = MarkdownTo.Html(md);
                var ret = Response.AsText(html);
                ret.ContentType = "text/plain";
                return ret;
            };


            Get["/whatsnew/html"] = parameters =>
            {
                this.Expires(scope: CachingScope.None);
                var md = NewVerListener.WhatsNewMd;
                var htmlFragment = MarkdownTo.Html(md);
                string html = WhatsNewSource.DraftTemplate.Replace("${CONTENT}", htmlFragment);
                var ret = Response.AsText(html);
                ret.ContentType = "text/html";
                return ret;
            };

            // V:\_GIT\h3control-bin\ 
            Get["/demo"] = parameters =>
            {
                this.Expires(scope: CachingScope.None);
                string url = "https://github.com/devizer/h3control-bin/raw/master/info/markdown-test.md";
                string md = new HttpClient().GetStringAsync(url + "?" + Guid.NewGuid().ToString("N")).Result;
                string html = MarkdownTo.Html(md);
                return AsMarkdown(html);
            };

            Get["/error"] = parameters =>
            {
                throw new InvalidOperationException("Requested exception thrown");
            };


            // NiceTrace.Message("Routing convigured by " + this.GetType().Name);
        }


        private dynamic AsMarkdown(string content)
        {
            string html = WhatsNewSource.DraftTemplate.Replace("${CONTENT}", content);
            var ret = Response.AsText(html);
            ret.ContentType = "text/html";
            return ret;
            
        }


    }
}