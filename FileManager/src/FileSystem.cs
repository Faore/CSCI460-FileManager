using System;
using System.Net;

namespace FileManager
{
    public class FileSystem
    {
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
                    if (((DirectoryRow)current.Rows[j]).getString() == p1[i])
                    {
                        current = (DirectoryTable)current.Rows[j];
                    }
                }
                if (last == current)
                {
                    throw new Exception("Path not found, ");
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
    }
}