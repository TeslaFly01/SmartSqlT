using System;

namespace SmartCode.Framework.Util
{
    public enum InternalTraceLevel
    {
        Default,
        Off,
        Error,
        Warning,
        Info,
        Verbose
    }
    
	public class InternalTrace
	{
        //private readonly static string TIME_FMT = "HH:mm:ss.fff";
		private static bool initialized;

        private static InternalTraceWriter writer;
        public static InternalTraceWriter Writer
        {
            get { return writer; }
        }

		public static InternalTraceLevel Level;
        public static void Initialize(string logName)
        {
            int lev = (int) new System.Diagnostics.TraceSwitch("Trace", "CodeBuilder internal trace").Level;
            Initialize(logName, (InternalTraceLevel)lev);
        }

        public static void Initialize(string logName, InternalTraceLevel level)
        {
			if (!initialized)
			{
				Level = level;

				if (writer == null && Level > InternalTraceLevel.Off)
				{
					writer = new InternalTraceWriter(logName);
					writer.WriteLine("InternalTrace: Initializing at level " + Level.ToString());
				}

				initialized = true;
			}
        }

        public static void ReInitialize(string logName, InternalTraceLevel level)
        {
            if (initialized){ 
                Close();
                initialized=false; 
            }

            Initialize(logName, level);
        }

        public static void Flush()
        {
            if (writer != null)
                writer.Flush();
        }

        public static void Close()
        {
            if (writer != null)
                writer.Close();

            writer = null;
        }

        public static Logger GetLogger(string name)
		{
			return new Logger( name );
		}

		public static Logger GetLogger( Type type )
		{
			return new Logger( type.FullName );
		}

        public static void Log(InternalTraceLevel level, string message, string category)
        {
            Log(level, message, category, null);
        }

        public static void Log(InternalTraceLevel level, string message, string category, Exception ex)
        {
            Writer.WriteLine("{0} {1,-5} [{2,2}] {3}: {4}",
                DateTime.Now.ToString(),
                level == InternalTraceLevel.Verbose ? "Debug" : level.ToString(),
                System.Threading.Thread.CurrentThread.ManagedThreadId,
                category,
                message);

            if (ex != null)
                Writer.WriteLine(ex.ToString());
        }
    }
}
