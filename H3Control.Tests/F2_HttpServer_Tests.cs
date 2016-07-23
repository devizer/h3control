namespace H3Control.Tests
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    using NUnit.Framework;

    using Universe;

    [TestFixture]
    public class F2_HttpServer_Tests : BaseTest
    {
        private H3LauncherForTests Launcher;
        private int Port;

        string BaseUrl
        {
            get { return "http://localhost:" + Port; }
        }
        
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            CrossInfo.AttachUnitTrace("H3Control unit tests");
            Launcher = new H3LauncherForTests();
            Port = TestEnvironment.GetTcpPort();
            Launcher.LaunchAndWait(Port);
        }

        [Test]
        public void T01_Get_Favicon_Doesnt_Fail_And_HasBytes()
        {
            var url = BaseUrl + "/favicon.ico";
            ResponseDriller driller = ResponseDriller.CreateGet(url);
            driller.Dump();
            var len = driller.Bytes.Length;
            NiceTrace.Message("{0}: length is {1} bytes", url, len);
            Assert.AreEqual(HttpStatusCode.OK, driller.Result.StatusCode);
        }

        [Test]
        public void T02_Get_Ver_Via_Http_Doesnt_Fail_And_Ver_EndEquals_Assembly_Version()
        {
            var url = BaseUrl + "/Ver";
            ResponseDriller driller = ResponseDriller.CreateGet(url);
            driller.Dump();
            NiceTrace.Message("/Ver response: '{0}'", driller.String);
            var expectedVersion = H3Environment.VerAsPublic;
            Assert.AreEqual(HttpStatusCode.OK, driller.Result.StatusCode);
            Assert.AreEqual(expectedVersion, driller.String);
        }

        [Test]
        public void T03_Get_Default_cshtml_Doesnt_Fail_And_Returns_200()
        {
            var url = BaseUrl + "/";
            ResponseDriller driller = ResponseDriller.CreateGet(url);
            driller.Dump();
            NiceTrace.Message("'{0}'", driller.StringStart);
            Assert.AreEqual(HttpStatusCode.OK, driller.Result.StatusCode);
        }

        [Test]
        public void T04_Select_Processes_Doesnt_Fail_And_Returns_200()
        {
            // Cpu, Rss, Swapped, Size
            string[] urls = new[]
            {
                "api/json/processes/by-Rss/top-3",
                "api/json/processes/by-Cpu/top-3",
                "api/json/processes/by-Swapped/top-3",
                "api/json/processes/by-Size/top-3",
            };

            int counter = 0;
            foreach (var path in urls)
            {
                var url = BaseUrl + "/" + path;
                ResponseDriller driller = ResponseDriller.CreateGetJson(url);
                driller.Dump();
                Assert.AreEqual(HttpStatusCode.OK, driller.Result.StatusCode);
                JSonExtentions.CheckFormat(driller.String, path);
                if (true || ++counter == urls.Length)
                    NiceTrace.Message(JSonExtentions.Format(driller.String));
            }
        }

        [Test]
        public void T05_Select_Processes_by_Nianyty_Fails_and_Return_400()
        {
            // yes, there is no such column is Nianyty in processes response
            var url = BaseUrl + "/api/json/processes/by-Nianyty/top-3";
            ResponseDriller driller = ResponseDriller.CreateGetJson(url);
            driller.Dump();
            Assert.AreEqual(HttpStatusCode.BadRequest, driller.Result.StatusCode);
        }

        [Test]
        public void T06_Get_Device_Doesnt_Fail_And_Returns_200()
        {
            // yes, there is no such column is Nianyty in processes response
            var url = BaseUrl + "/api/json/device/me";
            ResponseDriller driller = ResponseDriller.CreateGetJson(url);
            driller.Dump();
            Assert.AreEqual(HttpStatusCode.OK, driller.Result.StatusCode);
            JSonExtentions.CheckFormat(driller.String, "/api/json/device/me");
            NiceTrace.Message(JSonExtentions.Format(driller.String));
        }

        [Test]
        public void T07_Get_WhatsNew_Doesnt_Fail_And_Returns_200()
        {
            var urls = new[]
            {
                BaseUrl + "/whatsnew/html",
                BaseUrl + "/whatsnew/markdown",
                BaseUrl + "/whatsnew/html-include",
            };
            foreach (var url in urls)
            {
                ResponseDriller driller = ResponseDriller.CreateGet(url);
                driller.Dump();
                Assert.AreEqual(HttpStatusCode.OK, driller.Result.StatusCode, "URL is {0}", url);
                Assert.IsTrue(
                    driller.String.IndexOf("Update", StringComparison.InvariantCultureIgnoreCase) >= 0,
                    "Response of {0} SHOULD include word 'UPDATE'. But Response is{1}{2}", 
                    url,
                    Environment.NewLine,
                    driller.String);
            }
        }

        [Test]
        public void T08_404()
        {
            // yes, there is no such column is Nianyty in processes response
            string[] paths = new[]
            {
                "/Content/no-such-pAgE",
                "/Content/no-such-pAgE.css",
            };

            foreach (var method in new[] { HttpMethod.Get, HttpMethod.Put, })
            foreach (var path in paths)
            {
                var url = BaseUrl + path;
                var req = new HttpRequestMessage(method, url);
                ResponseDriller driller = ResponseDriller.Create(req);
                driller.Dump();
                Assert.AreEqual(HttpStatusCode.NotFound, driller.Result.StatusCode);
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