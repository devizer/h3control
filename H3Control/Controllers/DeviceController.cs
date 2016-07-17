using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace H3Control.Controllers
{
    using System.Web.Http;

    public class DeviceController : ApiController
    {
        [HttpGet]
        public object Ping()
        {
            return new { Pong = "ok"};
        }

        [HttpGet]
        public DeviceModel GetDevice(string device)
        {
            try
            {
                var ret =
                    !H3Environment.IsH3
                        ? DeviceModel.Sample()
                        : DeviceDataSource.GetLocal();

                ret.VerInfo = NewVerListener.Info;
                if (!H3Environment.IsH3)
                    ret.Mem = new MemInfo_OnLinix()
                    {
                        Total = 1234 * 1024,
                        SwapFree = 567 * 1024,
                        SwapTotal = 567 * 1024,
                        BuffersAndCache = 789 * 1024,
                        Free = 321 * 1024,
                        Buffers = 73 * 1024
                    };

                return ret;
            }
            catch (Exception ex)
            {
                var ret = DeviceModel.Sample();
                ret.IsSuccess = false;
                ret.ErrorInfo = ex.ToString();
                return ret;
            }
        }
    }

    public class ControlStatus
    {
        public bool IsOk { get; set; }
        public string Error { get; set; }
    }

}
