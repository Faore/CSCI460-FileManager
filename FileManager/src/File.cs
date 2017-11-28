using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace FileManager
{

    public class File
    {
        /* File local variables */
        int filesize;
        string filename;
        byte[] file_data;


        /* Constructor */
        public File(string filename_in, string data)
        {
            filename = filename_in;
            file_data = data.ToCharArray();
            filesize = (int) file_data.length;
        }


        /* File functions */
        public int required_num_blocks()
        {
            return ( (int) Math.ceiling(filesize / 1024));
        }
    }
}
