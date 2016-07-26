﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Control
{
    using System.IO;
    using System.Reflection;
    using System.Threading;

    using Common;

    using Universe;

    public static class H3Environment
    {
        public static class ProcessesDefaults
        {
            public static readonly int TopN = 5;
            public static readonly PsSortOrder Order = PsSortOrder.Rss;
        }
        
        static Lazy<Version> _Ver = new Lazy<Version>(() =>
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        });

        static Lazy<string> _VerAsPublic = new Lazy<string>(() =>
        {
            return Ver.ToString(3);
        });

        static Lazy<DateTime> _BuiltAtUtc = new Lazy<DateTime>(() =>
        {
            var ret = AssemblyBuildDateTimeAttribute.CallerUtcBuildDate;
            if (!ret.HasValue)
                throw new InvalidOperationException("[AssemblyBuildDateTime] is absent in H3Control assembly");

            return ret.Value;
        });

        public static DateTime BuiltAtUtc
        {
            get { return _BuiltAtUtc.Value; }
        }

        public static string VerAsPublic
        {
            get { return _VerAsPublic.Value; }
        }
        
        public static Version Ver
        {
            get { return _Ver.Value; }
        }

        public static bool IsH3
        {
            get { return _IsH3.Value; }
        }

        static readonly Lazy<bool> _IsH3 = new Lazy<bool>(() =>
        {
            try
            {
                return
                    Directory.Exists("/sys/devices/platform/sunxi-ddrfreq/devfreq/sunxi-ddrfreq")
                    && Directory.Exists("/sys/devices/virtual/hwmon/hwmon1")
                    && Directory.Exists("/sys/devices/system/cpu/cpu0/cpufreq");
            }
            catch (Exception ex)
            {
                NiceTrace.Message("INFO: Device isnt a H3 board. " + ex.Message);
                return false;
            }
        }, LazyThreadSafetyMode.ExecutionAndPublication);

        public static bool IsRelease
        {
            get
            {
#if DEBUG
                return false;
#else
                return true;
#endif
            }
        }
        

    }



}
