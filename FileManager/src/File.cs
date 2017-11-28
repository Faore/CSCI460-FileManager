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

        public ulong Filesize => Convert.ToUInt64(FileData.Length);

        public ushort RequiredBlocks => Convert.ToUInt16(Math.Ceiling((double) (Filesize / 1024)));

        /* Constructor */
        public File(string filenameIn, string data)
        {
            Filename = filenameIn;
            FileData = Encoding.ASCII.GetBytes(data);
        }
    }
}
