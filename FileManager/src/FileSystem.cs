using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;

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

        private List<DirectoryRow> SortTableEntries(List<DirectoryRow> entries)
        {
            var sortedEntries = new List<DirectoryRow>();
            ushort lookForNext = 0;
            foreach (var entry in entries)
            {
                if (entry.Next != 0) continue;
                //This is the last entry (without a next);
                sortedEntries.Add(entry);
                entries.Remove(entry);
                lookForNext = entry.BlockStart;
                break;
            }
            if (sortedEntries.Count == 0)
            {
                throw new Exception("Could not find final file entry. File System is corrupted.");
            }
            while (entries.Count > 0)
            {
                foreach (var entry in entries)
                {
                    if (entry.Next != lookForNext) continue;
                    sortedEntries.Add(entry);
                    entries.Remove(entry);
                    lookForNext = entry.BlockStart;
                    break;
                }
                throw new Exception("Parent table entry not found. File System is corrupted.");
            }
            sortedEntries.Reverse();
            return sortedEntries;
        }

        private byte[] CollapseBlockSeperatedList(List<byte[]> list)
        {
            var bytes = new byte[0];
            return list.Aggregate(bytes, (current, b) => current.Concat(b).ToArray());
        }

        public File GetFile(string pathname)
        {
            /* separate pathname into path and name */
            var splitPathname =  pathname.Split('/');
            var filename = splitPathname[splitPathname.Length - 1];    /* filename is the last element of the pathname */
            var path = "";
            DirectoryTable x;
            if (splitPathname.Length == 2)
            {
                x = GetRoot();
            }
            else
            {
                for (var i = 1; i < splitPathname.Length - 1; i++) {
                    path += $"/{splitPathname[i]}";
                }
                x = GetDirectoryContents(path);
            }
            
            var tableEntries = new List<DirectoryRow>();
            tableEntries = SortTableEntries(tableEntries);
            // Build the file.
            var blockSeperatedContents = new List<byte[]>();
            foreach (var entry in tableEntries)
            {
                var remainingSizeInEntry = entry.Size; 
                for (ushort i = 0; i < entry.ReferencedBlockCount; i++)
                {
                    if (remainingSizeInEntry >= 1024)
                    {
                        blockSeperatedContents.Add(_disk.ReadBlock((ushort) (entry.BlockStart + i)));
                        remainingSizeInEntry -= 1024;
                    }
                    else
                    {
                        var block = _disk.ReadBlock((ushort) (entry.BlockStart + i));
                        blockSeperatedContents.Add(block.Take(remainingSizeInEntry).ToArray());
                    }
                }
            }

            var fileBytes = CollapseBlockSeperatedList(blockSeperatedContents);
            var outputFile = new File(filename, System.Text.Encoding.ASCII.GetString(fileBytes));
            return outputFile;
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
            var freeBlocks = FindFirstFreeBlockPair();
            parentDirectory.InsertRow(new DirectoryRow(name, freeBlocks.Item1, 0, 0));
            var splitPath = path.Split('/');
            var superParent = GetParentPath(path);
            var targetDirectory = new DirectoryTable();
            _disk.WriteBlock(freeBlocks.Item1, targetDirectory.ToBytes().Take(targetDirectory.ToBytes().Length/2).ToArray());
            _disk.WriteBlock(freeBlocks.Item1, targetDirectory.ToBytes().Skip(targetDirectory.ToBytes().Length/2).ToArray());
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

        private Tuple<ushort, ushort> FindContiguousFreeBlocks(ushort n)
        {
            return FindContiguousFreeBlocks(n, GetFreeBlocks());
        }

        private Tuple<ushort, ushort> FindContiguousFreeBlocks(ushort n, bool[] freeBlocks)
        {
            var nFree = new bool[n];
            for (ushort i = 0; i < (ushort) freeBlocks.Length; i++)
            {
                Array.Copy(nFree, 1, nFree, 0, nFree.Length - 1);
                if (freeBlocks[i])
                {
                    nFree[0] = true;
                }
                else
                {
                    nFree[0] = false;
                }
                var solution = nFree.All(b => b);
                if (solution)
                {
                    return new Tuple<ushort, ushort>((ushort) (i - n), i);
                }
            }
            return null;
        }
        
        private Tuple<ushort, ushort>[] FindNonContiguousFreeBlocks(ushort n)
        {
            return FindNonContiguousFreeBlocks(n, GetFreeBlocks());
        }

        private Tuple<ushort, ushort>[] FindNonContiguousFreeBlocks(ushort n, bool[] freeBlocks)
        {
            var list = new List<Tuple<ushort, ushort>>();
            var neededBlocks = n;
            while (neededBlocks > 0)
            {
                var found = false;
                for (int i = neededBlocks; i > 0; i--)
                {
                    var blocks = FindContiguousFreeBlocks(neededBlocks, freeBlocks);
                    if (blocks == null) continue;
                    neededBlocks -= (ushort) i;
                    list.Add(blocks);
                    for (int j = blocks.Item1; j <= blocks.Item2; j++)
                    {
                        freeBlocks[j] = false;
                    }
                    found = true;
                    break;
                }
                if (!found)
                {
                    //Theres not enough disk space for the required number of blocks.
                    throw new Exception("Virtual disk is full.");
                }
            }
            return list.ToArray();
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

        private Tuple<ushort, ushort> FindFirstFreeBlockPair()
        {
            var blocks = FindContiguousFreeBlocks(2, GetFreeBlocks());
            if (blocks == null)
            {
                throw new Exception("Virtual disk is full.");                
            }
            return blocks;
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
                        for (ushort i = 0; i < row.ReferencedBlockCount; i++)
                        {
                            free[row.BlockStart + i] = false;
                        }
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