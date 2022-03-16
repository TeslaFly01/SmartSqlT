using System;
using System.Diagnostics;
using System.IO;

namespace SmartCode.Framework.Util
{
	public class InternalTraceWriter : TextWriter
	{
        StreamWriter writer;

        static string logDirectory;
        public static string LogDirectory
        {
            get
            {
                if (logDirectory == null)
                {
                    logDirectory = Path.Combine(Environment.CurrentDirectory, "Logs");
                    if (!Directory.Exists(logDirectory))
                        Directory.CreateDirectory(logDirectory);
                }
                return logDirectory;
            }
        }

		public InternalTraceWriter(string logName)
		{
            //int pId = Process.GetCurrentProcess().Id;
            //string domainName = AppDomain.CurrentDomain.FriendlyName;

            string fileName = logName.Replace("%p", DateTime.Now.ToString("yyyy-MM-dd"));
            string logPath = Path.Combine(LogDirectory, fileName);
            this.writer = new StreamWriter(logPath, true);
            this.writer.AutoFlush = true;
		}

        public override System.Text.Encoding Encoding
        {
            get { return writer.Encoding; }
        }

        public override void Write(char value)
        {
            writer.Write(value);
        }

        public override void Write(string value)
        {
            base.Write(value);
        }

        public override void WriteLine(string value)
        {
            writer.WriteLine(value);
        }

        public override void Close()
        {
            if (writer != null)
            {
                writer.Flush();
                writer.Close();
                writer = null;
            }
        }

        public override void Flush()
        {
            if ( writer != null )
                writer.Flush();
        }
	}
}
