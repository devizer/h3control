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
            try
            {
                err = Console.Out;
            }
            catch (Exception)
            {
            }

            try
            {
                Console.SetError(TextWriter.Null);
            }
            catch (Exception)
            {
            }

            Dictionary<string, List<string>> ret = new Dictionary<string, List<string>>();
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface adapter in adapters)
            {
                var uniList = adapter
                    .GetIPProperties()
                    .UnicastAddresses
                    .Select(x => x.Address)
                    .ToList();

                if (uniList.Count > 0)
                    ret[adapter.Description] = uniList
                        .Select(x => x.ToString())
                        .Where(x => !string.IsNullOrEmpty(x))
                        .ToList();
            }

            try
            {
                if (err != null) Console.SetError(err);
            }
            catch (Exception)
            {
            }

            return ret;
        }
    }
}