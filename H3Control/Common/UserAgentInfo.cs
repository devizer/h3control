using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universe
{
    using System.Threading;

    using H3Control.Common;

    using Nancy;
    using Nancy.Bootstrapper;

    public class UserAgentInfo
    {
        // 'Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 5.2; WOW64; Trident/4.0; .NET CLR 2.0.50727; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET4.0C; .NET4.0E; InfoPath.1)'
        // 'Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; WOW64; Trident/4.0; .NET CLR 2.0.50727; .NET CLR 3.0.4506.2152; .NET CLR 3.5.30729; .NET4.0C; .NET4.0E; InfoPath.1)'
        // 'Mozilla/5.0 (Windows; U; Windows NT 5.2; en-US) AppleWebKit/534.16 (KHTML, like Gecko) Chrome/10.0.648.204 Safari/534.16'
        // 'Mozilla/5.0 (Windows NT 6.1; WOW64; rv:47.0) Gecko/20100101 Firefox/47.0'
        // 'Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/51.0.2704.103 Safari/537.36'
        // 'Mozilla/5.0 (iPad; CPU OS 7_1_1 like Mac OS X) AppleWebKit/537.51.2 (KHTML, like Gecko) Version/7.0 Mobile/11D201 Safari/9537.53'
        // IE11: 'Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko'
        public readonly string UserAgent;

        public int Major { get; private set; }
        public int Minor { get; private set; }
        public BowserFamily Family { get; private set; }


        public UserAgentInfo(string userAgent)
        {
            if (userAgent == null)
                throw new ArgumentNullException("userAgent");

            if (userAgent.Length == 0)
                throw new ArgumentException("userAgent is expected", "userAgent");

            UserAgent = userAgent;

            // Do we throw exception?
            try
            {
                Parse();
            }
            catch (Exception ex)
            {
                Family = BowserFamily.Other;
                Major = Minor = 0;
                NiceTrace.Message("Failed to parse User-Agent '{0}'{1}{2}", UserAgent, Environment.NewLine, ex);
            }
        }

        public bool Is_IE8_OrBelow
        {
            get { return Family == BowserFamily.IE && Major > 0 && Major <= 8; }
        }

        public override string ToString()
        {
            StringBuilder ret = new StringBuilder(Family.ToString());
            if (Major > 0)
            {
                ret.Append("/").Append(Major);
                if (Minor > 0)
                    ret.Append('.').Append(Minor);
            }
            
            return ret.ToString();
        }

        private void Parse()
        {
            var probes = Probes;
            BowserFamily family = BowserFamily.Other;
            int major = 0;
            int minor = 0;
            foreach (var probe in probes)
            {
                int p = this.UserAgent.IndexOf(probe.Pattern);
                if (p >= 0)
                {
                    family = probe.Family;
                    if (TryGetVersionByRV(out major, out minor))
                        goto done;
                    if (FindNumber(UserAgent, p + probe.Pattern.Length + 1, out major, out minor))
                        goto done;
                }
            }

            return;

            done:
            Family = family;
            Major = major;
            Minor = minor;
        }

        bool TryGetVersionByRV(out int major, out int minor)
        {
            major = minor = 0;
            if (UserAgent == null)
                return false;

            var arg = " " + UserAgent + " ";
            var p1 = arg.IndexOf("rv:");
            if (p1 > 0)
            {
                if (FindNumber(arg, p1 + 3, out major, out minor))
                    return true;
            }

            return false;
        }



        bool FindNumber(string arg, int p, out int major, out int minor)
        {
            major = minor = 0;
            StringBuilder ret = new StringBuilder();
            for (int i = p; i < arg.Length; i++)
            {
                char c = arg[i];
                if ((c == '.' && ret.ToString().IndexOf(".") >=0 ) || (c >= '0' && c <= '9'))
                    ret.Append(c);
                else
                    break;
            }

            var rawVer = ret.ToString();
            if (rawVer.Length == 0) return false;
            var arr = rawVer.Split('.');
            if (arr.Length == 1)
            {
                if (!int.TryParse(arr[0], out major))
                {
                    major = 0;
                    return false;
                }
                else 
                    return major > 0;
            }

            if (!int.TryParse(arr[0], out major) || !int.TryParse(arr[1], out minor))
            {
                major = minor = 0;
                return false;
            }

            return major > 0;
        }


        public enum BowserFamily
        {
            Other, 
            Edge,
            IE,
            Firefox,
            Opera,
            Chrome,
            Safary,
        }

        private static readonly Probe[] Probes = new[]
        {
            new Probe("Edge", BowserFamily.Edge),
            new Probe("MSIE", BowserFamily.IE),
            new Probe("Trident", BowserFamily.IE),
            new Probe("Firefox", BowserFamily.Firefox),
            new Probe("Opera", BowserFamily.Opera),
            new Probe("OPR", BowserFamily.Opera),
            new Probe("Chrome", BowserFamily.Chrome),
            new Probe("Safari", BowserFamily.Safary),
        };

        class Probe
        {

            public string Pattern;
            public BowserFamily Family;

            public Probe(string pattern, BowserFamily family)
            {
                Pattern = pattern;
                Family = family;
            }
        }

    }

    public static class NancyUserAgentExtention
    {
        static ThreadLocal<string> _UserAgentFullName = new ThreadLocal<string>(() => null);

        public static void BindUserAgentInfo(this IPipelines applicationPipelines)
        {
            // return;
            applicationPipelines.BeforeRequest.AddItemToStartOfPipeline(delegate(NancyContext context)
            {
                var requestHeaders = context.Request.Headers;
                string raw = requestHeaders.UserAgent;
                var ua = raw;
                _UserAgentFullName.Value = ua == "" ? null : ua;
                FirstRound.Only("Catch User-Agent", RoundCounter.Twice, () =>
                {
                    NiceTrace.Message("Request by '{3}' for {1}{2}           {0}", 
                        ua, 
                        context.Request.Url, 
                        Environment.NewLine,
                        context.GetUserAgent());
                });
                
                return null;
            });
            
        }

        // [Paranoja]
        public static UserAgentInfo GetUserAgent(this NancyContext nancyContext)
        {
            try
            {
                return new UserAgentInfo(nancyContext.Request.Headers.UserAgent);
            }
            catch (Exception)
            {
                return new UserAgentInfo(UserAgentInfo.BowserFamily.Other.ToString());
            }
        }
    }
}
