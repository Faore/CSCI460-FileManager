using System;

namespace FileManager
{
    public class VirtualDisk
    {
        private byte[] diskContents;

        public VirtualDisk()
        {
            this.diskContents = new byte[ushort.MaxValue];
        }

        public byte[] ReadBlock(ushort block)
        {
            var data = new byte[1024];

            for (ushort i = 0; i < 1024; i++)
            {
                data[i] = this.diskContents[block * 1024 + i];
            }
            
            return data;
        }

        public void WriteBlock(ushort block, byte[] data)
        {
            if (data.Length != 1024)
            {
                throw new Exception("Tried to write block of size " + data.Length + ". Each block must be 1024 Bytes/1kB in size.");
            }
            for (ushort i = 0; i < 1024; i++)
            {
                this.diskContents[block * 1024 + i] = data[i];
            }
        }
    }
}