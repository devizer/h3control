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

    class H3Launcher
    {
        public int H3Pid;
        public Process H3;

        public void LaunchAndWait(int port)
        {
            var testAssemblyPath = Assembly.GetExecutingAssembly().Location;
            NiceTrace.Message("Test assembly Path: {0}", testAssemblyPath);
            string exeDir = Path.GetDirectoryName(testAssemblyPath);
            string exe = Path.Combine(exeDir, "H3Control.exe");
            string args = "--binding=*:" + port;
            Trace.WriteLine("Launch: " + exe + " " + args);


            ProcessStartInfo si;
            if (CrossInfo.ThePlatform == CrossInfo.Platform.Windows)
                si = new ProcessStartInfo(exe, args);
            else
                si = new ProcessStartInfo("mono", exe + " " + args);

            si.UseShellExecute = false;
            si.CreateNoWindow = true;
            IEnumerable<string> files = Directory.GetFiles(exeDir).Select(x => Path.GetFileName(x)).ToList();
            NiceTrace.Message("Files near test assembly: {0}", string.Join(", ", files));
            H3 = Process.Start(si);
            H3Pid = H3.Id;
            Trace.WriteLine("PID: " + H3Pid);

            int counter = 0;
            PollWithTimeout.Run(10000, () =>
            {
                var url = "http://localhost:" + port + "/favicon.ico";
                NiceTrace.Message("Try #{0} {1}", ++counter, url);
                return FastCheck(url);
            }, pollInterval: 111);
        }

        static bool FastCheck(string url)
        {
            try
            {
                new WebClient().DownloadData(url);
                return true;
            }
            catch (Exception ex)
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