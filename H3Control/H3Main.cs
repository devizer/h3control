using Microsoft.Owin.Hosting;
using System;
using System.Text;
using System.Threading.Tasks;

namespace H3Control
{
    using System.Diagnostics;
    using System.Net;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Web.Configuration;

    using Common;

    using Links;

    using Mono.Unix.Native;

    using NDesk.Options;

    using Owin;

    using Universe;

    public class H3Main
    {
        
        static int Main(string[] args)
        {
            LoadLogger.Bind();
            DebugTraceListener.Bind();
            TraceEnvironment();

            return Exec(args);
        }

        private static int Exec(string[] args)
        {
            int threads;
            int waitThreads;
            ThreadPool.GetMinThreads(out threads, out waitThreads);
            ThreadPool.SetMinThreads(10, waitThreads);

            typeof (WebApiAppBuilderExtensions).ToString().Equals("shit");

            string binding = "*:5000";
            bool help = false, nologo = false, isver = false;
            string generatePasswordHash = null;
            var p = new OptionSet()
            {
                {"b|binding=", "Http binding, e.g. ip:port. Default is *:5000 (asterisk means all IPs)", v => binding = v},
                {"w|white-list=", "Comma separated IPs. Default or empty arg turns restrictions off", v => H3WhiteListConfig.WhiteListArg = v},
                {"g|generate-pwd=", "Generate password hash (encrypt) and exit", v => generatePasswordHash = v},
                {"p|password=", "HASH of the desired password", v => H3PasswordConfig.Hash = v},
                {"v|version", "Show version", v => isver = true},
                {"h|?|help", "Display this help", v => help = v != null},
                {"n|nologo", "Hide logo", v => nologo = v != null}
            };

            p.Parse(args);

            if (!string.IsNullOrEmpty(generatePasswordHash))
            {
                Console.WriteLine(HashExtentions.SHA1(generatePasswordHash.Trim('\r','\n')));
                return 0;
            }

            if (isver)
            {
                Console.WriteLine(H3Environment.VerAsPublic);
                return 0;
            }

            if (!nologo)
            {
                Console.WriteLine("H3Control " + H3Environment.VerAsPublic +
                                  " is a console/daemon which \n   * \"Displays\" temperature, frequency and usage via built-in http server.\n   * Allows to control CPU & DDR frequency");
                Console.WriteLine();
            }

            if (help)
            {
                p.WriteOptionDescriptions(Console.Out);
                return 0;
            }

            NewVerListener.Listen();

            try
            {
                if (H3Environment.IsH3)
                    DeviceDataSource.GetLocal().ToString();
            }
            catch (Exception ex)
            {
                NiceTrace.Message("Device Data Source Failed" + Environment.NewLine + ex);
            }

            try
            {
                CpuUsageListener_OnLinux.Bind(500);
            }
            catch 
            {
            }

            string baseUrl = "http://" + binding;

            StringBuilder cfg = new StringBuilder("Configuration by command line:").AppendLine();
            cfg.AppendLine("  Url is " + baseUrl);
            if (H3WhiteListConfig.HasWhiteList)
                cfg.AppendFormat("  WHITE-list restriction(s) are activated: {0}", string.Join("; ", H3WhiteListConfig.WhiteList)).AppendLine();
            else
                cfg.AppendLine("  Warning: white-list isn't specified, so ip restrictions are absent");
            if (H3PasswordConfig.IsStricted)
                cfg.AppendLine("  Access to change a frequency IS RESTRICTED by a password");
            if (DebugTraceListener.LogFolder != null)
                cfg.AppendLine("  Logs are located in " + DebugTraceListener.LogFolder);

            Console.WriteLine(cfg);
            NiceTrace.Message(cfg.ToString());

            try
            {

                using (var server = Launch_H3Server(baseUrl))
                {
                    PsListener_OnLinux.Bind();
                    Console.WriteLine(Environment.NewLine + "HTTP server successefully has been started.");
                    Console.WriteLine("Press Ctlr-C to quit.");
                    ThreadPool.QueueUserWorkItem(state => Preload(baseUrl));

                    var exitEvent = new ManualResetEvent(false);
                    UnixExitSignal.Bind(s =>
                    {
                        Console.WriteLine("Shutdown command recieved.");
                        NiceTrace.Message("Shutdown command recieved.");
                        exitEvent.Set();
                    }, Signal.SIGUSR2, Signal.SIGINT);

                    exitEvent.WaitOne();
                }
                Console.WriteLine("HTTP server stopped");
                NiceTrace.Message("HTTP server stopped");
                return 0;
            }
            catch (Exception ex)
            {
                var m1 = "[ERROR] h3control terminated abnormally:" + Environment.NewLine + ex + Environment.NewLine + Environment.NewLine;
                NiceTrace.Message(m1);
                Console.WriteLine(m1);
                var m2 = "Short exception description: " + Environment.NewLine + ex.Get();
                NiceTrace.Message(m2);
                Console.WriteLine(m2);
                return 1;
            }
        }

        public static IDisposable Launch_H3Server(string baseUrl)
        {
                return WebApp.Start<H3ControlNancyStartup>(baseUrl);
        }

        private static void Preload(string baseUrl)
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
                baseUrl, 
            };

            bool hasErrors = false;
            CountdownEvent done = new CountdownEvent(list.Length);
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
                            // Console.WriteLine(s + ": OK, data is " + data.Length);
                        }
                        catch (Exception ex)
                        {
                            // Console.WriteLine(s + ": Fail" + ex);
                            hasErrors = true;
                        }
                        
                        NiceTrace.Message("Pre-jitted in {0:0.00} secs: {1}", sw1.ElapsedMilliseconds/1000m, url);
                        done.Signal();
                    }
                });
            }

            done.Wait();
            if (true || !hasErrors)
            {
                var msg = list.Length + " routes pre-jitted during " + sw.ElapsedMilliseconds / 1000m + " secs";
                NiceTrace.Message(msg);
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

        static void TraceEnvironment()
        {
            long workingSet64 = Process.GetCurrentProcess().WorkingSet64;
            StringBuilder b = new StringBuilder();
            Try(b, () => "  Platform .......... " + CrossInfo.ThePlatform);
            Try(b, () => "  Is Linux on Arm ... " + CrossInfo.IsLinuxOnArm);
            Try(b, () => "  Runtime ........... " + CrossInfo.RuntimeDisplayName);
            Try(b, () => "  OS ................ " + CrossInfo.OsDisplayName);
            Try(b, () => "  CPU ............... " + CrossInfo.ProcessorName);
            Try(b, () =>
            {
                var totalMem = CrossInfo.TotalMemory == null ? "n/a" : string.Format("{0:n0} Mb", CrossInfo.TotalMemory / 1024);
                return "  Memory ............ " + totalMem + "; Working Set: " + (workingSet64/1024L/1024).ToString("n0") + " Mb";
            });

            NiceTrace.Message("H3Control " + H3Environment.VerAsPublic + ". Environment:" + Environment.NewLine + b);
        }

        static void Try(StringBuilder b, Func<string> action)
        {
            try
            {
                b.Append(action() + Environment.NewLine);
            }
            catch (Exception)
            {
            }
        }

    }
}

