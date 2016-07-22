namespace H3Control
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;

    public class CpuUsageListener_OnLinux
    {
        // static private long? PrevTime;
        static private CpuUsageModel PrevModel;    // raw
        static private CpuUsageModel CurrentDelta; // delta
        static private long IntervalMssec = 500;
        static private readonly object Sync = new object();
        static private Thread _thread;

        public static void Bind(long intervalMsec = 500)
        {
            lock (Sync)
            {
                IntervalMssec = intervalMsec;
                if (Environment.OSVersion.Platform == PlatformID.Win32NT) return;

                if (_thread == null)
                {
                    PrevModel = GetLocalSnapshot();
                    Thread.Sleep(1);
                    CurrentDelta = GetNextDelta();
                    _thread = new Thread(() => GathererRunner()) {IsBackground = true};
                    _thread.Start();
                }
            }
        }

        public static CpuUsageModel CpuUsage
        {
            get
            {
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    return null;

                Bind();
                
                CpuUsageModel ret;
                lock (Sync)
                {
                    // we got delta!!!!
                    // now convert ticks into per cents
                    ret = CurrentDelta.Clone();
                }

                TransormToPercents(ret.Total);
                foreach (var cpu1UsageModel in ret.Cores)
                    TransormToPercents(cpu1UsageModel);

                return ret;

            }
        }

        static void GathererRunner()
        {
            while (true)
            {
                lock (Sync)
                {
                    CurrentDelta = GetNextDelta();
                }

                long copy;
                lock (Sync) copy = IntervalMssec;
                Thread.Sleep((int)Math.Max(100L, Math.Min(5000L, copy)));
            }
        }

        // RAW
        static CpuUsageModel GetNextDelta()
        {
            lock (Sync)
            {
                var next = GetLocalSnapshot();
                var prev = PrevModel;
                CpuUsageModel delta = new CpuUsageModel
                {
                    Total = GetDelta1(prev.Total, next.Total),
                    Cores = new List<Cpu1UsageModel>(),
                };

                var n = Math.Min(prev.Cores.Count, next.Cores.Count);
                for(int i=0; i<n; i++)
                    delta.Cores.Add(GetDelta1(prev.Cores[i], next.Cores[i]));

                PrevModel = next;
                // Console.WriteLine("DONE: GetNextDelta()");
                return delta;
            }
        }

        static Cpu1UsageModel GetDelta1(Cpu1UsageModel prev, Cpu1UsageModel next)
        {
            return new Cpu1UsageModel()
            {
                Idle = Math.Max((ulong) 0, next.Idle - prev.Idle),
                Nice = Math.Max((ulong) 0, next.Nice - prev.Nice),
                User = Math.Max((ulong) 0, next.User - prev.User),
                System = Math.Max((ulong) 0, next.System - prev.System),
            };
        }

        static void TransormToPercents(Cpu1UsageModel m)
        {
            decimal total = m.Idle + m.Nice + m.System + m.User;
            if (total > 0)
            {
                m.Idle /= total * 0.01m;
                m.Nice /= total * 0.01m;
                m.System /= total * 0.01m;
                m.User /= total * 0.01m;
            }
        }

        public static CpuUsageModel GetLocalSnapshot()
        {
            CpuUsageModel ret = new CpuUsageModel() { Cores = new List<Cpu1UsageModel>() };
            using (FileStream fs = new FileStream("/proc/stat", FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader rd = new StreamReader(fs, Encoding.UTF8))
            {
                while (true)
                {
                    string line = rd.ReadLine();
                    if (line == null)
                        break;

                    Cpu1UsageModel model;
                    int? corePosition;
                    if (ParseCpuLine(line, out model, out corePosition))
                    {
                        if (!corePosition.HasValue)
                            ret.Total = model;
                        else
                            ret.Cores.Add(model);
                    }
                }
            }

            if (ret.Total == null || ret.Cores.Count == 0)
                ret = null;

            // Console.WriteLine("DONE: GetLocalSnapshot()");
            return ret;
        }

        // corePosition starts from 0
        private static bool ParseCpuLine(string line, out Cpu1UsageModel model1, out int? corePosition)
        {
            model1 = null;
            corePosition = null;
            if (line == null || line.Length <= 4 || !line.Substring(0, 3).Equals("cpu", StringComparison.InvariantCultureIgnoreCase))
                return false;

            string strNumbers = line.Substring(3);
            ulong tmp;
            var numbers = strNumbers
                .Split(' ', '\t')
                .Where(x => ulong.TryParse(x, out tmp))
                .Select(x => ulong.Parse(x))
                .ToList();

            if (strNumbers[0] == ' ')
            {
                // Avg All cores
                corePosition = null;
            }
            else
            {
                if (numbers.Count < 1) return false;
                corePosition = (int)numbers[0];
                numbers.RemoveAt(0);
            }

            // cpu column removed. numbers starts with UserTime column
            if (numbers.Count < 4) return false;
            ulong
                longUserTime   = numbers[0],
                longNiceTime   = numbers[1],
                longSystemTime = numbers[2],
                longIdleTime   = numbers[3];

            model1 = new Cpu1UsageModel
            {
                Nice = longNiceTime,
                Idle = longIdleTime,
                User = longUserTime,
                System = longSystemTime,
            };

            return true;

        }

    }


}