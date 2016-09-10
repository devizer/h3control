using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Universe
{
    public class HashExtentions
    {
        private static readonly UTF8Encoding Utf8 = new UTF8Encoding(false);

        public static string SHA1(string arg)
        {
            if (arg == null)
                throw new ArgumentNullException("arg");

            var sha1 = System.Security.Cryptography.SHA1.Create();
            var hash = string.Join("", sha1.ComputeHash(Utf8.GetBytes(arg)).Select(x => x.ToString("X2")));
            return hash;
        }
    }
}
