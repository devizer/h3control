namespace H3Control.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Threading;

    using NUnit.Framework;

    using Universe;

    [TestFixture]
    public class H3_HttpServer_Tests
    {
        private IDisposable H3Server;
        private int Port;

        string BaseUrl
        {
            get { return "http://localhost:" + Port; }
        }
        
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            WhiteListConfig.WhiteListArg = null;
            PasswordConfig.Hash = null;
            Port = TestEnvironment.GetTcpPort();
            H3Server = H3Main.Launch_H3Server("http://*:" + Port);
        }

        [Test]
        public void T01_Get_Favicon_Returns()
        {
            var url = BaseUrl + "/favicon.ico";
            using (WebClient client = new WebClient())
            {
                var data = client.DownloadData(url);
                Trace.WriteLine(url + ": length is " + data.Length);
            }
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            if (H3Server != null)
            {
                var copy = H3Server;
                ThreadPool.QueueUserWorkItem(state => copy.Dispose());
                H3Server = null;
            }
        }

    }
}