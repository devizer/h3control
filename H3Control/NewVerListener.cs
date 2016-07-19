﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Control
{
    using System.Net;
    using System.Net.Http;
    using System.Threading;

    using Common;

    using Links;

    using Newtonsoft.Json;

    using Universe;

    public class NewVerListener
    {

        // { version: '1.5.77', date: 1447211522 }
        // private const string _url = "https://www.dropbox.com/s/ikvj3edhovhh8ow/h3control-version.json?dl=1";
        private const string _url = "https://github.com/devizer/h3control-bin/raw/master/public/h3control-version.json";
        
        static BuildInfo _info;
        static object _sync = new object();
        static Thread _thread;

        public static BuildInfo Info
        {
            get { lock (_sync) return _info == null ? null : _info.Clone(); }
            set { lock (_sync) _info = value; }
        }

        public static void Listen()
        {
            lock (_sync)
            {
                if (_thread == null)
                {
                    Info = new BuildInfo() {CurVer = H3Environment.Ver};
                    if (Environment.OSVersion.Platform == PlatformID.Unix)
                    {
                        ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) =>
                        {
                            return true;
                        };
                    }

                    _thread = new Thread(() =>
                    {
                        while (true)
                        {
                            Thread.Sleep(1000);
                            RefreshInfo();
                            Thread.Sleep(5*60*1000);
                        }
                    }) { IsBackground = true, Name = "New Version Info Listener" };

                    _thread.Start();
                }
            }
        }

        private static void RefreshInfo()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var message = client.GetAsync(_url).Result;
                    var rawString = message.Content.ReadAsStringAsync().Result;
                    // Console.WriteLine("NEW VERINFO STRING: " + rawString);
                    BuildInfoRaw raw = JsonConvert.DeserializeObject<BuildInfoRaw>(rawString);
                    // Console.WriteLine("NEW VERINFO OBJECT: " + raw);
                    if (raw != null && raw.version != null)
                    {
                        string[] arr = raw.version.Split('.');
                        if (arr.Length == 3)
                        {
                            BuildInfo newValue = new BuildInfo()
                            {
                                NewVer = new Version(raw.version + ".0"),
                                Date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(raw.date).ToLocalTime(),
                                CurVer = H3Environment.Ver,
                            };

                            Info = newValue;
                            FirstRound.Only("Fresh Public Version", RoundCounter.Twice, () =>
                            {
                                NiceTrace.Message("Info about FRESH PUBLIC arrived. {0}.", newValue);
                            });

                            if (newValue.IsNew)
                            {
                                NiceTrace.Message("FRESH PUBLIC version is new one. " + newValue.NewVer);
                            }
                        }
                    }


                }
            }
            catch (Exception ex)
            {
                NiceTrace.Message("Check new version by {0} failed. {1}", _url, ex.Get());
            }
        }

        // Input schema
        public class BuildInfoRaw
        {
            public string version { get; set; }
            public long date { get; set; }

            public override string ToString()
            {
                return string.Format("version: {0}, date: {1}", version, date);
            }
        }


        // Public schema
        public class BuildInfo
        {
            public Version CurVer { get; set; }
            public Version NewVer { get; set; }
            public DateTime Date { get; set; }

            public bool IsNew
            {
                get { return NewVer != null && CurVer != null && NewVer.CompareTo(CurVer) > 0; }
            }

            public BuildInfo Clone()
            {
                return new BuildInfo() { Date = Date, NewVer = NewVer, CurVer = CurVer};
            }

            public override string ToString()
            {
                return string.Format("CurVer: {0}, NewVer: {1}, Date: {2}, IsNew: {3}", CurVer, NewVer, Date, IsNew);
            }
        }
    }
}
