﻿namespace H3Control.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using Universe;


    public class PsParser_OnLinux_or_FreeBSD
    {
        public static readonly string PS_ARGS = "ax -o pid,pcpu,rss,vsz,args";
        private static readonly string[] ColumnKeys = new[] { "pid", "pcpu", "rss", "vsz", "args" };

        public static List<PsProcessInfo> Select(PsSortOrder order = PsSortOrder.Cpu, int top = 5)
        {
            Stopwatch watch = Stopwatch.StartNew();
            // on FreeBSD `size` should be changed to vsz or vsize
            // https://www.freebsd.org/cgi/man.cgi?ps(1)
            ProcessStartInfo si = new ProcessStartInfo("ps", PS_ARGS);
            si.UseShellExecute = false;
            si.CreateNoWindow = true;
            si.RedirectStandardOutput = true;
            si.StandardOutputEncoding = Encoding.UTF8;
            using (Process p = Process.Start(si))
            {
                Thread.Sleep(10);
                string line;
                var lines = p.StandardOutput;
                List<PsProcessInfo> all = new List<PsProcessInfo>();
                while ((line = lines.ReadLine()) != null)
                {
                    PsProcessInfo pi;
                    if (TryParse_ps_Line(line, out pi))
                        all.Add(pi);
                }

                var elapsed = watch.Elapsed;
                FirstRound.Only("Exec ps command", RoundCounter.Twice, () =>
                {
                    NiceTrace.Message("Fetched {0} processes info using {1} secs", all.Count, elapsed);
                });

                watch = Stopwatch.StartNew();
                FillSwapped(all);
                FirstRound.Only("Fetch swapped memory by all processes", RoundCounter.Twice, () =>
                {
                    NiceTrace.Message("Fetch swapped memory by all processes by {0} secs", watch.Elapsed);
                });


                return 
                    PsProcessInfoExtentions.SortBy(all, order)
                    .Where((info, i) => i < top).ToList();
            }
        }

        static readonly UTF8Encoding UTF8 = new UTF8Encoding(false);
        static void FillSwapped(List<PsProcessInfo> all)
        {
            foreach (var proc in all)
            {
                proc.Swapped = -1;
                try
                {
                    string fileName = "/proc/" + proc.Pid + "/status";
                    if (File.Exists(fileName))
                    {
                        using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
                        using (StreamReader kernelReader = new StreamReader(fs, UTF8))
                        {
                            var fileContent = kernelReader.ReadToEnd();
                            var contentReader = new StringReader(fileContent);
                            string line;
                            while ((line = contentReader.ReadLine()) != null)
                            {
                                const string pattern = "VmSwap:";
                                if (line.StartsWith(pattern, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    var sub = line.Substring(pattern.Length).ToLower().Replace("kb", "").Trim();
                                    int swapped;
                                    if (int.TryParse(sub, out swapped))
                                    {
                                        proc.Swapped = swapped;
                                        break;
                                    }

                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    NiceTrace.Message("Unable to fetch swapped memory size of process {0}:{1} {2}{3}",
                        proc.Pid, proc.Args, Environment.NewLine, ex);
                }
            }
        }

        private static bool TryParse_ps_Line(string line, out PsProcessInfo pi)
        {
            pi = null;
            if (string.IsNullOrEmpty(line))
                return false;

            var arr = line.TrimStart().Split(' ').Where(x => x.Length > 0).ToArray();
            if (arr.Length < 5) return false;
            int pid;
            decimal cpu;
            decimal rss;
            decimal size;
            if (int.TryParse(arr[0], out pid))
                if (decimal.TryParse(arr[1], out cpu))
                    if (decimal.TryParse(arr[2], out rss))
                        if (decimal.TryParse(arr[3], out size))
                        {
                            StringBuilder b = new StringBuilder();
                            for (int i = 4; i < arr.Length; i++) b.Append(b.Length == 0 ? "" : " ").Append(arr[i]);
                            pi = new PsProcessInfo()
                            {
                                Pid = pid,
                                CpuUsage = cpu,
                                Rss = (long)rss,
                                Size = (long)size,
                                Args = b.ToString(),
                            };

                            return true;
                        }

            return false;
        }
    }

    public enum PsSortOrder
    {
        None = 0,
        Cpu = 1,
        Rss = 2,
        Swapped = 3,
        Size = 4,
    }

    public class PsProcessInfo
    {
        public int Pid { get; set; }
        public decimal CpuUsage { get; set; }
        public long Rss { get; set; }
        public long Size { get; set; }
        public long Swapped { get; set; }
        public string Args { get; set; }

        public PsProcessInfo()
        {
            Swapped = -1;
        }

        public PsProcessInfo Clone()
        {
            return (PsProcessInfo) this.MemberwiseClone();
        }

        public override string ToString()
        {
            return string.Format("Pid: {0,6}, CpuUsage: {1,5}, Rss: {2,8}, Swapped: {3,8}, Size: {4,8}, Args: {5}", Pid, CpuUsage, Rss, Swapped, Size, Args);
        }
    }

    public static class PsProcessInfoExtentions
    {
        internal static IEnumerable<PsProcessInfo> SortBy(this IEnumerable<PsProcessInfo> arg, PsSortOrder order)
        {
            if (order == PsSortOrder.Cpu) return arg.OrderByDescending(x => x.CpuUsage);
            if (order == PsSortOrder.Rss) return arg.OrderByDescending(x => x.Rss);
            if (order == PsSortOrder.Size) return arg.OrderByDescending(x => x.Size);
            if (order == PsSortOrder.Swapped) return arg.OrderByDescending(x => x.Swapped).ThenByDescending(x => x.Size);
            return arg;
        }

        public static IEnumerable<PsProcessInfo> Clone(this IEnumerable<PsProcessInfo> arg)
        {
            foreach (var i in arg)
                yield return i.Clone();
        }
    }

    public class PsListener_OnLinux
    {
        private static Thread _t;
        static readonly object SyncInit = new object();
        static readonly object SyncProcesses = new object();
        private static List<PsProcessInfo> _Processes = null;


        public static void Bind(int milliSeconds = 2222)
        {
            lock (SyncInit)
            {
                if (_t == null)
                {
                    _t = new Thread(Start, 64*1024) { IsBackground = true };
                    _Processes = PsParser_OnLinux_or_FreeBSD.Select(order: PsSortOrder.None, top: 99999);
                    _t.Start();
                }
            }
        }

        public static List<PsProcessInfo> Select(PsSortOrder order = PsSortOrder.Cpu, int top = 5)
        {
            Bind(milliSeconds: 2222);
            List<PsProcessInfo> copyOfAll;
            lock (SyncProcesses)
                copyOfAll = _Processes;

            var myPid = _CurrentProcessId.Value;
            var myInfo = copyOfAll.FirstOrDefault(x => x.Pid == myPid);
            if (myInfo != null)
            {
                var cpuUsage = ThisProcessCpuUsageListener.GetCpuUsage();
                if (cpuUsage.HasValue)
                    myInfo.CpuUsage = 100m*(decimal) cpuUsage.Value;
            }

            return copyOfAll
                .SortBy(order)
                .Where((info, i) => i < top)
                .Clone()
                .ToList();
        }

        static readonly Lazy<int> _CurrentProcessId = new Lazy<int>(() =>
        {
            return Process.GetCurrentProcess().Id;
        });

        private static void Start(object o)
        {
            while (true)
            {
                Thread.Sleep(3333);
                Stopwatch sw = Stopwatch.StartNew();
                var all = PsParser_OnLinux_or_FreeBSD.Select(order: PsSortOrder.None, top: 99999);
                FirstRound.Only("PsParser_OnLinux_or_FreeBSD.Select()", 20, () =>
                {
                    NiceTrace.Message("PsParser_OnLinux_or_FreeBSD.Select() takes {0:n0} msec", sw.ElapsedMilliseconds);
                });
                
                lock (SyncProcesses)
                {
                    _Processes = all;
                }
            }
        }


    }


}