using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Control
{
    using Universe;

    public class DeviceModel
    {
        public bool IsSuccess { get; set; }
        public bool IsLimitSuccess { get; set; }
        public string Host { get; set; }
        public string Ip { get; set; }
        public decimal CpuCur { get; set; }
        public decimal CpuMin { get; set; }
        public decimal CpuMax { get; set; }
        public decimal Tempr { get; set; }
        public decimal DdrCur { get; set; }
        public decimal DdrMin { get; set; }
        public decimal DdrMax { get; set; }
        public string ErrorInfo { get; set; }
        public CpuUsageModel Cpu { get; set; }

        public NewVerListener.BuildInfo VerInfo { get; set; }
        public MemInfo_OnLinix Mem { get; set; }
        public bool HasChangeAccess { get; set; }
        public string OsName { get; set; }

        public static DeviceModel Sample()
        {
            string osName = null;
            DrunkActionExtentions.TryAndForget(() => osName = CrossInfo.OsDisplayName);
            return new DeviceModel()
            {
                CpuCur = 720,
                CpuMax = 1728,
                CpuMin = 480,
                DdrCur = 672,
                DdrMax = 672,
                DdrMin = 408,
                Host = "OrangePI",
                Ip = "192.168.0.15",
                Tempr = 48,
                IsSuccess = true,
                IsLimitSuccess = true,
                OsName = osName,

                Cpu = new CpuUsageModel()
                {
                    Total = new Cpu1UsageModel() {  User = 35.2m, Idle = 30.66m, Nice = 0, System = 34.14m},
                    Cores = new List<Cpu1UsageModel>
                    {
                        new Cpu1UsageModel() {  User = 20.1m, Idle = 30, Nice = 0, System = 49.9m},
                        new Cpu1UsageModel() {  User = 10.2m, Idle = 66, Nice = 0, System = 50},
                        new Cpu1UsageModel() {  User = 12.3m, Idle = 44, Nice = 0, System = 50},
                        new Cpu1UsageModel() {  User = 24.4m, Idle = 77, Nice = 0, System = 50},
                    },
                }
            };
        }
    }

    public class Cpu1UsageModel
    {
        public decimal User { get; set; }
        public decimal Nice { get; set; }
        public decimal System { get; set; }
        public decimal Idle { get; set; }

        public Cpu1UsageModel Clone()
        {
            // return (Cpu1UsageModel) this.MemberwiseClone();
            return new Cpu1UsageModel()
            {
                Idle = Idle,
                Nice = Nice,
                User = User,
                System = System
            };
        }
    }

    public class CpuUsageModel
    {
        public Cpu1UsageModel Total { get; set; }
        public List<Cpu1UsageModel> Cores { get; set; }

        internal CpuUsageModel Clone()
        {
            var ret = new CpuUsageModel()
            {
                Total = this.Total.Clone(),
                Cores = new List<Cpu1UsageModel>()
            };

            foreach (var m in this.Cores)
                ret.Cores.Add(m.Clone());

            return ret;
        }
    }
}
