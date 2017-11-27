    using System;
    using System.Diagnostics.SymbolStore;

namespace FileManager
    {
        public class DirectoryRow
        {
            public char[] name;
            public ushort blockStart;
            public ushort size;
            public ushort next;

            public bool isFile
            {
                get { return size != 0; }
            }

            public DirectoryRow(string n, ushort b, ushort s, ushort x)
            {
                if(n.Length >58)
                {
                    throw new Exception("File name too long");//cannot fit names strings larger than 58 chars into row name
                }
                
                char[] n2 = n.ToCharArray();
                char[] n3 = new char[58];
                for (int i = 0; i <n2.Length; i++)//creates char array from input string n, padded with nulls to be length 58
                {
                    n3[i] = n2[i];
                }
                
                this.name = n3;
                this.blockStart = b;
                this.size = s;
                this.next = x;
            }
    
            public static DirectoryRow createFromBytes(byte[] row)//crate a new directry row based on an input array of bytes
            {
                string n = "";
                ushort b = 0;
                ushort s = 0;
                ushort x = 0;
                for (int i = 0; i < 58; i++)//parse first 58 bits in "row", this corresponds to file's name
                {
                    char character = (char) row[i];
                    n += character;
                }
                b = (ushort) (row[58] + (row[59] << 8));//parse bits 59-60 in "row", this corresponds to file's blockStart
                
                s = (ushort) (row[60] + (row[61] << 8));//parse bits 61-62 in "row", this corresponds to file's size
                
                x = (ushort) (row[62] + (row[63] << 8));//parse bit 63 and 64 in "row", this corresponds to file's next
                
                return new DirectoryRow(n, b, s, x);//create new instance and return it
            }
    
            public byte[] toBytes() //takes all 4 items in a directory row instance, cast them to bytes and return all as a byte array of length 64
            {
                byte[] row = new byte[64];
                for (int i = 0; i < 58; i++)
                {
                    row[i] = (byte)name[i];
                }
                byte b1 = (byte)(blockStart & 255);//ushorts cast as 2 bytes, need to esparate before adding to return array
                byte b2 = (byte)(blockStart >> 8);
                row[58] = b1;
                row[59] = b2;
                b1 = (byte)(size & 255);
                b2 = (byte)(size >> 8);
                row[60] = b1;
                row[61] = b2;
                b1 = (byte)(next & 255);
                b2 = (byte)(next >> 8);
                row[62] = b1;
                row[63] = b2;

                return row;
            }

            public string getString()
            {
                //Return the string without any null characters.
                return name.ToString().Replace("\0", string.Empty);
            }
        }
    }