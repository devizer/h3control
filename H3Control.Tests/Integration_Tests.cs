using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Control.Tests
{
    using System.Diagnostics;
    using System.IO;

    using Common;

    using NUnit.Framework;

    using Universe;

    [TestFixture]
    public class Integration_Tests
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            if (CrossInfo.ThePlatform == CrossInfo.Platform.Linux)
            {
                string all = File.ReadAllText("/proc/cpuinfo");
                Console.WriteLine("/proc/cpuinfo:::::::::::::" + Environment.NewLine + all);
            }
            CrossInfo.AttachUnitTrace("H3Control unit tests");
        }
        
        [Test]
        public void T01_Platform()
        {
            Trace.WriteLine("Platform:" + CrossInfo.ThePlatform);
        }

        [Test]
        public void T02_CPU()
        {
            Trace.WriteLine("CPU: " + CrossInfo.ProcessorName);
        }

        [Test]
        public void T03_Memory()
        {
            Trace.WriteLine("Memory: " + CrossInfo.TotalMemory);
        }

        [Test]
        public void T03_Processes()
        {
            List<PsProcessInfo> list = PsListener_OnLinux.Select(PsSortOrder.Cpu, 99999);
            Trace.WriteLine("Total processes: " + list.Count);
            foreach (var p in list)
            {
                Trace.WriteLine("     " + p);
            }

            Trace.WriteLine("");
        }

    }
}
