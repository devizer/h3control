namespace Universe
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Microsoft.Ajax.Utilities;

    using Simple.Owin.Helpers;

    public static class HttpClientExtentions
    {
        public static Task<string> GetAsStringAsync(this HttpClient client, string url)
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

            Task<string> taskRead = taskGet.Then(response =>
            {
                response.EnsureSuccessStatusCode();
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new InvalidOperationException(string.Format(
                        "Status of '{0}' response isn't OK(200). Status is {1}. Phrase is '{2}'", 
                        url, 
                        response.StatusCode, 
                        response.ReasonPhrase));
                
                return response.Content.ReadAsStringAsync();
            });

            return taskRead;
        }
    }

    public static class TasksExtentions
    {
        // Process should not crash if arg fails
        public static Task<T> TouchException<T>(this Task<T> arg)
        {
            arg.ContinueWith(delegate(Task<T> t)
            {
                var ex = t.Exception;
                return t.Result;
            }, TaskContinuationOptions.NotOnFaulted);

            return arg;
        }

        // Process should not crash if arg fails
        public static Task TouchException(this Task arg)
        {
            arg.ContinueWith(delegate(Task t)
            {
                var ex = t.Exception;
            }, TaskContinuationOptions.NotOnFaulted);

            return arg;
        }

        // Process should not crash if arg fails
        public static Task TraceException(this Task arg, string taskTitle)
        {
            arg.ContinueWith(delegate(Task t)
            {
                var ex = t.Exception;
                Trace.WriteLine("Task '" + taskTitle + "' failed." + Environment.NewLine + ex);
            }, TaskContinuationOptions.NotOnFaulted);

            return arg;
        }

        // Process should not crash if arg fails
        public static Task DebugException(this Task arg, string taskTitle)
        {
            arg.ContinueWith(delegate(Task t)
            {
                var ex = t.Exception;
                Debug.WriteLine("Task '" + taskTitle + "' failed." + Environment.NewLine + ex);
            }, TaskContinuationOptions.NotOnFaulted);

            return arg;
        }

    }
}