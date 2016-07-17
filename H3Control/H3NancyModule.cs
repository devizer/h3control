namespace H3Control
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    using Common;
    using Controllers;
    using Links;

    using Microsoft.SqlServer.Server;

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
                return View["default"];
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
                return Response.AsText(H3Environment.Ver.ToString(3));
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

                    Stopwatch sw3 = Stopwatch.StartNew();
                    var list = PsListener_OnLinux.Select(order, topN);
                    var time = sw3.Elapsed;
                    FirstRound.Only("Select Processes", RoundCounter.Twice, () =>
                    {
                        NiceTrace.Message("Processes ({0} msec):{1}{2}",
                            time, Environment.NewLine,
                            string.Join(Environment.NewLine, list.Select(x => "           " + x.ToString())));
                    });

                    // Trim command with args for IE8
                    var browser = this.Context.GetUserAgent();
                    if (browser.Is_IE8_OrBelow)
                        foreach (var pi in list)
                            pi.Args = pi.Args != null && pi.Args.Length > 80 ? (pi.Args.Substring(0, 77) + "...") : pi.Args;

                    return new {Processes = list};
                }
                catch (Exception ex)
                {
                    NiceTrace.Message("PsListener_OnLinux.Select() failed:{0}{1}", Environment.NewLine, ex);
                    return new {Error = ex.Get()};
                }
            };

            Get["api/json/device/{device}"] = parameters =>
            {
                this.Expires(scope: CachingScope.None);
                string device = (string)parameters.device;
                var model = new DeviceController().GetDevice(device);
                model.HasChangeAccess = !PasswordConfig.IsStricted || Context.CurrentUser.IsAuthenticated();
                return Response.AsJson(model);
            };

            Post["api/control/{side}/{freq}"] = parameters =>
            {
                if (Context.CurrentUser.IsAuthenticated())
                    NiceTrace.Message("CONTROL action recieved from by user '{0}': {1}", Context.CurrentUser.UserName, Request.Url.Path);

                if (PasswordConfig.IsStricted && !Context.CurrentUser.IsAuthenticated())
                    return 403;

                var side = (string) parameters.side;
                var freq = (string) parameters.freq;
                var ret = new ControlController().Control(side, freq);
                return Response.AsJson(ret); ;
            };

            // NiceTrace.Message("Routing convigured by " + this.GetType().Name);
        }



    }


}