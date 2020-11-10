using System;
using System.IO;
using System.Text;

namespace AmazonDL.UtilLib
{
    public static class Logger
    {
        static public string KeylogPath { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "keylog.log");
        static public bool IsDebug { get; set; } = false;
        static public bool IsRequests { get; set; } = false;
        static public bool IsTrack { get; set; } = false;
        static public bool IsVerbose { get; set; } = false;

        public static void Info(string message)
        {
            Log("info", message);
        }

        public static void Debug(string message)
        {
            if (!IsDebug)
                return;
            Log("debug", message);
        }
        public static void Request(string message)
        {
            if (!IsRequests)
                return;
            Log("req", message);
        }
        public static void Verbose(string message)
        {
            if (!IsVerbose && !IsDebug)
                return;
            Log("verb", message);
        }
        public static void Track(string message)
        {
            if (!IsTrack)
                return;
            Log("track", message);
        }
        public static void Warn(string message)
        {
            ConsoleColor prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Log("warn", message);
            Console.ForegroundColor = prev;
        }

        public static void Error(string message)
        {
            ConsoleColor prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Log("error", message);
            Console.ForegroundColor = prev;
        }

        public static void Key(string message, string name)
        {
            try
            {
                if (!File.Exists(KeylogPath))
                {
                    File.Create(KeylogPath).Dispose();
                }

                using StreamWriter sw = File.AppendText(KeylogPath);
                string time = DateTime.Now.ToString("dd-MM-yy HH:mm:ss.ffff");
                sw.WriteLine($"{time} - {message} - {name}");
            }
            catch (Exception)
            {
            }

            ConsoleColor prev = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Log("key", message);
            Console.ForegroundColor = prev;
        }

        static void Log(string level, string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss.ff");
            string text = $"{time} : {message}";

            Console.WriteLine(text);
        }
    }
}
