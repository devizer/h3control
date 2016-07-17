using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Control.Tests
{
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;

    class TestEnvironment
    {
        private static int TcpPort = 0;

        public static int GetTcpPort()
        {
            if (TcpPort == 0)
            {
                TcpListener l = new TcpListener(IPAddress.Loopback, 0);
                l.Start();
                int port = ((IPEndPoint)l.LocalEndpoint).Port;
                l.Stop();
                TcpPort = port;
                Trace.WriteLine("Working port is: " + port);
                return port;
            }

            return TcpPort;
        }

        public static void ResetTcpPort()
        {
            TcpPort = 0;
        }
    }
}
