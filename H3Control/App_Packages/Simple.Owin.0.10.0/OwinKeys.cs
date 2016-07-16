﻿namespace Simple.Owin
{
    internal static class OwinKeys
    {
        public static class Host
        {
            /// <summary>
            /// Required, type is TextWriter.
            /// </summary>
            public const string TraceOutput = "host.TraceOutput";
        }

        public static class Owin
        {
            /// <summary>
            /// Required, type is CancellationToken.
            /// </summary>
            public const string CallCancelled = "owin.CallCancelled";

            /// <summary>
            /// Required, type is string.
            /// </summary>
            public const string Version = "owin.Version";
        }

        public static class Request
        {
            /// <summary>
            /// Required, type is Stream.
            /// </summary>
            public const string Body = "owin.RequestBody";

            /// <summary>
            /// Required, type is IDictionary&lt;string, string[]>.
            /// </summary>
            public const string Headers = "owin.RequestHeaders";

            /// <summary>
            /// Required, type is string.
            /// </summary>
            public const string Method = "owin.RequestMethod";

            /// <summary>
            /// Required, type is string.
            /// The server MUST provide percent-decoded value. 
            /// </summary>
            public const string Path = "owin.RequestPath";

            /// <summary>
            /// Required, type is string.
            /// The server MUST provide percent-decoded value. 
            /// </summary>
            public const string PathBase = "owin.RequestPathBase";

            /// <summary>
            /// Required, type is string.
            /// </summary>
            public const string Protocol = "owin.RequestProtocol";

            /// <summary>
            /// Required, type is string.
            /// The server MUST provide percent-encoded value. 
            /// </summary>
            public const string QueryString = "owin.RequestQueryString";

            /// <summary>
            /// Required, type is string.
            /// </summary>
            public const string Scheme = "owin.RequestScheme";
        }

        public static class Response
        {
            /// <summary>
            /// Required, type is Stream.
            /// </summary>
            public const string Body = "owin.ResponseBody";

            /// <summary>
            /// Required, type is IDictionary&lt;string, string[]>.
            /// </summary>
            public const string Headers = "owin.ResponseHeaders";

            /// <summary>
            /// Optional, type is string.
            /// </summary>
            public const string Protocol = "owin.ResponseProtocol";

            /// <summary>
            /// Optional, type is string.
            /// </summary>
            public const string ReasonPhrase = "owin.ResponseReasonPhrase";

            /// <summary>
            /// Optional, type is int.
            /// </summary>
            public const string StatusCode = "owin.ResponseStatusCode";
        }

        public static class SendFile
        {
            public const string Async = "sendfile.SendAsync";
        }

        public static class Server
        {
            /// <summary>
            /// Optional, type is IDictionary&lt;string, object>.
            /// </summary>
            public const string Capabilities = "server.Capabilities";

            /// <summary>
            /// Optional, type is bool.
            /// </summary>
            public const string IsLocal = "server.IsLocal";

            /// <summary>
            /// Optional, type is string (IPAddress).
            /// </summary>
            public const string LocalIpAddress = "server.LocalIpAddress";

            /// <summary>
            /// Optional, type is string (int).
            /// </summary>
            public const string LocalPort = "server.LocalPort";

            /// <summary>
            /// Optional, type is string.
            /// </summary>
            public const string Name = "server.Name";

            /// <summary>
            /// Optional, type is Action&lt;Action&lt;object>, object>.
            /// </summary>
            public const string OnSendingHeaders = "server.OnSendingHeaders";

            /// <summary>
            /// Optional, type is string (IPAddress).
            /// </summary>
            public const string RemoteIpAddress = "server.RemoteIpAddress";

            /// <summary>
            /// Optional, type is string (int).
            /// </summary>
            public const string RemotePort = "server.RemotePort";

            /// <summary>
            /// Optional, type is IUser.
            /// </summary>
            public const string User = "server.User";
        }

        public static class Simple
        {
            public const string Context = "simple.Context";
            public const string Form = "simple.Form";
            public const string FullUri = "simple.FullUri";
            public const string Status = "simple.Status";
        }

        public static class Ssl
        {
            /// <summary>
            /// Optional, type is X509Certificate.
            /// </summary>
            public const string ClientCertifiate = "ssl.ClientCertificate";
        }

        public static class WebSocket
        {
            public const string Func = "websocket.Func";
            public const string Support = "websocket.Support";
            public const string Version = "websocket.Version";
        }
    }
}