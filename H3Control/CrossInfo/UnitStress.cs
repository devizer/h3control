using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Universe
{
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;

    class UnitStress
    {
        public static void Run(int numThreads, int duration, Action iteration, Action threadInitializer = null, Action threadFinalizer = null)
        {
            // Heat 
            try
            {
                iteration();
            }
            catch (Exception ex)
            {
            }


            Dictionary<int, object> totalThreads = new Dictionary<int, object>();
            long totalActions = 0;
            Barrier b = new Barrier(numThreads);
            List<Thread> threads = new List<Thread>();
            FailCollection fails = new FailCollection();
            int[] numbers = new int[numThreads];
            int index = 0;
            foreach (var action in Enumerable.Range(1, numThreads))
            {
                var a = action;
                int current = index;
                Thread t = new Thread(() =>
                {
                    lock (totalThreads) totalThreads[Thread.CurrentThread.ManagedThreadId] = null;

                    if (threadInitializer != null)
                        threadInitializer();

                    int n1 = 0;
                    b.SignalAndWait();
                    Stopwatch sw = Stopwatch.StartNew();
                    do
                    {
                        try
                        {
                            iteration();
                            Interlocked.Increment(ref totalActions);
                            n1++;
                        }
                        catch (Exception ex)
                        {
                            FailInfo fi = new FailInfo(ex, current);
                            fails.SmartAdd(fi);
                            DumpException("Stress Action failed: " + ex);
                        }

                    }
                    while (sw.ElapsedMilliseconds < duration);

                    if (threadFinalizer != null)
                        threadFinalizer();

                    lock (numbers) numbers[current] = n1;
                    b.SignalAndWait();

                }) { IsBackground = true };

                t.Start();
                threads.Add(t);
                index++;
            }

            foreach (var thread in threads)
                thread.Join();

            string totalActionsAs =
                totalActions + " = " + string.Join("+", numbers.Select(x => x.ToString()));

            if (numThreads == 1)
                totalActionsAs = totalActions.ToString();

            Trace.WriteLine(string.Format(
                "  Workers: {0}. Total actions: {1}, threads: {2}",
                numThreads,
                totalActionsAs,
                totalThreads.Count));

            if (fails.Count > 0)
            {
                var asStr = fails
                    .OrderByDescending(x => x.Count)
                    .Select(x => string.Format("   :) #{0}, {1} times {2}", x.Worker, x.Count, x.Reason));

                Trace.WriteLine(string.Join(Environment.NewLine, asStr));
            }


        }

        static readonly StringBuilder Exceptions = new StringBuilder();
        private static void DumpException(string s)
        {

            lock (Exceptions)
            {
                if (Exceptions.Length == 0)
                    AppDomain.CurrentDomain.DomainUnload += (sender, args) =>
                    {
                        File.WriteAllText(
                            string.Format("Exceptions-{0}.txt", DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")),
                            Exceptions.ToString());
                    };

                Exceptions.AppendLine().AppendLine(s);
            }


        }

        class FailInfo
        {
            public string AsString;
            public int Worker;
            public string Reason;
            public int Count;

            public FailInfo(Exception ex, int worker)
            {
                Worker = worker;
                Count = 1;
                AsString = ex.ToString();

                List<string> msgs = new List<string>();
                while (ex != null)
                {
                    msgs.Add(ex.Message);
                    ex = ex.InnerException;
                }

                Reason = String.Join(" -->-- ", msgs);
            }
        }

        class FailCollection : Collection<FailInfo>
        {
            public void SmartAdd(FailInfo fi)
            {
                lock (base.Items)
                {
                    var found = this.FirstOrDefault(x => x.Worker == fi.Worker && x.AsString == fi.AsString);
                    if (found == null)
                        this.Add(fi);

                    else
                        found.Count++;
                }
            }
        }


    }
}
