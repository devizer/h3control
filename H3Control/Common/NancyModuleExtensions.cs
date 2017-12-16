using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Nancy;

namespace Universe
{
    public static class NancyModuleExtensions
    {
        [CanBeNull, Pure]
        public static string GetServerIp(this NancyModule nancyModule)
        {
            string ret = null;
            object rawEnv;
            if (nancyModule.Context.Items.TryGetValue("OWIN_REQUEST_ENVIRONMENT", out rawEnv))
            {
                var d = (IDictionary<string, object>) rawEnv;
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