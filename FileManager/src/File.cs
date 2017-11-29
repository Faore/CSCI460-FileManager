using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace FileManager
{

    public class File
    {
        /* File local variables */
        public string Filename;
        public byte[] FileData;

        public int Filesize => Convert.ToInt32(FileData.Length);

        public ushort RequiredBlocks
        {
            get
            {

                double m1 = (double) this.Filesize / (double) 1024;
                ushort m2 = (ushort) Math.Ceiling(m1);
                return m2;
            }
        }

        /* Constructor */
        public File(string filenameIn, string data)
        {
            Filename = filenameIn;
            FileData = Encoding.ASCII.GetBytes(data);
        }
    }
}
