namespace H3Control.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;

    using Universe;


    public class PsParser_OnLinux_Ps
    {
        private static readonly string[] ColumnKeys = new[] { "pid", "pcpu", "rss", "size", "args" };

        public static List<PsProcessInfo> Select(PsSortOrder order = PsSortOrder.Cpu, int top = 5)
        {
            Stopwatch watch = Stopwatch.StartNew();
            ProcessStartInfo si = new ProcessStartInfo("ps", "ax -o pid,pcpu,rss,size,args");
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
                    PsProcessInfo.Sort(all, order)
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
                        using (StreamReader rdr = new StreamReader(fs, UTF8))
                        {
                            string line;
                            while ((line = rdr.ReadLine()) != null)
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

        internal static IEnumerable<PsProcessInfo> Sort(IEnumerable<PsProcessInfo> ret, PsSortOrder order)
        {
            if (order == PsSortOrder.Cpu) return ret.OrderByDescending(x => x.CpuUsage);
            if (order == PsSortOrder.Rss) return ret.OrderByDescending(x => x.Rss);
            if (order == PsSortOrder.Size) return ret.OrderByDescending(x => x.Size);
            if (order == PsSortOrder.Swapped) return ret.OrderByDescending(x => x.Swapped);
            return ret;
        }

        internal static IEnumerable<PsProcessInfo> Clone(IEnumerable<PsProcessInfo> ret)
        {
            foreach (var i in ret)
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
                    _t = new Thread(Start) { IsBackground = true };
                    _Processes = PsParser_OnLinux_Ps.Select(order: PsSortOrder.None, top: 99999);
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

            var some = order == PsSortOrder.Cpu
                ? copyOfAll.Where(x => x.Args == null || x.Args.IndexOf("h3control", StringComparison.InvariantCultureIgnoreCase) < 0)
                : copyOfAll;

            var ordered = PsProcessInfo.Sort(some, order).Where((info, i) => i < top);
            var ret = PsProcessInfo.Clone(ordered);
            return ret.ToList();
        }

        private static void Start(object o)
        {
            while (true)
            {
                Thread.Sleep(3333);
                var all = PsParser_OnLinux_Ps.Select(order: PsSortOrder.None, top: 99999);
                lock (SyncProcesses)
                {
                    _Processes = all;
                }
            }
        }


    }


}