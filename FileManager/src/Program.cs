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
            
            vd.WriteBlock(0, table.ToBytes().Take(table.ToBytes().Length/2).ToArray());
            vd.WriteBlock(1, table.ToBytes().Skip(table.ToBytes().Length/2).ToArray());
            
            table = new DirectoryTable();
            table.InsertRow(new DirectoryRow("folder2",4,0,0));
            
            vd.WriteBlock(2, table.ToBytes().Take(table.ToBytes().Length/2).ToArray());
            vd.WriteBlock(3, table.ToBytes().Skip(table.ToBytes().Length/2).ToArray());
            
            table = new DirectoryTable();
            table.InsertRow(new DirectoryRow("doom",6,0,0));
            
            vd.WriteBlock(4, table.ToBytes().Take(table.ToBytes().Length/2).ToArray());
            vd.WriteBlock(5, table.ToBytes().Skip(table.ToBytes().Length/2).ToArray());
            
            fs.CreateFile("/folder1", new File("derp", "content"));
            
            var fileManager = new FileManager(fs);
            fileManager.StartConsole();

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