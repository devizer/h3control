namespace H3Control
{
    using System;

    using Nancy;
    using Nancy.Conventions;

    public class StaticContentConventionBuilderAddOn
    {
        public static Func<NancyContext, string, Response> AddDirectoryWithExpiresHeader(
                string requestedPath,
                TimeSpan expiresTimeSpan,
                string contentPath = null,
                params string[] allowedExtensions)
        {
            var responseBuilder = StaticContentConventionBuilder
               .AddDirectory(requestedPath, contentPath, allowedExtensions);

            return (context, root) =>
            {
                var response = responseBuilder(context, root);
                if (response != null)
                {
                    response.Headers.Add("Expires", DateTime.Now.Add(expiresTimeSpan).ToString("R"));
                }
                return response;
            };
        }          
    }
}