using Microsoft.Owin.Hosting;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace H3Control
{
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    using Common;
    using Links;
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
            string pidFileFullPath = GetDefaultPidFileFullPath();
            var p = new OptionSet(StringComparer.InvariantCultureIgnoreCase)
            {
                {"b|Binding=", "Http binding, e.g. ip:port. Default is *:5000 (asterisk means all IPs)", v => binding = v},
                {"w|White-list=", "Comma separated IPs. Default or empty arg turns restrictions off", v => H3WhiteListConfig.WhiteListArg = v},
                {"g|Generate-pwd=", "Generate password hash (encrypt) and exit", v => generatePasswordHash = v},
                {"f|pid-File=", "pid-file path, default is either /var/run/h3control.pid or /tmp/h3control.pid", v => pidFileFullPath = v},
                {"p|Password=", "HASH of the desired password", v => H3PasswordConfig.Hash = v},
                {"v|Version", "Show version", v => isver = true},
                {"h|?|Help", "Display this help", v => help = v != null},
                {"n|nologo", "Hide logo", v => nologo = v != null}
            };

            p.Parse(args);

            if (!string.IsNullOrEmpty(generatePasswordHash))
            {
                Console.WriteLine(HashExtentions.SHA1(generatePasswordHash.Trim('\r', '\n')));
                return 0;
            }

            if (isver)
            {
                Console.WriteLine(H3Environment.VerAsPublic);
                return 0;
            }

            if (!nologo)
            {
                Console.WriteLine(@"H3Control " + H3Environment.VerAsPublic + @"is a console/daemon which
   * ""Shows"" temperature, frequency and usage via built-in http server.
   * Allows to control CPU & DDR frequency
   * Allows to control number of online CPU cores
");
            }

            if (help)
            {
                p.WriteOptionDescriptions(Console.Out);
                return 0;
            }

            bool isPidCreated = CreatePidFile(pidFileFullPath);

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

            StringBuilder cfgInfo = new StringBuilder("h3control's configuration by command line:").AppendLine();
            cfgInfo.AppendLine("   * Url is " + baseUrl);

            if (H3WhiteListConfig.HasWhiteList)
                cfgInfo.AppendFormat("   * WHITE-list restriction(s) are activated: {0}", string.Join("; ", H3WhiteListConfig.WhiteList)).AppendLine();
            else
                cfgInfo.AppendLine("   * WHITE-list isn't specified, so ip restrictions are absent");

            if (H3PasswordConfig.IsStricted)
                cfgInfo.AppendLine("   * Access to change a frequency IS RESTRICTED by a password");

            if (DebugTraceListener.LogFolder != null)
                cfgInfo.AppendLine("   * Logs are located in " + DebugTraceListener.LogFolder);

            if (isPidCreated)
                cfgInfo.AppendLine("   * Pid file location: " + pidFileFullPath);
            else
                cfgInfo.AppendLine("   * Warning! pid file " + pidFileFullPath + " was not created");

            DrunkActionExtentions.TryAndForget(() =>
            {
                var info = NetworkInterfaceExtentions.GetDescription();
                var needAll = binding.StartsWith("*");
                var hasAny = info.SelectMany(x => x.Value).Any();
                const string notFoundMessage = "   * Warning: None network adapters are known. http-server may be unavailable";
                if (needAll)
                {
                    if (!hasAny)
                        cfgInfo.AppendLine(notFoundMessage);
                    else
                    {
                        cfgInfo.AppendLine("   * h3control http-server is binding to all these network adapters:");
                        foreach (var k in info.Keys.OrderBy(x => x))
                        {
                            cfgInfo.AppendFormat("      - network '{0}': {1}", k, string.Join(", ", info[k]));
                            cfgInfo.AppendLine();
                        }
                    }
                }
                else
                {
                    if (!hasAny)
                        cfgInfo.AppendLine(notFoundMessage);
                }
            });


            Console.WriteLine(cfgInfo);
            NiceTrace.Message(cfgInfo);

            try
            {

                using (var server = Launch_H3Server(baseUrl))
                {
                    PsListener_OnLinux.Bind();
                    Console.WriteLine("HTTP server successefully has been started.");
                    Console.WriteLine("Press Ctlr-C to quit.");
                    ThreadPool.QueueUserWorkItem(state => H3PreCompiler.Compile(baseUrl));

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

        static void TraceEnvironment()
        {
            long workingSet64 = Process.GetCurrentProcess().WorkingSet64;
            StringBuilder archInfo = new StringBuilder();
            Try(archInfo, () => "  Platform .......... " + CrossInfo.ThePlatform + ", " + (BitConverter.IsLittleEndian ? "little-endian" : "big-endian"));
            Try(archInfo, () => "  Is Linux on Arm ... " + CrossInfo.IsLinuxOnArm);
            Try(archInfo, () => "  Runtime ........... " + CrossInfo.RuntimeDisplayName);
            Try(archInfo, () => "  OS ................ " + CrossInfo.OsDisplayName);
            Try(archInfo, () => "  CPU ............... " + CrossInfo.ProcessorName);
            Try(archInfo, () =>
            {
                var totalMem = CrossInfo.TotalMemory == null ? "n/a" : string.Format("{0:n0} Mb", CrossInfo.TotalMemory / 1024);
                return "  Memory ............ " + totalMem + "; Working Set: " + (workingSet64/1024L/1024).ToString("n0") + " Mb";
            });

            DateTime? built = AssemblyBuildDateTimeAttribute.CallerUtcBuildDate;
            NiceTrace.Message(string.Format(
                "H3Control {0} (built date is '{1}'). Environment:{2}{3}", 
                H3Environment.VerAsPublic, 
                built.HasValue ? built.Value.ToString("R") : "N/A",
                Environment.NewLine, 
                archInfo));
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

        static string GetDefaultPidFileFullPath()
        {
            string[] candidates = new[] {"/var/run/h3control.pid", "/tmp/h3control.pid"};
            try
            {
                if (CrossInfo.ThePlatform == CrossInfo.Platform.Windows)
                    candidates = new[]
                    {
                        Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "run\\h3control.pid")
                    };
            }
            catch 
            {
            }

            string pidFile = candidates.First();
            // TODO: Do we need check for access permission?
            return pidFile;
        }

        static bool CreatePidFile(string fullPath)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            }
            catch
            {
            }

            try
            {
                using (FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                using (StreamWriter wr = new StreamWriter(fs, Encoding.ASCII))
                {
                    wr.WriteLine(Process.GetCurrentProcess().Id);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}

