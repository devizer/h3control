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
        public void T01_ThePlatform_Doesnt_Fail()
        {
            Trace.WriteLine("Platform:" + CrossInfo.ThePlatform);
        }

        [Test]
        public void T02_ProcessorName_Doesnt_Fail()
        {
            Trace.WriteLine("CPU: " + CrossInfo.ProcessorName);
        }

        [Test]
        public void T03_Total_Memory_Doesnt_Fail()
        {
            Trace.WriteLine("Memory: " + CrossInfo.TotalMemory);
        }

        [Test]
        public void T03_PsListener_OnLinux_Returns_At_Least_One_Process()
        {
            List<PsProcessInfo> list = PsListener_OnLinux.Select(PsSortOrder.Cpu, 99999);
            Trace.WriteLine("Total processes: " + list.Count);
            foreach (var p in list)
            {
                Trace.WriteLine("     " + p);
            }

            Assert.IsTrue(list.Count > 0);

            Trace.WriteLine("");
        }

        [Test]
        public void T04_Hostname_Isnt_Empty()
        {
            var hostname = DeviceDataSource.HostName;
            Trace.WriteLine("Hostname: " + hostname);
            Assert.IsTrue(!string.IsNullOrEmpty(hostname));
        }


    }
}
