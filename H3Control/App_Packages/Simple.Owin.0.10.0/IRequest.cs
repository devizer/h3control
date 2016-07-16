using System;
using System.IO;
using System.Security.Principal;

namespace Simple.Owin
{
    /// <summary>
    /// Abstraction for an HTTP request
    /// </summary>
    internal interface IRequest
    {
        /// <summary>
        /// Gets the input body stream.
        /// </summary>
        Stream Body { get; }

        IFormData FormData { get; set; }

        /// <summary>
        /// Gets the URL.
        /// </summary>
        Uri FullUri { get; }

        /// <summary>
        /// Gets the request headers.
        /// </summary>
        IRequestHeaders Headers { get; }

        /// <summary>
        /// Gets the HTTP method.
        /// </summary>
        string Method { get; }

        string Path { get; }

        string PathBase { get; }

        string Protocol { get; }

        /// <summary>
        /// Gets the query string.
        /// </summary>
        QueryString QueryString { get; }

        string Scheme { get; }

        IPrincipal User { get; set; }
    }
}