using System;
using System.ComponentModel;
using NUnit.Framework;

namespace FileManager
{
    [TestFixture]
    public class TestVirtualDisk
    {   
        [TestCase]
        public void TestReadBlock()
        {
            var block0 = new VirtualDisk().ReadBlock(0);
            Assert.AreEqual(1024, block0.Length);
        }

        [TestCase]
        public void TestReadWriteBlock()
        {
             
            var disk = new VirtualDisk();
            var write = new byte[1024];
            new Random().NextBytes(write);
            disk.WriteBlock(15, write);
            var read = disk.ReadBlock(15);
            for (int i = 0; i < 1024; i++)
            {
                if (read[i] != write[i])
                {
                    Assert.IsTrue(false);
                    return;
                }                
            }
            Assert.IsTrue(true);
        }
    }
    
}