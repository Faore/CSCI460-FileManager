    using System;
    using System.Diagnostics.SymbolStore;
    using System.Text;

namespace FileManager
    {
        public class DirectoryRow
        {
            public char[] Name;
            public ushort BlockStart;
            public ushort Size;
            public ushort Next;

            public bool IsFile => Size != 0;

            public ushort ReferencedBlockCount
            {
                get
                {
                    double m1 = (double) this.Size / (double) 1024;
                    ushort m2 = (ushort) Math.Ceiling(m1);
                    return m2;
                }
            }

            public DirectoryRow(string n, ushort b, ushort s, ushort x) // name, block start, size, next
            {
                if(n.Length >58)
                {
                    throw new Exception("File name too long");//cannot fit names strings larger than 58 chars into row name
                }
                
                var n2 = n.ToCharArray();
                var n3 = new char[58];
                for (var i = 0; i <n2.Length; i++)//creates char array from input string n, padded with nulls to be length 58
                {
                    n3[i] = n2[i];
                }
                
                this.Name = n3;
                this.BlockStart = b;
                this.Size = s;
                this.Next = x;
            }
    
            public static DirectoryRow CreateFromBytes(byte[] row)//crate a new directry row based on an input array of bytes
            {
                var n = "";
                ushort b = 0;
                ushort s = 0;
                ushort x = 0;
                for (var i = 0; i < 58; i++)//parse first 58 bits in "row", this corresponds to file's name
                {
                    var character = (char) row[i];
                    n += character;
                }
                b = (ushort) (row[58] + (row[59] << 8));//parse bits 59-60 in "row", this corresponds to file's blockStart
                
                s = (ushort) (row[60] + (row[61] << 8));//parse bits 61-62 in "row", this corresponds to file's size
                
                x = (ushort) (row[62] + (row[63] << 8));//parse bit 63 and 64 in "row", this corresponds to file's next
                
                return new DirectoryRow(n, b, s, x);//create new instance and return it
            }
    
            public byte[] ToBytes() //takes all 4 items in a directory row instance, cast them to bytes and return all as a byte array of length 64
            {
                var row = new byte[64];
                for (var i = 0; i < 58; i++)
                {
                    row[i] = (byte)Name[i];
                }
                var b1 = (byte)(BlockStart & 255);//ushorts cast as 2 bytes, need to esparate before adding to return array
                var b2 = (byte)(BlockStart >> 8);
                row[58] = b1;
                row[59] = b2;
                b1 = (byte)(Size & 255);
                b2 = (byte)(Size >> 8);
                row[60] = b1;
                row[61] = b2;
                b1 = (byte)(Next & 255);
                b2 = (byte)(Next >> 8);
                row[62] = b1;
                row[63] = b2;

                return row;
            }

            public void SetString(string n)
            {
                var n2 = n.ToCharArray();
                var n3 = new char[58];
                for (var i = 0; i <n2.Length; i++)//creates char array from input string n, padded with nulls to be length 58
                {
                    n3[i] = n2[i];
                }
                Name = n3;
            }

            public string GetString()
            {
                //Return the string without any null characters.
                return new string(Name).Trim('\0');
            }
        }
    }
