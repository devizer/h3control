using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Control
{
    using System.Reflection;

    class MyEnvironment
    {
        static Lazy<Version> _Ver = new Lazy<Version>(() =>
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        });

        public static Version Ver
        {
            get { return _Ver.Value; }
        }
    }

}
