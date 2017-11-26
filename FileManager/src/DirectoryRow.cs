namespace FileManager
{
    public class DirectoryRow
    {
        string name = "";
        private ushort blockStart;
        private ushort size;
        private ushort next;

        public DirectoryRow(string n, ushort b, ushort s, ushort x)
        {
            this.name = n;
            this.blockStart = b;
            this.size = s;
            this.next = x;
        }

        public void createFromBytes(byte[] row)//return type will be DirectoryRow
        {
           
        }

        public void createFromInformation(string itemName, ushort blockStart, ushort size, ushort next)//return type will be DirectoryRow
        {
            
        }

        public void toBytes() //return type will be byte[]
        {
           
        }
    }
}