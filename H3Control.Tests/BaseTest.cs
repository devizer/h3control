namespace Universe
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text;

    using NUnit.Framework;

    public class BaseTest
    {
        private static Stopwatch StartAt = Stopwatch.StartNew();
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) =>
                {
                    return true;
                };
            }
            CrossInfo.AttachUnitTrace("H3Control unit tests");
            StartAt = Stopwatch.StartNew();
        }

        Stopwatch TestAt = Stopwatch.StartNew();
        [SetUp]
        public void SetUp()
        {
            TestAt = Stopwatch.StartNew();
            CrossInfo.NextTest();
        }

        [TearDown]
        public void TearDown()
        {
            // Trace.WriteLine(Environment.StackTrace + Environment.NewLine);
            var ws1 = (Process.GetCurrentProcess().WorkingSet64/1024L/1024).ToString("n0");
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var ws2 = (Process.GetCurrentProcess().WorkingSet64/1024L/1024).ToString("n0");
            Trace.WriteLine("***** " + TestAt.Elapsed + ", "
                            + "Working Set: " + ws1 + " Mb"
                            + (ws1 != ws2 ? ", after GC: " + ws2 + " Mb" : "")
                            + Environment.NewLine);
        }


        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            AppDomain.CurrentDomain.DomainUnload += (sender, args) =>
            {
            };

            Trace.WriteLine("Total time: " + StartAt.Elapsed);
        }

    }
}