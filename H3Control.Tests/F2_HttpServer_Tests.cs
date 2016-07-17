namespace H3Control.Tests
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Threading;

    using NUnit.Framework;

    using Universe;

    [TestFixture]
    public class F2_HttpServer_Tests
    {
        private H3LauncherAsTest Launcher;
        private int Port;

        string BaseUrl
        {
            get { return "http://localhost:" + Port; }
        }
        
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            CrossInfo.AttachUnitTrace("H3Control unit tests");
            Launcher = new H3LauncherAsTest();
            Port = TestEnvironment.GetTcpPort();
            Launcher.LaunchAndWait(Port);
        }

        [Test]
        public void T01_Get_Favicon_Doesnt_Fail()
        {
            var url = BaseUrl + "/favicon.ico";
            using (WebClient client = new WebClient())
            {
                var data = client.DownloadData(url);
                Trace.WriteLine(url + ": length is " + data.Length);
            }
        }

        [Test]
        public void T01_Get_Ver_Via_Http_Doesnt_Fail()
        {
            var url = BaseUrl + "/Ver";
            using (WebClient client = new WebClient())
            {
                var ver = client.DownloadString(url);
                Trace.WriteLine("Version (via http) is " + ver);
            }
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            if (Launcher != null)
            {
                var copy = Launcher;
                ThreadPool.QueueUserWorkItem(state => copy.Close());
                Launcher = null;
            }
        }

    }
}