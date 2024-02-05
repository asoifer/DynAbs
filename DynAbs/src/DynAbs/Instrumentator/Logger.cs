using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynAbs
{
    public static class Logger
    {
        public static bool ConsoleEnabled = true;

        public static void Error(string value)
        {
            try
            {
                if (ConsoleEnabled)
                { 
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(value.PadRight(Console.WindowWidth - 1));
                    Console.ResetColor();
                }
            }
            catch (Exception)
            {
                ConsoleEnabled = false;
            }
        }

        public static void Warning(string value)
        {
            try
            {
                if (ConsoleEnabled)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(value.PadRight(Console.WindowWidth - 1));
                    Console.ResetColor();
                }
            }
            catch (Exception)
            {
                ConsoleEnabled = false;
            }
        }

        public static void Notice(string value)
        {
            try
            {
                if (ConsoleEnabled)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(value.PadRight(Console.WindowWidth - 1));
                    Console.ResetColor();
                }
            }
            catch (Exception)
            {
                ConsoleEnabled = false;
            }
        }

        public static void Info(string value)
        {
            try
            {
                if (ConsoleEnabled)
                {
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(value.PadRight(Console.WindowWidth - 1));
                    Console.ResetColor();
                }
            }
            catch (Exception)
            {
                ConsoleEnabled = false;
            }
        }

        public static void Debug(string value)
        {
            try
            {
                if (ConsoleEnabled)
                {
                    Console.WriteLine(value.PadRight(Console.WindowWidth - 1));
                }
            }
            catch (Exception)
            {
                ConsoleEnabled = false;
            }
        }
    }
}
