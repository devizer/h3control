namespace Universe
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.NetworkInformation;

    public static class NetworkInterfaceExtentions
    {
        public static Dictionary<string, List<string>>  GetDescription()
        {
            
            TextWriter err = null;
            TryOnMono(() => err = Console.Out);
            TryOnMono(() => Console.SetError(TextWriter.Null));

            try
            {
                Dictionary<string, List<string>> ret = new Dictionary<string, List<string>>();
                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface adapter in adapters)
                {
                    var d = adapter.Description;
                    if (string.IsNullOrEmpty(d)) d = "unknown #" + (ret.Count + 1);

                    var uniList = adapter
                        .GetIPProperties()
                        .UnicastAddresses
                        .Select(x => x.Address)
                        .Select(x => x.ToString())
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToList();

                    if (uniList.Count > 0)
                        ret[d] = uniList;
                }

                return ret;
            }
            finally
            {
                if (err != null)
                    TryOnMono(() => Console.SetError(err));
            }


        }

        static void TryOnMono(Action action)
        {
            bool isMono = Type.GetType("Mono.Runtime", false) != null;
            if (!isMono)
                return;

            try
            {
                action();
            }
            catch (Exception)
            {
            }
        }
    }
}