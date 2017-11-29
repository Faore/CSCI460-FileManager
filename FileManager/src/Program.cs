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
            table.InsertRow(new DirectoryRow("latin",2,0,0));
            table.InsertRow(new DirectoryRow("english",8,0,0));
            
            vd.WriteBlock(0, table.ToBytes().Take(table.ToBytes().Length/2).ToArray());
            vd.WriteBlock(1, table.ToBytes().Skip(table.ToBytes().Length/2).ToArray());
            
            table = new DirectoryTable();
            table.InsertRow(new DirectoryRow("another_folder",4,0,0));
            
            vd.WriteBlock(2, table.ToBytes().Take(table.ToBytes().Length/2).ToArray());
            vd.WriteBlock(3, table.ToBytes().Skip(table.ToBytes().Length/2).ToArray());
            
            table = new DirectoryTable();
            table.InsertRow(new DirectoryRow("an_empty_folder",6,0,0));
            
            vd.WriteBlock(4, table.ToBytes().Take(table.ToBytes().Length/2).ToArray());
            vd.WriteBlock(5, table.ToBytes().Skip(table.ToBytes().Length/2).ToArray());
            
            table = new DirectoryTable();
            
            vd.WriteBlock(6, table.ToBytes().Take(table.ToBytes().Length/2).ToArray());
            vd.WriteBlock(7, table.ToBytes().Skip(table.ToBytes().Length/2).ToArray());
            
            table = new DirectoryTable();
            
            vd.WriteBlock(8, table.ToBytes().Take(table.ToBytes().Length/2).ToArray());
            vd.WriteBlock(9, table.ToBytes().Skip(table.ToBytes().Length/2).ToArray());
            
            fs.CreateFile("/latin", new File("la1", "Lorem ipsum dolor sit amet"));
            fs.CreateFile("/latin", new File("la2", "Nam fermentum quam a magna dignissim sodales. Proin vitae purus congue, ultrices lorem et, efficitur ligula. Nam tempus interdum purus, vitae pretium odio lobortis at. Praesent eu erat sit amet ligula venenatis bibendum. Nam commodo, leo eget ullamcorper aliquet, orci tortor blandit purus, eu maximus lorem eros ut dolor. Integer hendrerit turpis vel ullamcorper lobortis. Nunc maximus ultricies felis, vitae porttitor quam rutrum sed. Phasellus tempus efficitur purus eu suscipit. Etiam sed augue metus. Quisque euismod dignissim arcu. Aliquam semper a sapien ac congue. Aenean tincidunt, nulla aliquet ultricies porta, massa urna egestas nunc, at aliquet magna sapien id ex. Aliquam at bibendum erat. "));
            fs.CreateFile("/english", new File("hello", "Hello World!"));
            fs.CreateFile("/english", new File("goodbye", "Goodbye World!"));
            fs.CreateFile("/english", new File("greetings", "Greetings World!"));
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