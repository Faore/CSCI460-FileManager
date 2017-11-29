using System;
using System.Collections;
using System.Collections.Generic;

namespace FileManager
{
    public class DirectoryTable
    {
        public List<DirectoryRow> Rows;

        public DirectoryTable()
        {
            Rows = new List<DirectoryRow>();
        }

        public DirectoryRow GetRow(byte i)
        {
            return Rows[i];
        }
    
        public void InsertRow(DirectoryRow row)//creates new directory row in this table
        {
            if (Rows.Count == 32)
            {
                throw new Exception("Table cannot store anymore rows.");
            }
            Rows.Add(row);
        }

        public void DeleteRow(DirectoryRow row)//deletes a row form this table
        {
            Rows.Remove(row);
        }
        
        public static DirectoryTable CreateFromBytes(byte[] data)//creates new directory table
        {
            var table = new DirectoryTable();
            for (var i = 0; i < 32; i++)
            {
                var bytes = new byte[64];
                for (var j = 0; j < 64; j++)
                {
                    bytes[j] = data[i * 64 + j];
                }
                var row = DirectoryRow.CreateFromBytes(bytes);
                var str = row.GetString();
                if (str != string.Empty && row.Name != null)
                {
                    table.InsertRow(row);                    
                } 
            }
            return table;
        }

        public byte[] ToBytes()//returns contents of a table as an array of bytes
        {
            var bytes = new byte[2048];
            for (var i = 0; i < Rows.Count; i++)
            {
                var row = ((DirectoryRow) Rows[i]).ToBytes();
                for (var j = 0; j < 64; j++)
                {
                    bytes[i * 64 + j] = row[j];
                }
            }
            return bytes;
        }
    }
}