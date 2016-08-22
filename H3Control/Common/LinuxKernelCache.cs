namespace H3Control.Common
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using Universe;

    public class LinuxKernelCache
    {
        public static void Flush(string stage = "on startup")
        {
            Exception ex1 = null, ex2 = null;
            try
            {
                string output;
                int code;
                CrossInfo.HiddenExec("sync", "", out output, out code);
            }
            catch (Exception ex)
            {
                ex1 = ex;
            }

            try
            {
                File.WriteAllText("/proc/sys/vm/drop_caches", "3");
            }
            catch (Exception ex)
            {
                ex2 = ex;
            }

            if (ex2 == null)
                NiceTrace.Message("Kernel cache syccesefully flushed {0}", stage);
            else
                NiceTrace.Message("Kernel cache wasn't flushed {0}{1}{2}", stage, Environment.NewLine, ex2);
        }
    }
}