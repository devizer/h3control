using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Control
{
    using System.Reflection;

    class LoadLogger
    {
        public static void Bind()
        {
            return;

/*
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                Console.WriteLine("RESOLVING: " + args.Name);
                return Assembly.Load(args.Name);
            };
*/

            AppDomain.CurrentDomain.AssemblyLoad += (sender, args) =>
            {
                Console.WriteLine("LOADED: " + args.LoadedAssembly.GetName());
            };
        }
    }
}
