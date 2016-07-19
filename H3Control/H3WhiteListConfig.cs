namespace H3Control
{
    using System.Collections.Generic;
    using System.Linq;

    public class H3WhiteListConfig
    {
        public static string WhiteListArg;

        public static bool HasWhiteList
        {
            get { return WhiteList.Count > 0; }
        }

        public static HashSet<string> WhiteList
        {
            get
            {
                var s = WhiteListArg;
                return s != null && s.Length > 0
                    ? new HashSet<string>(s
                        .Split(',', ';')
                        .Select(x => x.Trim())
                        .Where(x => x.Length > 0))
                    : new HashSet<string>();
            }
        }
    }
}