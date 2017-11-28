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

        public string getFile(string pathname)
        {
            File output = null;

            /* separate pathname into path and name */
            string[] split_pathname =  pathname.split('/');
            string filename = split_pathname[split_pathname.length];    /* filename is the last element of the pathname */
            string path;
            for (int i = 0; i < split_pathname.length - 1; i++) {
                path += split_pathname[i];
            }

            /* search the given path for file */
            int correct_path = 0;
            ushort block_location;
            DirectoryTable x = getDirectoryContents(path);
            /* if filename w/in x: */
            int j;
            for (j = 0; j < x.Rows.Count; j++) {
                row = x.Rows[j];
            /* if current row describes the location for filename, correct_path = 1 */ 
                if (row.itemName == filename) {
                    block_location = row.blockStart;
                    correct_path = 1;
                    break;
                }
            }

            /* check to see if the file is in the given path */
            if (correct_path == 1) { /* if the file is in the given path */
                byte[] encoded_file = Disk.readBlock(block_location);
                char[] decoded_file_characters = new char[encoded_file.length];
                /* convert encoded_file into the corresponding File object named "output" */
                for (int i = 0; i < encoded_file.length; i++) {
                    decoded_file_characters[i] = (char) encoded_file[i];
                }
                string output_string = new string(decoded_file_characters);

                return output_string;
            }
            else {                  /* o/w bad arguments */
                return "ya done fuckd up";
            }
        }

        public DirectoryTable getParentPath(string path)
        {
            var splitPath = path.Split('/');
            var superParentPath = "";
            DirectoryTable superParent;
            for (int k = 1; k < splitPath.Length - 1; k++)
            {
                superParentPath += "/";
                superParentPath += splitPath[k];
            }
            if (superParentPath == String.Empty)
            {
                superParent = getRoot();
                return superParent;
            }
            else
            {
                superParent = getDirectoryContents(superParentPath);
                return superParent;
            }
        }

        public void createDirectory(string name, string path)
        {
            DirectoryTable parentDirectory = getDirectoryContents(path);
            parentDirectory.InsertRow(new DirectoryRow(name, findFirstFreeBlockPair(), 0, 0));
            var splitPath = path.Split('/');
            var superParent = getParentPath(path);
            for (int i = 0; i < superParent.Rows.Count; i++)
            {
                if (superParent.Rows[i].getString() == splitPath[splitPath.Length - 1])
                {
                    var bytes = parentDirectory.toBytes();
                    Disk.WriteBlock(superParent.Rows[i].blockStart, bytes.Take(bytes.Length/2).ToArray());
                    Disk.WriteBlock(superParent.Rows[i].blockStart, bytes.Skip(bytes.Length/2).ToArray());
                    return;
                }
            }
            throw new Exception("Something is horribly wrong with the file system.");
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
