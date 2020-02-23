using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universe.LinuxCpuManagement
{
    public class LinuxCpuInfoReader
    {
        private static readonly char[] CpuSeparators = new[] {' ', ',', ';'};
        const string BaseSysPath = "/sys/devices/system/cpu";
        private static readonly string OnlinePath = BaseSysPath + "/online";
        private static readonly string OfflinePath = BaseSysPath + "/offline";
        private static readonly string PossiblePath = BaseSysPath + "/possible";
        private static readonly string PresentPath = BaseSysPath + "/present";
        private static readonly string Online1Path = BaseSysPath + "/cpu1/online";
        private static readonly UTF8Encoding FileEncoding = new UTF8Encoding(false);

        public static LinuxCpuInfo Get()
        {
            LinuxCpuInfo ret = new LinuxCpuInfo()
            {
                OnlineCores = ParseCpuList(ReadFirstLine(OnlinePath)),
                OfflineCores = ParseCpuList(ReadFirstLine(OfflinePath)),
                PresentCores = ParseCpuList(ReadFirstLine(PresentPath)),
                // PossibleCores = ParseCpuList(ReadFirstLine(PossiblePath)),
            };

            bool online1Exists = File.Exists(Online1Path);

            ret.OnlineCores = ret.PresentCores.Union(ret.OnlineCores).ToList();
            ret.OfflineCores = ret.PresentCores.Union(ret.OfflineCores).ToList();
            ret.CanManageOnlineState = ret.PresentCores.Count > 1 && online1Exists;

            return ret;
        }

        public static string ReadFirstLine(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }

            try
            {
                using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var rdr = new StreamReader(fs, FileEncoding))
                {
                    var ret = rdr.ReadLine();
                    return ret;
                }
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }


        static List<int> ParseCpuList(string arg)
        {
            var ret = new List<int>();
            if (arg == null || arg.Length == 0) return new List<int>();
            var parts = arg.Split(CpuSeparators);
            foreach (var part in parts)
            {
                var partTrimmed = part.Trim();
                var p = partTrimmed.IndexOf('-');
                if (p > 0)
                {
                    var rawFrom = partTrimmed.Substring(0, 1);
                    var rawTo = partTrimmed.Substring(p + 1);
                    if (int.TryParse(rawFrom, out var from) && int.TryParse(rawTo, out var to))
                    {
                        for(int i=from; i<=to; i++) ret.Add(i);
                    }
                    else
                        throw new ArgumentException($"Can't parse cpu range '{partTrimmed}' from the '{arg}' list");
                }
                else
                {
                    if (int.TryParse(partTrimmed, out var cpuIndex))
                    {
                        ret.Add(cpuIndex);
                    }
                    else
                        throw new ArgumentException($"Can't parse cpu '{partTrimmed}' from the '{arg}' list");
                }

            }

            return ret;
        }
    }

    public class LinuxCpuInfo
    {
        public List<int> OnlineCores { get; internal set; }
        public List<int> OfflineCores { get; internal set; }
        // public List<int> PossibleCores { get; internal set; }
        public List<int> PresentCores { get; internal set; }
        public bool CanManageOnlineState { get; internal set; }
    }


}
