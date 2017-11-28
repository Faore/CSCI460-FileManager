using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace FileManager
{
    public class FileSystem
    {
        private readonly VirtualDisk _disk;
        
        public FileSystem(VirtualDisk disk)
        {
            _disk = disk;
        }

        private DirectoryTable GetRoot()
        {
            return ParseTableAtBlock(0);
        }

        public DirectoryTable GetDirectoryContents(string path)
        {
            if (path == "/")
            {
                return GetRoot();
            }
            var p1 = path.Split('/');
            var current = GetRoot();
            var last = current;
            for (var i = 1; i < p1.Length; i++)
            {
                for (var j = 0; j < current.Rows.Count; j++)
                {
                    if ((current.Rows[j]).GetString() == p1[i])
                    {
                        current = ParseTableAtBlock(current.Rows[j].BlockStart);
                    }
                }
                if (last == current)
                {
                    var p2 = "";
                    for (var k = 1; k <= i; k++)
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

        public void GetFile()
        {
            
        }

        public File GetFile(string pathname)
        {
            /* separate pathname into path and name */
            var splitPathname =  pathname.Split('/');
            var filename = splitPathname[splitPathname.Length];    /* filename is the last element of the pathname */
            var path = "";

            for (var i = 0; i < splitPathname.Length - 1; i++) {
                path += splitPathname[i];
            }

            /* search the given path for file */
            var correctPath = 0;
            ushort blockLocation = 0;
            var x = GetDirectoryContents(path);
            /* if filename w/in x: */
            int j;
            for (j = 0; j < x.Rows.Count; j++) {
                var row = x.Rows[j];
            /* if current row describes the location for filename, correct_path = 1 */
                if (row.GetString() != filename) continue;
                blockLocation = row.BlockStart;
                correctPath = 1;
                break;
            }

            /* check to see if the file is in the given path */
            if (correctPath == 1) { /* if the file is in the given path */
                if (blockLocation == 0) {
                    /* the impossible has happened */
                    throw new Exception("ya done fuckd up");
                }
                var encodedFile = _disk.ReadBlock(blockLocation);
                var decodedFileCharacters = new char[encodedFile.Length];
                /* convert encoded_file into the corresponding File object named "output" */
                for (var i = 0; i < encodedFile.Length; i++) {
                    decodedFileCharacters[i] = (char) encodedFile[i];
                }
                var outputString = new string(decodedFileCharacters);
                var outputFile = new File(filename, System.Text.Encoding.Default.GetString(encodedFile));
                return outputFile;
            }
            else {                  /* o/w bad arguments */
                throw new Exception("ya done fuckd up");
            }
        }

        private DirectoryTable GetParentPath(string path)
        {
            var splitPath = path.Split('/');
            var superParentPath = "";
            DirectoryTable superParent;
            for (var k = 1; k < splitPath.Length - 1; k++)
            {
                superParentPath += "/";
                superParentPath += splitPath[k];
            }
            if (superParentPath == string.Empty)
            {
                superParent = GetRoot();
                return superParent;
            }
            else
            {
                superParent = GetDirectoryContents(superParentPath);
                return superParent;
            }
        }

        public void CreateDirectory(string name, string path)
        {
            var parentDirectory = GetDirectoryContents(path);
            parentDirectory.InsertRow(new DirectoryRow(name, FindFirstFreeBlockPair(), 0, 0));
            var splitPath = path.Split('/');
            var superParent = GetParentPath(path);
            foreach (var t in superParent.Rows)
            {
                if (t.GetString() != splitPath[splitPath.Length - 1]) continue;
                var bytes = parentDirectory.ToBytes();
                _disk.WriteBlock(t.BlockStart, bytes.Take(bytes.Length/2).ToArray());
                _disk.WriteBlock(t.BlockStart, bytes.Skip(bytes.Length/2).ToArray());
                return;
            }
            throw new Exception("Something is horribly wrong with the file system.");
        }

        public ushort FindFirstFreeBlock()
        {
            var free = GetFreeBlocks();
            for (ushort i = 0; i < free.Length; i++)
            {
                if (free[i])
                {
                    return i;
                }
            }
            throw new Exception("Virtual disk is full.");
        }

        private ushort FindFirstFreeBlockPair()
        {
            var free = GetFreeBlocks();
            var lastFree = false;
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

        private bool[] GetFreeBlocks()
        {
            var free = new bool[ushort.MaxValue];
            for (ushort i = 0; i < ushort.MaxValue; i++)
            {
                free[i] = true;
            }
            var toProcessStack = new Stack<DirectoryTable>();
            toProcessStack.Push(GetRoot());
            while (toProcessStack.Count > 0)
            {
                var current = toProcessStack.Pop();
                foreach (var row in current.Rows)
                {
                    if (row.IsFile)
                    {
                        free[row.BlockStart] = false;
                    }
                    else
                    {
                        free[row.BlockStart] = false;
                        free[row.BlockStart + 1] = false;
                        toProcessStack.Push(ParseTableAtBlock(row.BlockStart));
                    }
                }
            }
            return free;
        }

        /* Doesn't actually create the File object, just finds the free memory in the virtual disk and inserts it there */
        public void CreateFile(string path, File file)
        {
          //  DirectoryTable table_to_update = getDirectoryContents(path);
          //  DirectoryRow row_to_insert = new DirectoryRow(file.filename)
          //  table_to_update.
            return;
        }

        private DirectoryTable ParseTableAtBlock(ushort block)
        {
            var read1 = _disk.ReadBlock(block);
            var read2 = _disk.ReadBlock((ushort) (block + 1));
           
            return DirectoryTable.CreateFromBytes(read1.Concat(read2).ToArray());
        }
    }
}
