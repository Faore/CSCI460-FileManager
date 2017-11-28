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
        int filesize;
        string filename;
        byte[] file_data;


        /* Constructor */
        public File(string filename_in, string data)
        {
            filename = filename_in;
            file_data = Encoding.ASCII.GetBytes(data);
            filesize = (int) file_data.Length;
        }


        /* File functions */
        public int required_num_blocks()
        {
            return ( (int) Math.Ceiling((double) (filesize / 1024)));
        }
    }
}
