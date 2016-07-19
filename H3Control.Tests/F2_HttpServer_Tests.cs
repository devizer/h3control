namespace H3Control.Tests
{
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
        public void T03_Get_Default_cshtml_Doest_Fail_And_Return_200()
        {
            var url = BaseUrl + "/";
            ResponseDriller driller = ResponseDriller.CreateGet(url);
            driller.Dump();
            NiceTrace.Message("'{0}'", driller.StringStart);
            Assert.AreEqual(HttpStatusCode.OK, driller.Result.StatusCode);
        }

        [Test]
        public void T04_Select_Processes_Doest_Fail_And_Return_200()
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
                // NiceTrace.Message("{0} response: '{1}'", "/" + path, driller.String);
                Assert.AreEqual(HttpStatusCode.OK, driller.Result.StatusCode);
                if (++counter == urls.Length)
                    NiceTrace.Message(JSonFormatter.Format(driller.String));
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
        public void T06_GetDevice_Doest_Fail_And_Return_200()
        {
            // yes, there is no such column is Nianyty in processes response
            var url = BaseUrl + "/api/json/device/me";
            ResponseDriller driller = ResponseDriller.CreateGetJson(url);
            driller.Dump();
            Assert.AreEqual(HttpStatusCode.OK, driller.Result.StatusCode);
            NiceTrace.Message(JSonFormatter.Format(driller.String));
        }

        [Test]
        public void T07_404()
        {
            // yes, there is no such column is Nianyty in processes response
            string[] paths = new[]
            {
                "/Content/no-such-pAgE",
                "/Content/no-such-pAgE.css",
/*
                "/Content/no-such-pAgE.htm",
                "/Content/no-such-pAgE.js",
                "/no-such-pAgE",
                "/no-such-pAgE.css",
                "/no-such-pAgE.htm",
                "/no-such-pAgE.js",
*/
            };

            foreach (var method in new[] { HttpMethod.Get, HttpMethod.Put, HttpMethod.Post, })
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

    class JSonFormatter
    {
        public static string Format(string arg)
        {
/*
            dynamic parsedJson = JsonConvert.DeserializeObject(arg);
            return JsonConvert.SerializeObject(parsedJson, Formatting.Indented); 
*/
            
            JObject obj = JObject.Parse(arg);
            StringBuilder b = new StringBuilder();
            StringWriter wr = new StringWriter(b);
            JsonTextWriter jwr = new JsonTextWriter(wr);
            jwr.Formatting = Formatting.Indented;
            jwr.IndentChar = ' ';
            jwr.Indentation = 6;
            
            JsonSerializer ser = new JsonSerializer();
            ser.Formatting = Formatting.Indented;
            ser.Serialize(jwr, obj);
            jwr.Flush();
            string ret = b.ToString();
            return ret;
        }
    }
}