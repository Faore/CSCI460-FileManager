using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
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

        private DirectoryTable GetSuperParentFromPath(string path)
        {
            var splitPath = path.Split('/');
            var superParentPath = "";
            DirectoryTable superParent;
            if (splitPath.Length == 2)
            {
                return null;
            }
            for (var k = 1; k < splitPath.Length - 2; k++)
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

        private DirectoryTable GetParentFromPath(string path)
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
            var superParent = GetParentFromPath(path);
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

        public void DeleteObject(string filePath)
        {
            var parent = GetParentFromPath(filePath);
            var superParent = GetSuperParentFromPath(filePath);
            var rewriteBlock = ushort.MaxValue;
            var parentName = filePath.Split('/')[filePath.Split('/').Length - 2];
            var oldName = filePath.Split('/')[filePath.Split('/').Length - 1];
            var newTableRows = new List<DirectoryRow>();
            foreach (var entry in parent.Rows)
            {
                if (entry.GetString() != oldName)
                {
                    newTableRows.Add(entry);
                }
            }
            parent.Rows = newTableRows;
            if (superParent == null)
            {
                rewriteBlock = 0;
            }
            else
            {
                foreach (var row in superParent.Rows)
                {
                    if (row.GetString() != parentName) continue;
                    rewriteBlock = row.BlockStart;
                    break;
                }
            }
            
            if (rewriteBlock == ushort.MaxValue)
            {
                throw new Exception("Could not find directory block to delete. Something is horribly wrong with the file system.");
            }
            _disk.WriteBlock(rewriteBlock, parent.ToBytes().Take(parent.ToBytes().Length/2).ToArray());
            _disk.WriteBlock((ushort) (rewriteBlock + 1), parent.ToBytes().Skip(parent.ToBytes().Length/2).ToArray());
        }

        public void RenameObject(string filePath, string newName)
        {
            var parent = GetParentFromPath(filePath);
            var superParent = GetSuperParentFromPath(filePath);
            var rewriteBlock = ushort.MaxValue;
            var parentName = filePath.Split('/')[filePath.Split('/').Length - 2];
            
            
            var oldName = filePath.Split('/')[filePath.Split('/').Length - 1];
            foreach (var entry in parent.Rows)
            {
                if (entry.GetString() == oldName)
                {
                    entry.SetString(newName);
                }
            }
            if (superParent == null)
            {
                rewriteBlock = 0;
            }
            else
            {
                foreach (var row in superParent.Rows)
                {
                    if (row.GetString() != parentName) continue;
                    rewriteBlock = row.BlockStart;
                    break;
                }
            }
            
            if (rewriteBlock == ushort.MaxValue)
            {
                throw new Exception("Could not find directory block to delete. Something is horribly wrong with the file system.");
            }
            _disk.WriteBlock(rewriteBlock, parent.ToBytes().Take(parent.ToBytes().Length/2).ToArray());
            _disk.WriteBlock((ushort) (rewriteBlock + 1), parent.ToBytes().Skip(parent.ToBytes().Length/2).ToArray());
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
            /* Variables important to CreateFile */
            DirectoryTable table_to_update = GetDirectoryContents(path);                /* - the current table we want to insert the row into.*/
            Tuple[] blocks_to_write = FindNonContiguousFreeBlocks(file.RequiredBlocks); /* - the array of tuples of blocks we want to write to */

            /* find the free blocks corresponding to path */
            int i;
            int current_filedata_location = 0;
            for (i = 0; i < blocks_to_write.Length; i++) { /* iterate over every tuple, which gives the noncontiguous blocks to write to */
                /* write to the blocks between (including) Item1 and Item2 */
                int j;
                for (j = blocks_to_write[i].Item1; j <= blocks_to_write[i].Item2; j++) { /* starting at tuple.left, and going to (including) tuple.right: */
                    byte[] buffer = new byte[1024];
                    int k;
                    for (k = 0; k < 1024; k++) { /* keep filling the buffer with filedata until you reach 1024 bytes */
                        if (current_filedata_location < file.FileData.Length) {
                            buffer[k] = file.FileData[current_filedata_location];
                            current_filedata_location++;
                        } else { /* FileData doesn't fill up it's last remaining block, trying to index into it --> out of bounds exception */
                            break;
                        }
                    }

                    /* figure out which block the file data will continue at noncontiguously */
                    ushort next;
                    if (j >= blocks_to_write[i].Item2) {
                        next = blocks_to_write[i + 1].Item1;
                    } else {
                        next = j + 1;
                    }

                    /* Insert row into table */
                    DirectoryRow row_to_add = new DirectoryRow(file.Filename, blocks_to_write[i].Item1, (ushort) (k + 1), next);
                    table_to_update.InsertRow(row_to_add);

                    /* Write buffer to the current block j */
                    _disk.WriteBlock((ushort) j, buffer);
                    table_to_update.InsertRow(row_to_add);
                }
            }

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
