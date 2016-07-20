namespace Universe
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Simple.Owin.Helpers;

    public static class HttpClientExtentions
    {
        public static Task<string> GetAsString(this HttpClient client, string url)
        {
            Task<HttpResponseMessage> taskGet;
            try
            {
                taskGet = client.GetAsync(url);
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format("URL '{0}' is not suitable with HttpClient", url), "url", ex);
            }

            Task<string> taskRead = taskGet.Then(x =>
            {
                
                if (x.StatusCode != HttpStatusCode.OK)
                    throw new InvalidOperationException(string.Format(
                        "Status of '{0}' response isn't OK(200). Status is {1}. Phrase is '{2}'", 
                        url, 
                        x.StatusCode, 
                        x.ReasonPhrase));
                
                return x.Content.ReadAsStringAsync();
            });

            return taskRead;
        }
    }
}