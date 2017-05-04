using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Discord;


namespace DOBAR.Helper
{
    internal static class Logger
    {
        private static object Locker { get; } = new object();
        public static void Log(LogSeverity logLevel, string msg)
        {
            switch (logLevel)
            {
                case LogSeverity.Critical:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case LogSeverity.Error:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    break;
                case LogSeverity.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogSeverity.Info:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                case LogSeverity.Verbose:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case LogSeverity.Debug:
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    break;
            }

            msg = $"{DateTime.Now}: {msg}";

#if DEBUG
            Console.WriteLine(msg);
#else
            if (loggingType != LoggingType.Debug)
                Console.WriteLine(msg);
#endif
            Console.ResetColor();

            lock (Locker)
            {
                File.AppendAllText("logs.txt", $"({logLevel}) {msg}\n");
            }
        }

        /// <summary>
        /// Logs a message to the console on debug level. Ignored if compiled in release configuration.
        /// </summary>
        /// <param name="msg"></param>
        public static void Debug(string msg)
        {
            Log(LogSeverity.Debug, msg);
        }

        /// <summary>
        /// Logs a message to the console on an information level.
        /// </summary>
        /// <param name="msg"></param>
        public static void Info(string msg)
        {
            Log(LogSeverity.Info, msg);
        }

        /// <summary>
        /// Logs a message to the console on a warning level.
        /// </summary>
        /// <param name="msg"></param>
        public static void Warn(string msg)
        {
            Log(LogSeverity.Warning, msg);
        }

        /// <summary>
        /// Logs a message to the console on a error level.
        /// </summary>
        /// <param name="msg"></param>
        public static void Error(string msg)
        {
            Log(LogSeverity.Error, msg);
        }

        /// <summary>
        /// Logs an exception to the console on the error level.
        /// </summary>
        /// <param name="ex">Exception to log.</param>
        public static void Error(Exception ex)
        {
            Error(ex.Message);
            Error(ex.Source);
            Error(ex.StackTrace);

            if (ex.InnerException != null)
                Error(ex.InnerException);
        }
    }
}
