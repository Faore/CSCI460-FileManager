using System;
using System.Collections.Generic;
using System.IO;
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
        
        public DirectoryTable getRoot()
        {
            return parseTableAtBlock(0);
        }

        public DirectoryTable getDirectoryContents(string path)
        {
            if (path == "/")
            {
                return getRoot();
            }
            string[] p1 = path.Split('/');
            DirectoryTable current = getRoot();
            DirectoryTable last = current;
            for (int i = 1; i < p1.Length; i++)
            {
                for (int j = 0; j < current.Rows.Count; j++)
                {
                    if ((current.Rows[j]).getString() == p1[i])
                    {
                        current = parseTableAtBlock(current.Rows[j].blockStart);
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
                last = current;
            }
            return current;
        }

        public void getFile()
        {
            
        }

        public void createDirectory(string name, string path)
        {
            
        }

        public ushort findFirstFreeBlock()
        {
            bool[] free = getFreeBlocks();
            for (ushort i = 0; i < free.Length; i++)
            {
                if (free[i])
                {
                    return i;
                }
            }
            throw new Exception("Virtual disk is full.");
        }
        
        public ushort findFirstFreeBlockPair()
        {
            bool[] free = getFreeBlocks();
            bool lastFree = false;
            for (ushort i = 0; i < free.Length; i++)
            {
                if (free[i])
                {
                    if (lastFree)
                    {
                        return (ushort) (i - 1);
                    }
                    else
                    {
                        lastFree = true;
                    }
                }
                else
                {
                    lastFree = false;
                }
        }
            throw new Exception("Virtual disk is full.");
        }

        public bool[] getFreeBlocks()
        {
            bool[] free = new bool[ushort.MaxValue];
            for (ushort i = 0; i < ushort.MaxValue; i++)
            {
                free[i] = true;
            }
            Stack<DirectoryTable> toProcessStack = new Stack<DirectoryTable>();
            toProcessStack.Push(getRoot());
            while (toProcessStack.Count > 0)
            {
                var current = toProcessStack.Pop();
                foreach (DirectoryRow row in current.Rows)
                {
                    if (row.isFile)
                    {
                        free[row.blockStart] = false;
                    }
                    else
                    {
                        free[row.blockStart] = false;
                        free[row.blockStart + 1] = false;
                        toProcessStack.Push(parseTableAtBlock(row.blockStart));
                    }
                }
            }
            return free;
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