using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Control
{
    using System.Diagnostics;

    using Microsoft.Owin;

    using Universe;

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

            DumpRequest(context);

            // Console.WriteLine("Im White List. Ip is " + ipAddress);

            if (_whiteList != null && _whiteList.Count > 0 && ipAddress != null && !_whiteList.Contains(ipAddress))
            {
                response.StatusCode = 403;
                return context.Response.WriteAsync("403: Access denied by administrator");
            }

            return Next.Invoke(context);
        }

        [Conditional("DEBUG")]
        private static void DumpRequest(IOwinContext context)
        {
            NiceTrace.Message("");
            var env = context.Request.Environment;
            object owinRequestPath;
            env.TryGetValue("owin.RequestPath", out owinRequestPath);
            if (owinRequestPath != null) NiceTrace.Message("REQUEST " + owinRequestPath);

            foreach (var k in env.Keys.OrderBy(x => x))
            {
                NiceTrace.Message("  {0}: {1}", k, env[k]);
            }

            var caps = context.Request.Environment["server.Capabilities"] as Dictionary<string, object>;
            if (caps != null)
            {
                NiceTrace.Message("  Capabilities::");
                foreach (var k in caps.Keys.OrderBy(x => x))
                {
                    NiceTrace.Message("  |   {0}: {1}", k, caps[k]);
                }
            }
        }
    }
}
