using System;
using System.Collections;

namespace FileManager
{
    public class DirectoryTable
    {
        private byte[] BlockData;
        private readonly ArrayList Rows = new ArrayList();

        public DirectoryTable()
        {
            
        }

        public DirectoryRow GetRow(byte i)
        {
            return (DirectoryRow) Rows[i];
        }

        public void InsertRow(DirectoryRow row)
        {
            if (Rows.Count == 32)
            {
                throw new Exception("Table cannot store anymore rows.");
            }
            Rows.Add(row);
        }

        public void DeleteRow(DirectoryRow row)
        {
            Rows.Remove(row);
        }
        
        public static DirectoryTable createFromBytes(byte[] data)//return type will be DirectoryTable
        {
            DirectoryTable table = new DirectoryTable();
            for (int i = 0; i < 32; i++)
            {
                byte[] bytes = new byte[64];
                for (int j = 0; j < 64; j++)
                {
                    bytes[j] = data[i * 64 + j];
                }
                DirectoryRow row = DirectoryRow.createFromBytes(bytes);
                if (row.getString() != string.Empty && row.name != null)
                {
                    table.InsertRow(row);                    
                } 
            }
            return table;
        }

        public byte[] toBytes()
        {
            byte[] bytes = new byte[2048];
            for (int i = 0; i < Rows.Count; i++)
            {
                byte[] row = ((DirectoryRow) Rows[i]).toBytes();
                for (int j = 0; j < 64; j++)
                {
                    bytes[i * 64 + j] = row[j];
                }
            }
            return bytes;
        }
    }
}