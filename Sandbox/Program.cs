using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    using System.Diagnostics;
    using System.Threading;

    class Program
    {
        static void Main(string[] args)
        {
            TestProcessCpuUsage();
        }

        static void TestProcessCpuUsage()
        {

            Dump();
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 2000)
            {
                // Perform1();
                Thread.Sleep(10);
            }
            Dump();

        }

        static void Dump()
        {
            Process p = Process.GetCurrentProcess();
            Console.WriteLine("Privileged: {0}, User: {1}, Total: {2}",
                p.PrivilegedProcessorTime,
                p.UserProcessorTime,
                p.TotalProcessorTime
                );
        }
    }
}
