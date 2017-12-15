namespace H3Control.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;

    using Universe;

    class H3LauncherForTests
    {
        public int H3Pid;
        public Process H3;
        private static readonly long WaitForLaunch = 30000;

        public void LaunchAndWait(int port)
        {
            var testAssemblyPath = Assembly.GetExecutingAssembly().Location;
            NiceTrace.Message("Test assembly Path: {0}", testAssemblyPath);
            NiceTrace.Message("Current directory: {0}", Environment.CurrentDirectory);

            string exeDir = Path.GetDirectoryName(testAssemblyPath);
            // console runner
            string exe1 = Path.Combine(exeDir, "H3Control.exe");
            // resharper
            string exe2 = Path.Combine(Environment.CurrentDirectory, "H3Control.exe");
            string exe = exe1;
            foreach (var s in new[] { exe1, exe2}.Distinct())
            {
                var directoryName = Path.GetDirectoryName(s);
                IEnumerable<string> files = Directory
                    .GetFiles(directoryName)
                    .Select(x => Path.GetFileName(x))
                    .OrderBy(x => Path.GetFileNameWithoutExtension(x))
                    .Select(x => x + TryGetVer(x))
                    .ToList();

                var sep = Environment.NewLine + "  * ";
                NiceTrace.Message("Files in folder {0} assembly: {1}{2}", s, sep, string.Join(sep, files));
                if (File.Exists(s))
                    exe = s;
            }

            string args = "--binding=*:" + port + " --pid-file=h3control-test-" + port + ".pid";
            Trace.WriteLine("Launch: " + exe + " " + args);

            ProcessStartInfo si;
            if (CrossInfo.ThePlatform == CrossInfo.Platform.Windows)
                si = new ProcessStartInfo(exe, args);
            else
                si = new ProcessStartInfo("mono", exe + " " + args);

            si.UseShellExecute = false;
            si.CreateNoWindow = true;
            H3 = Process.Start(si);
            H3Pid = H3.Id;
            Trace.WriteLine("PID: " + H3Pid);

            int counter = 0;
            var swLaunch = Stopwatch.StartNew();
            var slowProcList = new[] {"ppc", "aarch64"};
            bool isSlowPC = slowProcList.Any(x => CrossInfo.ProcessorName.ToLower().IndexOf(x) >= 0);
            var timeoutScale = isSlowPC ? 15 : 1;
            if (isSlowPC) Trace.WriteLine("Slow PC: So, wait for server launch is " + WaitForLaunch / 1000 + " sec");
            bool isOk = PollWithTimeout.Run(WaitForLaunch * timeoutScale, () =>
            {
                var url = "http://localhost:" + port + "/";
                NiceTrace.Message("Try #{0} {1}", ++counter, url);
                return FastCheck(url);
            }, pollInterval: 333 * timeoutScale);

            NiceTrace.Message("Launch result (during {0}): {1}{2}", swLaunch.Elapsed, isOk ? "SUCCESS" : "FAIL", Environment.NewLine);
        }

        static string TryGetVer(string file)
        {
            try
            {
                FileVersionInfo fi = FileVersionInfo.GetVersionInfo(file);
                return "  " + fi.FileVersion;
            }
            catch (Exception)
            {
                return "";
            }
        }

        static bool FastCheck(string url)
        {
            try
            {
                var ver = new WebClient().DownloadString(url);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Close()
        {
            try
            {
                H3.Kill();
                H3 = null;
            }
            catch (Exception)
            {
            }
        }
    }
}