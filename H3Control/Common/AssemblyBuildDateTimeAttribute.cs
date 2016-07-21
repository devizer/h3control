namespace Universe
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public class AssemblyBuildDateTimeAttribute : Attribute
    {
        private DateTime _UtcBuiltAt;
        
        public AssemblyBuildDateTimeAttribute(long secondsSince1970)
        {
            _UtcBuiltAt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(secondsSince1970);
        }

        // date --utc +"%a, %d %b %Y %T GMT"
        public AssemblyBuildDateTimeAttribute(string rfc1123Date)
        {
            if (rfc1123Date == null)
                throw new ArgumentNullException("rfc1123Date");

            if (string.IsNullOrWhiteSpace(rfc1123Date))
                throw new ArgumentException("rfc1123Date is empty");

            if (!DateTime.TryParseExact(
                rfc1123Date, 
                "R", 
                new CultureInfo("en-US"), 
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal,
                out _UtcBuiltAt))
                throw new ArgumentException(string.Format(
                    "Unable to parse date {0} using RFC1123 pattern.",
                    rfc1123Date));

        }

        public DateTime UtcBuiltAt
        {
            get { return _UtcBuiltAt; }
        }

        public static DateTime? CallerUtcBuildDate
        {
            get
            {
                return GetBuildDate(Assembly.GetCallingAssembly());
            }
        }

        public static bool HasCallerUtcBuildDate
        {
            get { return CallerUtcBuildDate.HasValue; }
        }

        public static DateTime? GetBuildDate(Assembly assembly)
        {
            IEnumerable<Attribute> arr = assembly.GetCustomAttributes(typeof(AssemblyBuildDateTimeAttribute));
            AssemblyBuildDateTimeAttribute attr = (AssemblyBuildDateTimeAttribute) arr.FirstOrDefault();
            return attr == null ? (DateTime?)null : attr.UtcBuiltAt;
        }
    }
}