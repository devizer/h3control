namespace Universe
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;

    using Nancy;
    using Nancy.Bootstrapper;
    using Nancy.Security;

    public static class BasicAuthenticationExtentions
    {

        private static readonly Func<NancyContext, bool> Always = context => true;

        private static readonly Func<string, string, IUserIdentity> Nobody = (login, password) => null;

        private static readonly Func<NancyContext, string> DefaultRealm = context => "Restricted Resources";
        
        public static void RequireBasicAuthentication(
            this IPipelines applicationPipelines,
            Func<NancyContext, bool> isAuthenticationRequired = null,
            Func<string, string, IUserIdentity> tryAuthenticate = null,
            Func<NancyContext, string> getRealmName = null)
        {
            isAuthenticationRequired = isAuthenticationRequired ?? Always;
            tryAuthenticate = tryAuthenticate ?? Nobody;
            getRealmName = getRealmName ?? DefaultRealm;

            applicationPipelines.BeforeRequest.AddItemToEndOfPipeline(delegate(NancyContext context)
            {
                if (true || !context.CurrentUser.IsAuthenticated() && isAuthenticationRequired(context))
                {
                    string login;
                    string password;
                    if (ExtractCredentialsFromHeaders(context.Request, out login, out password))
                    {
                        // NiceTrace.Message("LOGIN: {0}, PASSWORD: {1}", login, password);
                        var user = tryAuthenticate(login, password);
                        if (user != null)
                            context.CurrentUser = user;
                    }
                }
                return null;
            });

            applicationPipelines.AfterRequest.AddItemToEndOfPipeline(delegate(NancyContext context)
            {
                var realm = getRealmName(context);
                if (string.IsNullOrEmpty(realm)) realm = DefaultRealm(context);
                if (isAuthenticationRequired(context) && !context.CurrentUser.IsAuthenticated())
                {
                    context.Response.Headers["WWW-Authenticate"] = String.Format("{0} realm=\"{1}\"", (object) "Basic", realm);
                    context.Response.StatusCode = HttpStatusCode.Unauthorized;
                }
            });

        }

        private static bool ExtractCredentialsFromHeaders(Request request, out string login, out string password)
        {
            login = password = null;
            string authorization = request.Headers.Authorization;

            if (String.IsNullOrEmpty(authorization))
                return false;

            if (!authorization.StartsWith("Basic"))
                return false;

            string[] strArray;
            try
            {
                string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(authorization.Substring("Basic".Length).Trim()));
                strArray = String.IsNullOrWhiteSpace(decoded) ? null : decoded.Split(new[] { ':' }, 2);
            }
            catch (FormatException ex)
            {
                NiceTrace.Message("Wrong Authorization header. {0}{1}", Environment.NewLine, ex);
                strArray = null;
            }

            if (strArray != null && strArray.Length == 2)
            {
                login = strArray[0];
                password = strArray[1];
                return true;
            }

            return false;
        }

        public class SuperSimpleNancyIdentity : Nancy.Security.IUserIdentity
        {
            private string _UserName;

            public SuperSimpleNancyIdentity(string userName)
            {
                if (string.IsNullOrEmpty(userName))
                    throw new ArgumentException("userName is expected", "userName");

                _UserName = userName;
            }

            public string UserName
            {
                get { return _UserName; }
            }

            public IEnumerable<string> Claims
            {
                get { return Enumerable.Empty<string>(); }
            }
        }
    }

}