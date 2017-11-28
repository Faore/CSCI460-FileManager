using System;
using System.Collections.Generic;
using System.Linq;

namespace FileManager
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            //var fileManager = new FileManager(new FileSystem(new VirtualDisk()));
            //fileManager.startConsole();
            
            var vd = new VirtualDisk();
            var fs = new FileSystem(vd);
            
            var table = new DirectoryTable();
            table.InsertRow(new DirectoryRow("folder1",2,0,0));
            
            vd.WriteBlock(0, table.toBytes().Take(table.toBytes().Length/2).ToArray());
            vd.WriteBlock(1, table.toBytes().Skip(table.toBytes().Length/2).ToArray());
            
            table = new DirectoryTable();
            table.InsertRow(new DirectoryRow("folder2",4,0,0));
            
            vd.WriteBlock(2, table.toBytes().Take(table.toBytes().Length/2).ToArray());
            vd.WriteBlock(3, table.toBytes().Skip(table.toBytes().Length/2).ToArray());
            
            table = new DirectoryTable();
            table.InsertRow(new DirectoryRow("doom",6,0,0));
            
            vd.WriteBlock(4, table.toBytes().Take(table.toBytes().Length/2).ToArray());
            vd.WriteBlock(5, table.toBytes().Skip(table.toBytes().Length/2).ToArray());

            fs.getDirectoryContents("/folder1/folder2");
            fs.getDirectoryContents("/folder1");
            fs.getDirectoryContents("/");

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