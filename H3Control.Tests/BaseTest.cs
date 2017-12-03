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
        public virtual void TestFixtureSetUp()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix || CrossInfo.IsMono)
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
        public virtual void SetUp()
        {
            TestAt = Stopwatch.StartNew();
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TRAVIS")))
            {
                Trace.Listeners.Add(new ConsoleTraceListener());
                return;
            }

            try
            {
                CrossInfo.NextTest();
            }
            catch (Exception ex)
            {
                Trace.Listeners.Add(new ConsoleTraceListener());
            }

        }

        [TearDown]
        public virtual void TearDown()
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
        public virtual void TestFixtureTearDown()
        {
            AppDomain.CurrentDomain.DomainUnload += (sender, args) =>
            {
            };

            Trace.WriteLine("Total time: " + StartAt.Elapsed);
        }

    }
}