namespace H3Control
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class MemInfoParser_OnLinux
    {
        public static bool TryParse(out MemInfo_OnLinix info)
        {
            info = null;
            if (!File.Exists("/proc/meminfo"))
                return false;

            long? memTotal = null;
            long? memFree = null;
            long? buffers = null;
            long? cached = null;
            long? swapTotal = null;
            long? swapFree = null;

            StringComparer c = StringComparer.InvariantCultureIgnoreCase;
            using (FileStream fs = new FileStream("/proc/meminfo", FileMode.Open, FileAccess.Read, FileShare.Read))
            using (StreamReader rd = new StreamReader(fs, Encoding.UTF8))
            {
                string line;
                while ((line = rd.ReadLine()) != null)
                {
                    if (line.EndsWith(" Kb", StringComparison.InvariantCultureIgnoreCase))
                        line = line.Substring(0, line.Length - 3);

                    var arr = line.Split(':');
                    if (arr.Length != 2)
                        continue;


                    string rawKey = arr[0].Trim();
                    string rawVal = arr[1].Trim();
                    long val;
                    if (!long.TryParse(rawVal, out val))
                        continue;

                    if (c.Equals(rawKey, "memTotal")) memTotal = val;
                    if (c.Equals(rawKey, "memFree")) memFree = val;
                    if (c.Equals(rawKey, "buffers")) buffers = val;
                    if (c.Equals(rawKey, "cached")) cached = val;
                    if (c.Equals(rawKey, "swapTotal")) swapTotal = val;
                    if (c.Equals(rawKey, "swapFree")) swapFree = val;
                }
            }

            var all = new long?[] {memTotal, memFree, buffers, cached, swapTotal, swapFree};
            // Console.WriteLine("ALL: " + string.Join(", ", all.Select(x => Convert.ToString(x))));
            if (!all.All(x => x.HasValue))
                return false;

            info = new MemInfo_OnLinix()
            {
                Total = memTotal.Value,
                Free = (memFree + buffers + cached).Value,
                BuffersAndCache = (buffers + cached).Value,
                Buffers = buffers. Value, 
                SwapFree = swapFree.Value,
                SwapTotal = swapTotal.Value

            };

            Trace.WriteLine("MemInfo_OnLinix: " + Environment.NewLine + "   " + info);

            return true;
        }
    }
}