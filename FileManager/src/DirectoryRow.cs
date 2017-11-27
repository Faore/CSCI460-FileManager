    using System;
    
    namespace FileManager
    {
        public class DirectoryRow
        {
            char[] name;
            private ushort blockStart;
            private ushort size;
            private ushort next;
    
            public DirectoryRow(string n, ushort b, ushort s, ushort x)
            {
                if(n.Length >58)
                {
                    throw new Exception("File name too long");
                }
                
                char[] n2 = n.ToCharArray();
                char[] n3 = new char[58];
                for (int i = 0; i <n2.Length; i++)
                {
                    n3[i] = n2[i];
                }
                
                this.name = n3;
                this.blockStart = b;
                this.size = s;
                this.next = x;
            }
    
            public static DirectoryRow createFromBytes(byte[] row)//return type will be DirectoryRow
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
                
                return new DirectoryRow(n, b, s, x);
            }
    
            public void toBytes() //return type will be byte[]
            {
               
            }
        }
    }