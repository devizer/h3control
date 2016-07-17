namespace H3Control.Tests
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Universe;

    public class ResponseDriller
    {

        public static ResponseDriller CreateGet(string uri)
        {
            return Create(new HttpRequestMessage(HttpMethod.Get, uri));
        }

        public static ResponseDriller CreateGetJson(string uri)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, uri);
            req.Headers.Add("User-Agent", "h3control tests");
            req.Headers.Add("Accept", "application/json");
            var ret = new ResponseDriller(client.SendAsync(req)) { Req = req };
            return ret;
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

        public string StringStart
        {
            get
            {
                var s = String;
                if (s.Length > 50) s = s.Substring(0, 50);
                return s;
            }
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
            var maxKeyLength = Math.Max(19, keys.Max(x => x.Length) + 1);
            int numHeaders = result.Headers.SelectMany(x => x.Value).Count();
            StringBuilder b = new StringBuilder();
            b.AppendFormat(@"{0} response details:
  status recieved in| {1} msec
        (int) status| {2} 
       (enum) status| {3}
     (phrase) status| {4}
 content recieved in| {5} msec
      content length| {6}
             HEADERS| {7} rows",
                descr, elaplsed.ToString("n0"), 
                result.StatusCode, (int) result.StatusCode, result.ReasonPhrase,
                elapsed2.ToString("n0"), Bytes.Length.ToString("n0"),
                numHeaders).AppendLine();

            foreach (var k in keys)
            {
                var values = result.Headers.GetValues(k).ToList();
                b.AppendFormat(" {0," + maxKeyLength + "}: {1}", k, values.FirstOrDefault()).AppendLine();
                foreach (var h in values.Skip(1))
                {
                    b.AppendFormat(" {0," + maxKeyLength + "}: {1}", "", values.FirstOrDefault()).AppendLine();
                }
            }

            b.AppendLine();
            NiceTrace.Message(b);
            return result;
        }
    }
}