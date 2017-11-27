using System;
using System.Collections.Generic;
using System.Linq;

namespace FileManager
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var fileManager = new FileManager(new FileSystem(new VirtualDisk()));
            fileManager.startConsole();

            //Example Test
            /*var table = new DirectoryTable();

            var row = new DirectoryRow("HelloWorld", 1, 2, 3);
            
            table.InsertRow(row);
            var bytes = table.toBytes();
            var bytes2 = row.toBytes();

            var disk = new VirtualDisk();
            
            
            disk.WriteBlock(0, bytes.Take(bytes.Length/2).ToArray());
            disk.WriteBlock(1, bytes.Skip(bytes.Length/2).ToArray());

            var read1 = disk.ReadBlock(0);
            var read2 = disk.ReadBlock(1);
            

            var table2 = DirectoryTable.createFromBytes(read1.Concat(read2).ToArray());

            Console.WriteLine(table2.GetRow(0).size);
            */
        }
    }
}