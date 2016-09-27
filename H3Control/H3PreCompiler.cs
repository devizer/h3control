namespace H3Control
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Web.Configuration;

    using Common;

    using Links;

    using Universe;

    class H3PreCompiler
    {
        
        public static void Compile(string baseUrl)
        {
            if (baseUrl.StartsWith("http://*:") && baseUrl.Length > 9)
                baseUrl = "http://localhost:" + baseUrl.Substring(9);

            Stopwatch sw = Stopwatch.StartNew();
            string[] list = new[]
            {
                baseUrl + "/api/json/device/me", 
                baseUrl + "/H3Content/h3.css",
                baseUrl + "/favicon.ico",
                baseUrl + "/api/json/processes/by-Rss/top-1",
                baseUrl + "/whatsnew/html",
                baseUrl + "/whatsnew/markdown",
                baseUrl + "/whatsnew/html-include",
                baseUrl + "/404",
                baseUrl + "", 
            };

            bool hasErrors = false;
            CountdownEvent done = new CountdownEvent(list.Length);
            StringBuilder report = new StringBuilder();
            foreach (var s in list)
            {
                var url = s;
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    using (WebClient client = new WebClient())
                    {
                        client.Headers["User-Agent"] = "h3control-bootstrapper/" + Assembly.GetExecutingAssembly().GetName().Version;
                        Stopwatch sw1 = Stopwatch.StartNew();
                        try
                        {
                            var data = client.DownloadData(url);
                            lock (report)
                                report
                                    .AppendFormat("   {0}: OK in {1:n0} msec", url, sw1.ElapsedMilliseconds)
                                    .AppendLine();
                        }
                        catch (Exception ex)
                        {
                            hasErrors = true;
                            lock (report)
                                report
                                    .AppendFormat("   {0}: in {1:n0} msec. {2}", url, sw1.ElapsedMilliseconds, ex.Get())
                                    .AppendLine();
                        }

                        NiceTrace.Message("Pre-jitted in {0:0.00} secs: {1}", sw1.ElapsedMilliseconds / 1000m, url);
                        done.Signal();
                    }
                });
            }

            done.Wait();
            if (true || !hasErrors)
            {
                NiceTrace.Message(
                    "{0} routes pre-jitted in {1:n0} msec {2}{3}", 
                    list.Length, 
                    sw.ElapsedMilliseconds, 
                    Environment.NewLine,
                    report);
                    
                ThreadPool.QueueUserWorkItem(state =>
                {
                    Thread.Sleep(5555);
                    LinuxKernelCache.Flush("after startup");
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();
                });
            }
        }
    }
}