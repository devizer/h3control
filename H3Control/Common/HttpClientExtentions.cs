namespace Universe
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;

    using Simple.Owin.Helpers;

    public static class HttpClientExtentions
    {
        public static Task<string> GetAsStringAsync(this HttpClient client, string url)
        {
            if (client == null)
                throw new ArgumentNullException("client");

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
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new HttpRequestException(string.Format(
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
                if (ex != null) ex = ex.Flatten();
                return t.Result;
            }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

            return arg;
        }

        // Process should not crash if arg fails
        public static Task TouchException(this Task arg)
        {
            arg.ContinueWith(delegate(Task t)
            {
                var ex = t.Exception;
                if (ex != null) ex = ex.Flatten();
            }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

            return arg;
        }

        // Process should not crash if arg fails
        public static Task TraceException(this Task arg, string taskTitle)
        {
            arg.ContinueWith(delegate(Task t)
            {
                var ex = t.Exception;
                if (ex != null)
                    Trace.WriteLine("Task '" + taskTitle + "' failed." + Environment.NewLine + ex);

            }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

            return arg;
        }

        // Process should not crash if arg fails
        public static Task DebugException(this Task arg, string taskTitle)
        {
            arg.ContinueWith(delegate(Task t)
            {
                var ex = t.Exception;
                if (ex != null)
                    Debug.WriteLine("Task '" + taskTitle + "' failed." + Environment.NewLine + ex);

            }, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);

            return arg;
        }

    }

    public static class DrunkActionExtentions
    {
        public static void TryAndForget(this Delegate action, params object[] @params)
        {
            try
            {
                action.DynamicInvoke(@params);
            }
            catch
            {
            }
        }

        public static void TryAndForget(this Action action)
        {
            try
            {
                action();
            }
            catch
            {
            }
        }
    }
}