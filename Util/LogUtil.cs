﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamicomSimulator.Util
{
    internal class LogUtil
    {
        public static bool ShowTime { get; set; } = true;

        public static void Log(string message)
        {
            if (ShowTime)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
            }
            else
            {
                Console.WriteLine(message);
            }
        }
    }
}