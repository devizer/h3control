namespace H3Control.Controllers
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading;
    using System.Web.Http;

    using Universe;

    public class ControlController : ApiController
    {
        static readonly CultureInfo EnUs = CultureInfo.GetCultureInfo("en-US");

        public static readonly int[] SupportedDdrFreq = new[]
        {
            408000, 432000, 456000, 480000, 
            504000, 528000, 552000, 576000, 
            600000, 624000, 648000, 672000
        };

        [HttpPost]
        public ControlStatus SetCoreNumbers(int coresCount)
        {
            const string formatName = "/sys/devices/system/cpu/cpu{0}/online";
            const int coresTotal = 4;

            Exception error = null;
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    for (int core = 0; core < coresTotal; core++)
                    {
                        string file = string.Format(formatName, core);
                        bool isFileExists = File.Exists(file);
                        // New kernel doesn't allow to control core #0
                        if (!isFileExists && core == 0) continue;
                        var prevValue = DeviceDataSource.ReadSmallFile(file);
                        var newValue = core + 1 <= coresCount ? "1" : "0";
                        if (prevValue == newValue) continue;
                        using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.None))
                        using (StreamWriter wr = new StreamWriter(fs, Encoding.ASCII))
                        {
                            wr.Write(newValue);
                        }
                    }

                    return new ControlStatus() { IsOk = true };
                }
                catch (Exception ex)
                {
                    if (error == null)
                        error = ex;
                }
                Thread.Sleep(1);
            }

            NiceTrace.Message("Unable to change online cores number to value " + coresCount + Environment.NewLine + error);
            return new ControlStatus() { IsOk = false, Error = Convert.ToString(error)};
        }


        [HttpPost]
        public ControlStatus Control(string side, string freq)
        {
            if (freq == null)
                throw new ArgumentNullException("freq");

            if (side == null)
                throw new ArgumentNullException("side");

            int newValue;
            if (!Int32.TryParse(freq, out newValue))
                throw new ArgumentException("Invalid frequency argument", "freq");

            if (side == "updatespeed")
            {
                var f = Math.Min(5000, Math.Max(100, newValue));
                CpuUsageListener_OnLinux.Bind(f);
                Trace.WriteLine("UPDATE SPEED: " + f + " MSEC");
                return new ControlStatus() { IsOk = true };
            }

            if (!H3Environment.IsH3)
                return new ControlStatus() { IsOk = true /*, Addition_Info = "Device isnt equipped with H3"*/ };

            Exception error = null;
            if (side == "ddr")
            {
                // api/control/ddr/{freq}
                // paranoja?
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        const string format = "/sys/devices/platform/sunxi-ddrfreq/devfreq/sunxi-ddrfreq/scaling_{0}_freq";
                        string pathMin = string.Format(format, "min");
                        string pathMax = string.Format(format, "max");
                        File.WriteAllText(pathMin, freq + "000");
                        File.WriteAllText(pathMax, freq + "000");
                        ThreadPool.QueueUserWorkItem(state =>
                        {
                            Action drunk = () =>
                            {
                                Thread.Sleep(1); // doesnt work
                                File.WriteAllText(pathMin, "408" + "000");
                                NiceTrace.Message("Written 408000 to {0}", pathMin);
                            };
                            drunk.TryAndForget();
                        });

                        return new ControlStatus() {IsOk = true};
                    }
                    catch (Exception ex)
                    {
                        if (error == null)
                            error = ex;
                    }

                    Thread.Sleep(1);
                }

            }
            else if (side == "cpu-min" || side == "cpu-max")
            {
                string suffix = side == "cpu-min" ? "min" : "max";
                string file = string.Format("/sys/devices/system/cpu/cpu0/cpufreq/scaling_{0}_freq", suffix);
                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        using (FileStream fs = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.Write))
                        using (StreamWriter wr = new StreamWriter(fs, Encoding.ASCII))
                        {
                            wr.Write((newValue*1000).ToString("0", EnUs));
                        }
                        return new ControlStatus() {IsOk = true};
                    }
                    catch (Exception ex)
                    {
                        if (error == null)
                            error = ex;
                    }

                    Thread.Sleep(1);
                }
            }
            else
            {
                return new ControlStatus() { IsOk = false, Error = "Unknown command " + side };
            }
            

            // var deb = side + ": " + freq;
            // Debug.WriteLine("Control request is " + deb);
            NiceTrace.Message("Control command '{0}' with freq='{1}' filed {2}{3}", side, freq, Environment.NewLine, error);
            return new ControlStatus() { IsOk = false, Error = Convert.ToString(error) };
        }

    }
}