namespace Universe
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    using H3Control.Common;

    public static class ThisProcessCpuUsageListener
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
                    FirstIteration();
                    Worker = new Thread(Work, 64*1024) { IsBackground = true};
                    Worker.Start();
                    Thread.Sleep(1);
                    NextIteration();
                }
        }

        private static void FirstIteration()
        {
            try
            {
                Watch = Stopwatch.StartNew();
                PrevTime = Watch.Elapsed;
                PrevUsage = GetProcessorTime();
            }
            catch (Exception ex)
            {
                NiceTrace.Message("ThisProcessCpuUsageListener.FirstIteration() failed {0}{1}", Environment.NewLine, ex);
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
            try
            {
                var nextTime = Watch.Elapsed;
                var nextUsage = GetProcessorTime();

                var watchSeconds = (nextTime.TotalSeconds - PrevTime.TotalSeconds);
                var cpuSeconds = nextUsage.TotalSeconds - PrevUsage.TotalSeconds;
                CpuUsage = (float) (Math.Abs(cpuSeconds) > 0.0001d ? cpuSeconds/watchSeconds : 0);

                FirstRound.Only("ThisProcessCpuUsageListener.NextIteration dump", RoundCounter.Twice, () =>
                {
                    NiceTrace.Message("ThisProcessCpuUsageListener, nextUsage: {0}, PrevUsage: {1}", nextUsage.TotalSeconds, PrevUsage.TotalSeconds);
                    NiceTrace.Message("ThisProcessCpuUsageListener, nextTime: {0}, PrevTime: {1}", nextTime.TotalSeconds, PrevTime.TotalSeconds);
                });

                PrevTime = nextTime;
                PrevUsage = nextUsage;
            }
            catch (Exception ex)
            {
                NiceTrace.Message("ThisProcessCpuUsageListener.NextIteration() failed {0}{1}", Environment.NewLine, ex);
            }
        }

        static TimeSpan GetProcessorTime()
        {
            return Process.GetCurrentProcess().TotalProcessorTime;
        }
    }
}