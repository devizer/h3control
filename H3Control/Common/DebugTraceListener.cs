using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Universe
{
    using System.Linq;

    public class NiceTrace
    {
        public static void Message(string msg)
        {
            Trace.WriteLine(msg);
        }

        public static void Message(object msg)
        {
            Trace.WriteLine(Convert.ToString(msg));
        }

        public static void Message(string format, params object[] args)
        {
            Trace.WriteLine(string.Format(format, args));
        }
    }

    public class DebugTraceListener
    {
        private static string _LogFolder;
        internal const string LogExtension = ".log";

        public static string LogFolder
        {
            get { return _LogFolder; }
        }

        public static void Bind()
        {
            if (!EnableTracing)
                return;

            bool isFound = false;
            const string listenerName = "Default Universe Debug Output";

            lock(Trace.Listeners)
            {
                DefaultTraceListener dtl = Debug.Listeners.OfType<DefaultTraceListener>().FirstOrDefault();
                // if (dtl != null) Debug.Listeners.Remove(dtl);
                var dtl2 = Trace.Listeners.OfType<DefaultTraceListener>().FirstOrDefault();
                // if (dtl2 != null) Trace.Listeners.Remove(dtl2);

                
                foreach (TraceListener listener in Debug.Listeners)
                {
                    if (listener is UniverseTraceListener && listener.Name == listenerName)
                    {
                        isFound = true;
                        break;
                    }
                }

                if (!isFound)
                {

                    try
                    {
                        string file = FileName;
                        var logFolder = Path.GetDirectoryName(file);
                        _LogFolder = logFolder;
                        Directory.CreateDirectory(logFolder);
                        // FileStream fs = new FileStream(file, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                        // StreamWriter wr = new StreamWriter(fs, Encoding.Default);
                        {
                            // wr.AutoFlush = true;
                            // TextWriterTraceListener tl2 = new TextWriterTraceListener(wr, "Default BioLink Debug Output");
                            // если падает то listener не добавляется
                            UniverseTraceListener tl2 = new UniverseTraceListener(FolderName, listenerName);
                            Trace.AutoFlush = true;
                            Trace.Listeners.Add(tl2);
                        }
                    }
                    catch
                    {
                    }

                }
            }

            if (!isFound)
            {
                int globalCounter = 0;
                AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                {
                    globalCounter++;
                    Trace.WriteLine("UNHANDLED GLOBAL EXCEPTION " + globalCounter + ":" + Environment.NewLine + args.ExceptionObject);
                };
                
                
                string warning = ". Warning: TRACE compiler option is absent";
#if TRACE
                warning = string.Empty;
#endif 
                Trace.WriteLine("'" + listenerName + "' Trace Logger was Bound to '" + FileName + "'" + warning);
            }


#if !DEBUG
            if (!isFound)
                Trace.Listeners.Add(new DefaultTraceListener());
#endif

            if (!isFound)
            {
                CleanUp(); 
            }
        }

        static bool EnableTracing
        {
            get
            {
                var env = Environment.GetEnvironmentVariable("DISABLE_UNIVERSE_TRACE");
                if (env == null) return true;
                if (env.Equals("1", StringComparison.InvariantCultureIgnoreCase)
                    || env.Equals("yes", StringComparison.InvariantCultureIgnoreCase)
                    || env.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                    return false;

                return false;
            }
        }

        private static volatile bool IsCleanUP = false;
        internal static void CleanUp()
        {
            if (IsCleanUP)
                return;

            IsCleanUP = true;

            ThreadPool.QueueUserWorkItem(
                delegate
                    {
                        try
                        {
                            DebugTraceLimits limits = DebugTraceLimits.Default;
                            string folder = FolderName;
                            FileInfo[] fla = new DirectoryInfo(folder).GetFiles("*" + LogExtension);
                            DateTime[] dates = new DateTime[fla.Length];
                            for (int i = 0; i < dates.Length; i++)
                                dates[i] = fla[i].CreationTime;

                            Array.Sort(dates, fla);
                            Array.Reverse(fla);
                            List<string> names = new List<string>();
                            int n = 0;
                            long SumSize = 0;
                            DateTime lastDate = DateTime.Now - TimeSpan.FromHours(limits.HoursLimit);
                            bool deed = false;
                            foreach (FileInfo fi in fla)
                            {
                                if (deed)
                                    names.Add(fi.FullName);

                                else
                                {
                                    n++;
                                    if (n > limits.FilesLimit)
                                        deed = true;

                                    SumSize += fi.Length;
                                    if (SumSize > 1024L*1024L*limits.TotalSizeLimit)
                                        deed = true;

                                    if (fi.CreationTime < lastDate)
                                        deed = true;

                                    if (deed)
                                        names.Add(fi.FullName);
                                }
                            }

                            foreach (string name in names)
                            {
                                try
                                {
                                    File.Delete(name);
                                    Trace.WriteLine("Delete prev trace log: " + name);
                                }
                                catch
                                {
                                }
                            }
                        }
                        catch(System.Exception)
                        {
                            // игнорируем FileNotFoundException
                            // Ну и как это тестировать?
                        }
                        finally
                        {
                            IsCleanUP = false;
                        }
                    });
        }

        static string FileName
        {
            get { return Path.Combine(FolderName, DateTime.Now.ToString("yyyy-MM-dd")) + LogExtension; }
        }

        
        static string FolderName
        {
            get
            {
                List<string> parts = new List<string>();

                var shortAssemblyName = Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly().Location);
                var lastFolder = shortAssemblyName.ToLower() + ".logs";

                string ret;
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    ret =
                        Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            lastFolder);
                else
                {
                    var candidates = new[] {"/tmp", "/var/tmp", "/var/log"}.ToList();
                    var home = Environment.GetEnvironmentVariable("HOME");
                    if (!string.IsNullOrEmpty(home))
                        candidates.Add(Path.Combine(home, ".logs"));

                    ret = null;
                    foreach (var candidate in candidates)
                    {
                        try
                        {
                            if (Directory.Exists(candidate))
                            {
                                string ss = Path.Combine(candidate, lastFolder);
                                Directory.CreateDirectory(ss);
                                if (Directory.Exists(ss))
                                {
                                    ret = ss;
                                    break;
                                }
                            }
                        }
                        catch
                        {
                        }
                    }

                    if (ret == null)
                        ret = new DirectoryInfo(Environment.CurrentDirectory).FullName;
                }
                    

                return ret;
            }
        }


    }

    public class DebugTraceLimits
    {
        public static readonly DebugTraceLimits Default = new DebugTraceLimits();
        
        // Сколько файлов хранить
        public readonly int FilesLimit = 1000;

        // Сколько часов хранить логи
        public readonly int HoursLimit = 240;

        // Пороговый размер всех лог-файлов, Mb
        public readonly int TotalSizeLimit = 32;

        // Пороговый размер одного лог-файла, Mb
        public readonly int SingleFileLimit = 16;
    }

    class UniverseTraceListener: TraceListener
    {
        object SyncWrite = new object();
        private string _Folder;

        private string File;
        private DateTime Day;
        private StreamWriter Writer;
        private FileStream Stream;

        public UniverseTraceListener(string folder, string name) : base(name)
        {
            _Folder = folder;
        }

        bool CheckNewFile()
        {
            DateTime now = DateTime.Now;
            DateTime day = now.Date;
            if (File == null || Day != day || (this.Stream != null && this.Stream.Length > 1024L * 1024L * DebugTraceLimits.Default.SingleFileLimit))
            {
                string newFile = GetNewFile(day);
                File = newFile;
                Day = day;

                if (Writer != null)
                    Writer.Dispose();

                if (this.Stream != null)
                    this.Stream.Dispose();

                try
                {
                    this.Stream = new FileStream(Path.Combine(_Folder, newFile), FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                    Writer = new StreamWriter(Stream, Encoding.GetEncoding(1251));
                    Writer.AutoFlush = true;
                }
                catch
                {
                    if (Writer != null)
                        Writer.Dispose();

                    if (Stream != null)
                        this.Stream.Dispose();

                    this.Stream = null;
                    this.Writer = null;
                    File = null;
                }

                return true;
            }

            else
            {
                // продолжаем писать в тот же файл
                return false;
            }
        }

        string GetNewFile(DateTime day)
        {
            string prefix = DayPrefix(day);
            DirectoryInfo di = new DirectoryInfo(_Folder);
            FileInfo[] files = di.GetFiles(prefix + "*");
            List<string> names = new List<string>();
            foreach (FileInfo file in files)
                names.Add(Path.GetFileNameWithoutExtension(file.FullName));

            List<int> indexes = new List<int>();
            foreach (string nm in names)
            {
                string[] arr = nm.Split(LogNameSeparators);
                string rawLastIndex = arr[arr.Length - 1];
                int ii;
                if (int.TryParse(rawLastIndex, out ii))
                    indexes.Add(ii);
            }

            indexes.Sort();
            int nextIndex = indexes.Count == 0 ? 1 : (indexes[indexes.Count - 1] + 1);
            string ret = prefix + "-" + nextIndex.ToString(new string('0', 5)) + DebugTraceListener.LogExtension;
            return ret;
        }

        static string DayPrefix(DateTime day)
        {
            return day.ToString("yyyy'.'MM'.'dd");
        }

        private bool NeedLine = true;
        private readonly char[] LogNameSeparators = new char[] {'-', '.'};

        public override void WriteLine(string message)
        {
            try
            {
                bool isNewFile = false;
                lock (SyncWrite)
                {
                    isNewFile = CheckNewFile();

                    if (Writer != null)
                        Writer.WriteLine(TryGetPrefix() + message);
                }

                NeedLine = true;
                if (isNewFile)
                    DebugTraceListener.CleanUp();
            }
            catch
            {
            }
        }

        public override void Write(string message)
        {
            try
            {
                if (message.EndsWith(Environment.NewLine))
                {
                    if (message.Length > Environment.NewLine.Length)
                    {
                        lock (SyncWrite)
                        {
                            if (Writer != null)
                                Writer.Write(TryGetPrefix() + message);

                            NeedLine = true;
                        }
                    }


                    else
                    {
                        lock (SyncWrite)
                        {
                            if (Writer != null)
                                Writer.Write(TryGetPrefix() + message);
                        }
                    }
                }
            }
            catch
            {
            }
        }

        string TryGetPrefix()
        {
            if (NeedLine)
            {
                int id = Thread.CurrentThread.ManagedThreadId;
                int part1 = id%10;
                int part2 = (id/10)%26;
                int part3 = id/260;
                string threadId = ((char) (part2 + 65)).ToString() + (part3*10 + part1).ToString("0");
                return string.Format("{0:yyyy'.'MM'.'dd HH':'mm':'ss'.'ff}  {1}  ", DateTime.Now, threadId);
            }

            else
                return string.Empty;
        }


    }
}
