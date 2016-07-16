using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Universe.MediaProbe.Utils
{
    public class SmartyTemp
    {
        public static string GetTempFolderSmarty(string tempName)
        {
            List<string> vars = new List<string>()
            {
                Environment.GetEnvironmentVariable("SMARTY_TEMP"),

                Environment.GetEnvironmentVariable("HOME"),

                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),

                Environment.GetEnvironmentVariable("TEMP"),

                Environment.GetEnvironmentVariable("TMP"),

            };

            if (Environment.OSVersion.Platform != PlatformID.Unix)
            {
                vars.Add(
                    Path.Combine(
                        Environment.SystemDirectory.Substring(0, 3),
                        "Temp"));
            }
            else
            {
                vars.Clear();
                vars.Add(
                    Path.Combine(
                        Environment.GetEnvironmentVariable("HOME"),
                        ".tmp"));
            }

            foreach (var d2 in vars)
            {

                if (string.IsNullOrEmpty(d2))
                    continue;

                var d = Path.Combine(d2, tempName);

                try
                {
                    Directory.CreateDirectory(d);
                }
                catch { }

                if (Directory.Exists(d))
                {
                    string name = Guid.NewGuid().ToString();
                    var tryFullName = Path.Combine(d, name);
                    try
                    {

                        using (FileStream fs = new FileStream(tryFullName, FileMode.Create, FileAccess.Write, FileShare.Read))
                        {
                            fs.WriteByte(1);
                            return d;
                        }
                    }
                    catch
                    {
                    }
                    finally
                    {
                        File.Delete(tryFullName);
                    }


                }
            }

            throw new InvalidOperationException("Failed to access temporary Folder");
        }

        public static void KillOnRestart(string fileOrFolderName)
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix) return;

            MoveFileEx(fileOrFolderName, null, MOVEFILE_DELAY_UNTIL_REBOOT);
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, int dwFlags);
        internal static readonly int MOVEFILE_DELAY_UNTIL_REBOOT = 4;


    }
}
