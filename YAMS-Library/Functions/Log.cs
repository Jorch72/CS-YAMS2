using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YAMS.Functions
{
    static class Log
    {

        public static void Write(string strMessage) { Write(strMessage, LogSource.App, LogLevel.Info, DateTime.Now, 0); }
        public static void Write(string strMessage, LogSource Source) { Write(strMessage, Source, LogLevel.Info, DateTime.Now, 0); }
        public static void Write(string strMessage, LogSource Source, LogLevel Level) { Write(strMessage, Source, Level, DateTime.Now, 0); }
        public static void Write(string strMessage, LogSource Source, LogLevel Level, DateTime LogDate) { Write(strMessage, Source, Level, DateTime.Now, 0); }
        public static void Write(string strMessage, LogSource Source, LogLevel Level, DateTime LogDate, int intServerID)
        {
            Console.WriteLine(strMessage);
            _YAMS_dbDataSetTableAdapters.YAMS_LogTableAdapter ta = new _YAMS_dbDataSetTableAdapters.YAMS_LogTableAdapter();
            ta.Insert(LogDate, Source.ToString(), strMessage, (int)Level, intServerID);
            
        }

        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error
        }

        public enum LogSource
        {
            App,
            Server,
            Web,
            Networking,
            Updater
        }
    }
}
