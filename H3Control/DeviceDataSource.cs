namespace H3Control
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Runtime.Caching;
    using System.Text;
    using System.Threading;

    using Universe;

    public class DeviceDataSource
    {

        public static DeviceModel GetLocal()
        {
            return GetLocalImpl();
            return GetOrPerform(
                "My Device Info",
                TimeSpan.FromSeconds(0.2),
                () => GetLocalImpl()
                );
        }

        public static DeviceModel GetLocalImpl()
        {
            var ret = DeviceModel.Sample();
            int curCpu, curDdr = 672, tempr = 0;
            int.TryParse(ReadSmallFile("/sys/devices/system/cpu/cpu0/cpufreq/scaling_cur_freq"), out curCpu);
            var legacyTempPath = "/sys/devices/virtual/hwmon/hwmon1/temp1_input";
            var mainlineTempPath = "/sys/devices/virtual/thermal/thermal_zone0/temp";
            // TODO: Raspbian
            var raspbianTempPath = "/sys/class/thermal/thermal_zone0/temp";
            if (File.Exists(legacyTempPath))
            {
                int.TryParse(ReadSmallFile(legacyTempPath), out tempr);
            }
            else if (File.Exists(mainlineTempPath))
            {
                int.TryParse(ReadSmallFile(mainlineTempPath), out tempr);
                tempr = tempr / 1000;
            }

            var legacyDdrFrequency = "/sys/devices/platform/sunxi-ddrfreq/devfreq/sunxi-ddrfreq/cur_freq";
            if (File.Exists(legacyDdrFrequency))
            {
                int.TryParse(ReadSmallFile(legacyDdrFrequency), out curDdr);
                curDdr = curDdr / 1000;
            }

            ret.CpuCur = curCpu / 1000;
            ret.DdrCur = curDdr == 0 ? 666 : curDdr;
            ret.Tempr = tempr;
            try
            {
                int cpuMax, cpuMin;
                int.TryParse(ReadSmallFile("/sys/devices/system/cpu/cpu0/cpufreq/scaling_max_freq"), out cpuMax);
                int.TryParse(ReadSmallFile("/sys/devices/system/cpu/cpu0/cpufreq/scaling_min_freq"), out cpuMin);
                ret.CpuMax = cpuMax / 1000;
                ret.CpuMin = cpuMin / 1000;

                int ddrMax=672000, ddrMin=672000;
                if (H3Environment.IsLegacyDdr)
                {
                    int.TryParse(ReadSmallFile("/sys/devices/platform/sunxi-ddrfreq/devfreq/sunxi-ddrfreq/scaling_max_freq"), out ddrMax);
                    int.TryParse(ReadSmallFile("/sys/devices/platform/sunxi-ddrfreq/devfreq/sunxi-ddrfreq/scaling_min_freq"), out ddrMin);
                }

                ret.DdrMax = ddrMax / 1000;
                ret.DdrMin = ddrMin / 1000;
                ret.IsLimitSuccess = true;
            }
            catch (Exception)
            {
                ret.IsLimitSuccess = false;
            }

            try
            {
                MemInfo_OnLinix info;
                if (MemInfoParser_OnLinux.TryParse(out info))
                    ret.Mem = info;
            }
            catch (Exception ex)
            {
                NiceTrace.Message("MemInfo Parser failed" + Environment.NewLine + ex);
            }

            ret.Host = HostName;
            new Action(() => { ret.OsName = CrossInfo.OsDisplayName; }).TryAndForget();

            ret.Cpu = CpuUsageListener_OnLinux.CpuUsage;

            // Online Cores Count
            {
                int onlineCount = 0;
                onlineCount = GetOnlineCount(onlineCount);

                ret.OnlineCoresNumber = onlineCount;
            }


            // NiceTrace.Message("Device.Cpu is {0}", JSonExtentions.ToNewtonJSon(ret.Cpu, true));
            return ret;
        }

        private static int GetOnlineCount(int onlineCount)
        {
            const string formatName = "/sys/devices/system/cpu/cpu{0}/online";
            const int coresTotal = 4;
            for (int core = 0; core < coresTotal; core++)
            {
                string file = string.Format(formatName, core);
                if (Directory.Exists(Path.GetDirectoryName(file)))
                {
                    bool isCoreOnline = false;
                    if (!File.Exists(file) && core == 0)
                    {
                        isCoreOnline = true;
                    }
                    else
                    {
                        var rawIsOnline = ReadSmallFile(file);
                        int isOnline;
                        if (int.TryParse(rawIsOnline, out isOnline))
                            isCoreOnline = isOnline > 0;
                    }

                    if (isCoreOnline) onlineCount += 1;
                }
            }

            return onlineCount;
        }

        private static int GetOnlineCount_Legacy(int onlineCount)
        {
            const string formatName = "/sys/devices/system/cpu/cpu{0}/online";
            const int coresTotal = 4;
            for (int core = 0; core < coresTotal; core++)
            {
                string file = string.Format(formatName, core);
                if (Directory.Exists(Path.GetDirectoryName(file)))
                {
                    var rawIsOnline = ReadSmallFile(file);
                    int isOnline;
                    if (int.TryParse(rawIsOnline, out isOnline))
                        onlineCount += isOnline > 0 ? 1 : 0;
                }
            }

            return onlineCount;
        }

        public static string HostName
        {
            get { return Environment.MachineName; }
        }

        
        static Lazy<MemoryCache> Cache = new Lazy<MemoryCache>(() =>
        {
            var config = new NameValueCollection();
            config.Add("pollingInterval", "00:01:00");
            config.Add("physicalMemoryLimitPercentage", "0");
            config.Add("cacheMemoryLimitMegabytes", "1");
            var ret = new MemoryCache("api", config);
            return ret;
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        static T GetOrPerform<T>(string id, TimeSpan leavingTime, Func<T> performAction)
        {
            MemoryCache cache = Cache.Value;
            object raw = cache.Get(id);
            T ret;
            if (raw == null)
            {
                ret = performAction();
                CacheItemPolicy pol = new CacheItemPolicy()
                {
                    SlidingExpiration = leavingTime,
                    // AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow + leavingTime),
                };

                
                cache.Add(id, ret, pol);
            }
            else
            {
                ret = (T) raw;

            }

            return ret;
        }

        static readonly UTF8Encoding UTF8 = new UTF8Encoding(false);
        public static string ReadSmallFile(string name)
        {
            using (FileStream fs = new FileStream(name, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader rdr = new StreamReader(fs, UTF8))
            {
                var ret = rdr.ReadToEnd();
                return ret.Trim(' ', '\n', '\r', '\t');
            }
        }
    }
}
