﻿using System;
using System.Collections.Generic;

namespace FileManager
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var fileManager = new FileManager(new FileSystem());
            fileManager.startConsole();
        }
    }
}