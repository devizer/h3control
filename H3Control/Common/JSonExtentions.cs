namespace Universe
{
    using System.IO;
    using System.Text;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public static class JSonExtentions
    {
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