namespace H3Control.Tests
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    class PollWithTimeout
    {
        public static bool Run(long waitDuration, Func<bool> func, int pollInterval = 50)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (true)
            {
                var isOk = func();
                if (isOk) return true;
                if (sw.ElapsedMilliseconds >= waitDuration)
                    return false;

                Thread.Sleep(pollInterval);
            }
        }
    }
}