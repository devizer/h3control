using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Nancy;
using Nancy.Owin;

namespace Universe
{
    public static class NancyModuleExtensions
    {
        [CanBeNull, Pure]
        public static string GetServerIp(this NancyModule nancyModule)
        {
            string ret = null;
            object rawOwinEnv;
            if (nancyModule.Context.Items.TryGetValue(NancyMiddleware.RequestEnvironmentKey, out rawOwinEnv))
            {
                var d = (IDictionary<string, object>) rawOwinEnv;
                object serverIp;
                if (d.TryGetValue("server.LocalIpAddress", out serverIp))
                {
                    ret = Convert.ToString(serverIp);
                }
            }

            return string.IsNullOrEmpty(ret) ? null: ret;
        }
    }
}