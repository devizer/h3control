namespace H3Control.Tests
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

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
        public void T01_Get_Favicon_Doesnt_Fail_And_HasBytes()
        {
            var url = BaseUrl + "/favicon.ico";
            ResponseDriller favicon = ResponseDriller.CreateGet(url);
            favicon.Dump();
            var len = favicon.Bytes.Length;
            Trace.WriteLine(url + ": length is " + len);
            NiceTrace.Message("{0}: length is {1} bytes", url, len);
        }

        [Test]
        public void T02_Get_Ver_Via_Http_Doesnt_Fail_EndEquals_Assembly_Version()
        {
            var url = BaseUrl + "/Ver";
            ResponseDriller vers = ResponseDriller.CreateGet(url);
            vers.Dump();
            NiceTrace.Message("/Ver response: '{0}'", vers.String);
            var expected = H3Environment.Ver.ToString(3);
            Assert.AreEqual(expected, vers.String);
        }

        [Test]
        public void T03_Select_Processes()
        {
            // Cpu, Rss, Swapped, Size
            string[] urls = new[]
            {
                "api/json/processes/by-Rss/top-3",
                "api/json/processes/by-Cpu/top-3",
                "api/json/processes/by-Swapped/top-3",
                "api/json/processes/by-Size/top-3",
            };
            
            foreach (var path in urls)
            {
                var url = BaseUrl + "/" + path;
                ResponseDriller driller = ResponseDriller.CreateGet(url);
                driller.Dump();
                NiceTrace.Message("{0} response: '{1}'", "/" + path, driller.String);
                // Assert.Equals(vers.String, H3Environment.Ver.ToString(3));
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

    public class ResponseDriller
    {

        public static ResponseDriller CreateGet(string uri)
        {
            return Create(new HttpRequestMessage(HttpMethod.Get, uri));
        }
        
        public static ResponseDriller Create(HttpRequestMessage req)
        {
            HttpClient client = new HttpClient();
            req.Headers.Add("User-Agent", "h3control tests");
            var ret = new ResponseDriller(client.SendAsync(req)) {Req = req};
            return ret;
        }

        public Task<HttpResponseMessage> Arg { get; private set; }
        public HttpResponseMessage Result { get; private set; }
        public HttpRequestMessage Req { get; private set; }

        public  byte[] Bytes { get; private set; }

        public ResponseDriller(Task<HttpResponseMessage> arg)
        {
            Arg = arg;
        }

        public string String
        {
            get {  return new UTF8Encoding(false).GetString(Bytes);}
        }


        public HttpResponseMessage Dump()
        {
            var descr = Req.RequestUri;
            
            Stopwatch sw = Stopwatch.StartNew();
            var result = Result = Arg.Result;
            var elaplsed = sw.ElapsedMilliseconds;
            Bytes = result.Content.ReadAsByteArrayAsync().Result;
            var elapsed2 = sw.ElapsedMilliseconds;

            var keys = result.Headers.Select(x => x.Key).OrderBy(x => x).ToList();
            var maxKeyLength = Math.Min(9, keys.Max(x => x.Length)) + 4;
            StringBuilder b = new StringBuilder();
            b.AppendFormat("{0} recieved in {1} msec with status {2} ({3}) {4}. Content recieved in {5} msec",
                descr, elaplsed.ToString("n0"), 
                result.StatusCode, (int) result.StatusCode, result.ReasonPhrase,
                elapsed2).AppendLine();

            b.AppendLine("Response headers:");
            foreach (var k in keys)
            {
                var values = result.Headers.GetValues(k).ToList();
                b.AppendFormat(" {0," + maxKeyLength + "}: {1}", k, values.FirstOrDefault()).AppendLine();
                foreach (var h in values.Skip(1))
                {
                    b.AppendFormat(" {0," + maxKeyLength + "}: {1}", "", values.FirstOrDefault()).AppendLine();
                }
            }

            b.AppendFormat(" {0," + maxKeyLength + "}: {1}", "{CONTENT}", Bytes.Length + " bytes").AppendLine();
            b.AppendLine();
            NiceTrace.Message(b);
            return result;
        }
    }
}