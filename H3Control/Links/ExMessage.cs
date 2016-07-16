using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3Control.Links
{
    public static class ExMessage
    {
        public static string Get(this Exception ex)
        {
            List<string> ret = new List<string>();
            while (ex != null)
            {
                ret.Add(string.Format("[{0}] {1}", ex.GetType().Name, ex.Message));
                ex = ex.InnerException;
            }

            ret.Reverse();
            return string.Join(" <--- ", ret);
        }
    }
}
