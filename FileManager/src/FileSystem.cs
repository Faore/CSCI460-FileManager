using System;
using System.Linq;
using System.Net;

namespace FileManager
{
    public class FileSystem
    {
        private VirtualDisk Disk;
        
        public FileSystem(VirtualDisk disk)
        {
            Disk = disk;
        }
        
        public void getRoot()
        {
            
        }

        public void getDirectoryContents(string path)
        {
            string[] p1 = path.Split('/');
            DirectoryTable current = getRoot();
            DirectoryTable last = current;
            for (int i = 1; i < p1.Length; i++)
            {
                for (int j = 0; j < current.Rows.Count; j++)
                {
                    if ((current.Rows[j]).getString() == p1[i])
                    {
                        current = current.Rows[j].blockStart;
                    }
                }
                if (last == current)
                {
                    string p2 = "";
                    for (int k = 1; k <= i; k++)
                    {
                        p2 += "/";
                        p2 += p1[k];
                    }
                    throw new Exception($"Path not found, {p2} does not exist");
                }
            }
            
        }

        public void getFile()
        {
            
        }

        public void createDirectory(string name, string path)
        {
            
        }

        public void createFile()
        {
            
        }

        public DirectoryTable parseTableAtBlock(ushort block)
        {
            var read1 = Disk.ReadBlock(block);
            var read2 = Disk.ReadBlock((ushort) (block + 1));
           
            return DirectoryTable.createFromBytes(read1.Concat(read2).ToArray());
        }
    }
}