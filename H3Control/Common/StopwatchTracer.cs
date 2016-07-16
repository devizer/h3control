using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Universe
{
    public class StopwatchTracer : IDisposable
    {
        protected Stopwatch _sw;
        private string _msg;
        protected StopwatchTracer()
        {
            _sw = new Stopwatch();
            _sw.Start();
        }

        public static StopwatchTracer Run(string message, params object[] args)
        {
            string msg = string.Format(message, args);
            StopwatchTracer ret = new StopwatchTracer();
            ret._msg = msg;
            return ret;
        }

        public static StopwatchTracer Run(string msg)
        {
            StopwatchTracer ret = new StopwatchTracer();
            ret._msg = msg;
            return ret;
        }

        public long ElapsedMilliseconds
        {
            get { return _sw.ElapsedMilliseconds; }
        }
        
        public void Dispose()
        {
            Trace();
        }

        public void Trace()
        {
            if (_sw == null) return;

            long ms =
                _sw != null
                    ? _sw.ElapsedMilliseconds
                    : 0;

            ms = Math.Max(1, ms);
            string ms2 = new DateTime(0).AddMilliseconds(ms).ToString("HH:mm:ss.fff");
            string message = _msg + ": " + ms2;
            _sw = null;
            System.Diagnostics.Trace.WriteLine(message);
            // Console.WriteLine(":> " + message);
        }
    }
}
