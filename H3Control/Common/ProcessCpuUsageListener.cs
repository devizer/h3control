namespace Universe
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    public static class ProcessCpuUsageListener
    {
        static readonly object Sync = new object();
        private static Thread Worker;
        private static long Interval = 1000;

        private static Stopwatch Watch;
        private static TimeSpan PrevTime;
        private static TimeSpan PrevUsage;
        private static float CpuUsage;

        public static void Bind(long interval = 1000)
        {
            Interval = Math.Min(5000, Math.Max(200, interval));
            lock(Sync)
                if (Worker == null)
                {
                    Watch = Stopwatch.StartNew();
                    PrevTime = Watch.Elapsed;
                    PrevUsage = GetProcessorTime();
                    Worker = new Thread(Work, 64*1024) { IsBackground = true};
                    Worker.Start();
                    Thread.Sleep(1);
                    NextIteration();
                }
        }

        public static float GetCpuUsage()
        {
            Bind();
            lock(Sync) return CpuUsage;
        }

        private static void Work()
        {
            while (true)
            {
                Thread.Sleep((int) Interval);

                lock (Sync)
                {
                    NextIteration();
                }
            }
        }

        private static void NextIteration()
        {
            var nextTime = Watch.Elapsed;
            var nextUsage = GetProcessorTime();

            var watchSeconds = (nextTime.TotalSeconds - PrevTime.TotalSeconds);
            var cpuSeconds = nextUsage.TotalSeconds - PrevUsage.TotalSeconds;
            CpuUsage = (float) (Math.Abs(cpuSeconds) > 0.0001d ? cpuSeconds/watchSeconds : 0);

            NiceTrace.Message("nextUsage: {0}, PrevUsage: {1}", nextUsage.TotalSeconds, PrevUsage.TotalSeconds);
            NiceTrace.Message("nextTime: {0}, PrevTime: {1}", nextTime.TotalSeconds, PrevTime.TotalSeconds);

            PrevTime = nextTime;
            PrevUsage = nextUsage;
        }

        static TimeSpan GetProcessorTime()
        {
            return Process.GetCurrentProcess().TotalProcessorTime;
        }
    }
}