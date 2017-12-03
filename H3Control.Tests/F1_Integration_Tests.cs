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
    public class F1_Integration_Tests : BaseTest
    {
        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            if (CrossInfo.ThePlatform == CrossInfo.Platform.Linux)
            {
                string all = File.ReadAllText("/proc/cpuinfo");
                int lineNumbers = all.Split('\r', '\n').Count(x => x.Trim().Length > 0);
                Trace.WriteLine("/proc/cpuinfo:::::::::::::" + lineNumbers + " lines");
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
        public void T04A_Show_PS_Output_Without_Args()
        {
            Show_PS_Output("");
        }

        [Test]
        public void T04B_Show_PS_Output_With_Options()
        {
            Show_PS_Output(PsParser_OnLinux_or_FreeBSD.PS_ARGS);
        }

        private static void Show_PS_Output(string commandLineArgs)
        {
            string output;
            Exception exOutput;
            string error;
            Exception exError;
            int code;
            Trace.WriteLine(string.Format("PS Args: {0}", commandLineArgs == "" ? "[NONE]" : commandLineArgs));
            CrossInfo.HiddenExec("ps", commandLineArgs, out output, out exOutput, out error, out exError, out code);
            
            if (!string.IsNullOrEmpty(output)) 
                Trace.WriteLine(string.Format("Output: {0}{1}", Environment.NewLine, output));

            if (!string.IsNullOrEmpty(error)) 
                Trace.WriteLine(string.Format(
                    @"Error: {0}{1}{0}It seems installed ps utility isn't supported
FYI: ps from Git distribution doesn't work properly on Windows, but ps from MSYS works fine",
                    Environment.NewLine, error));
        }

        [Test]
        public void T05_PsListener_OnLinux_Returns_At_Least_One_Process()
        {
            List<PsProcessInfo> list = PsListener_OnLinux.Select(PsSortOrder.Rss, 99999);
            Trace.WriteLine("Total processes: " + list.Count);
            foreach (var p in list)
            {
                Trace.WriteLine("     " + p);
            }

            Assert.IsTrue(list.Count > 0);
            Trace.WriteLine("");
        }

        [Test]
        public void T06_Hostname_Isnt_Empty()
        {
            var hostname = DeviceDataSource.HostName;
            Trace.WriteLine("Hostname: " + hostname);
            Assert.IsTrue(!string.IsNullOrEmpty(hostname));
        }


    }
}
