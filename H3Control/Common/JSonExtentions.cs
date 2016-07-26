namespace Universe
{
    using System;
    using System.IO;
    using System.Text;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class JSonExtentions
    {
        public static string ToJson(this bool arg)
        {
            return arg ? "true" : "false";
        }

        public static string ToJson(this DateTime arg)
        {
            return ToNewtonJSon(arg, false);
        }

        public static string ToJson(this string arg)
        {
            if (arg == null)
                return "null";

            StringBuilder ret = new StringBuilder("\"");
            foreach (var c in arg)
            {
                if (c == '\\') ret.Append("\\\\");
                else if (c == '\"') ret.Append(@"""");
                else if (c < 32) ret.Append(((int) c).ToString("X4"));
                else ret.Append(c);
            }

            ret.Append("\"");
            return ret.ToString();
        }

        public static void CheckFormat(string candidate, string context = null)
        {
            string suffix = string.IsNullOrWhiteSpace(context) ? "" : (" (" + context + ")");
            if (candidate == null)
                throw new ArgumentNullException("candidate", "Null string is invalid json" + suffix);

            if (string.IsNullOrWhiteSpace(candidate))
                throw new ArgumentException("Empty or white space string is invalid json" + suffix, "candidate");

            try
            {
                JObject obj = JObject.Parse(candidate);
            }
            catch (Exception ex)
            {
                string trimmed = candidate.Length > 20 ? candidate.Substring(0, 20) : candidate;
                throw new ArgumentException("Invalid json string" + suffix + ". String starts with " + trimmed, "candidate", ex);
            }
            
        }
        
        public static string Format(string arg)
        {
            /*
                        dynamic parsedJson = JsonConvert.DeserializeObject(arg);
                        return JsonConvert.SerializeObject(parsedJson, Formatting.Indented); 
            */

            JObject obj = JObject.Parse(arg);
            StringBuilder b = new StringBuilder();
            StringWriter wr = new StringWriter(b);
            JsonTextWriter jwr = new JsonTextWriter(wr);
            jwr.Formatting = Formatting.Indented;
            jwr.IndentChar = ' ';
            jwr.Indentation = 6;

            JsonSerializer ser = new JsonSerializer();
            ser.Formatting = Formatting.Indented;
            ser.Serialize(jwr, obj);
            jwr.Flush();
            string ret = b.ToString();
            return ret;
        }

        public static string ToNewtonJSon(object arg, bool isIntended = false)
        {
            if (arg == null) return "<null>";
            StringBuilder b = new StringBuilder();
            StringWriter wr = new StringWriter(b);
            JsonTextWriter jwr = new JsonTextWriter(wr);
            jwr.Formatting = isIntended ? Formatting.Indented : Formatting.None;
            if (isIntended)
            {
                jwr.IndentChar = ' ';
                jwr.Indentation = 6;
            }

            JsonSerializer ser = new JsonSerializer();
            ser.Formatting = isIntended ? Formatting.Indented : Formatting.None;
            ser.Serialize(jwr, arg);
            jwr.Flush();
            string ret = b.ToString();
            return ret;
        }
    }
}