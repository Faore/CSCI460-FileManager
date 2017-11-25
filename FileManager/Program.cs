using System;
using System.Collections.Generic;

namespace FileManager
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            // A quick example:
            
            var disk = new VirtualDisk();
            disk.WriteBlock(0,new byte[1024]);

            var write = new byte[1024];
            var random = new Random();
            random.NextBytes(write);
            Console.WriteLine("Writing content (to block 0):");
            for (int i = 0; i < 1024; i++)
            {
                Console.Write("{0:X}", write[i]);                
            }
            disk.WriteBlock(0, write);
            Console.WriteLine("\n\nReading content:");
            var data = disk.ReadBlock(0);
            for (int i = 0; i < 1024; i++)
            {
                Console.Write("{0:X}", data[i]);                
            }
            Console.WriteLine();

            var read = disk.ReadBlock(0);
            for (int i = 0; i < 1024; i++)
            {
                if (read[i] != write[i])
                {
                    Console.WriteLine("Mismatch");
                    return;
                }                
            }
            Console.WriteLine("Match");
        }
    }
}