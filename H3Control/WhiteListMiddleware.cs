using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universe
{
    using System.Diagnostics;

    using Microsoft.Owin;

    using Universe;

    public class TraceMiddleware : OwinMiddleware
    {
        private Action<string> _notifyRequest;

        public TraceMiddleware(OwinMiddleware next, Action<string> notifyRequest) :
            base(next)
        {
            _notifyRequest = notifyRequest;
        }

        public override Task Invoke(IOwinContext context)
        {
            DumpRequest(context);
            return Next.Invoke(context);
        }

        private void DumpRequest(IOwinContext context)
        {
            StringBuilder dump = new StringBuilder();
            var env = context.Request.Environment;
            object owinRequestPath;
            env.TryGetValue("owin.RequestPath", out owinRequestPath);
            if (owinRequestPath != null) dump.AppendLine("Owin REQUEST " + owinRequestPath);

            foreach (var k in env.Keys.OrderBy(x => x))
            {
                dump.AppendFormat("  {0}: {1}", k, env[k]).AppendLine();
            }

            var caps = context.Request.Environment["server.Capabilities"] as Dictionary<string, object>;
            if (caps != null)
            {
                dump.AppendLine("  Capabilities::");
                foreach (var k in caps.Keys.OrderBy(x => x))
                {
                    dump.AppendFormat("  |   {0}: {1}", k, caps[k]).AppendLine();
                }
            }

            // headersRaw is internal Microsoft.Owin.Host.HttpListener.RequestProcessing.RequestHeadersDictionary
            var headersRaw = context.Request.Environment["owin.RequestHeaders"];
            IDictionary<string, string[]> headers = context.Request.Environment["owin.RequestHeaders"] as IDictionary<string, string[]>;
            if (headers != null)
            {
                dump.AppendLine("  Request headers::");
                foreach (var key in headers.Keys)
                {
                    string arrAsString =
                        IsRelease && key == "Authorization"
                            ? "************ protected *************"
                            : string.Join(", ", (headers[key] ?? new string[0]).Select(x => "'" + x + "'"));

                    dump.AppendFormat("   \"{0}\": {1}",
                        key,
                        arrAsString)
                        .AppendLine();
                }
            }

            if (_notifyRequest != null)
                _notifyRequest(dump.ToString());
        }

        public static bool IsRelease
        {
            get
            {
#if DEBUG
                return false;
#else
                return true;
#endif
            }
        }

    }



    public class WhiteListMiddleware : OwinMiddleware
    {

        private readonly HashSet<string> _whiteList;

        public WhiteListMiddleware(OwinMiddleware next, HashSet<string> whiteList) :
            base(next)
        {
            _whiteList = whiteList;
        }

        public override Task Invoke(IOwinContext context)
        {
            var request = context.Request;
            var response = context.Response;
            var ipAddress = (string) request.Environment["server.RemoteIpAddress"];

            if (_whiteList != null && _whiteList.Count > 0 && ipAddress != null && !_whiteList.Contains(ipAddress))
            {
                response.StatusCode = 403;
                response.ReasonPhrase = "Access forbidden by administrator";
                return context.Response.WriteAsync("403: Access forbidden by administrator");
            }

            return Next.Invoke(context);
        }

    }
}
