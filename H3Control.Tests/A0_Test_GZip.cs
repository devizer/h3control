namespace H3Control.Tests
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Compression;

    using NUnit.Framework;

    using Universe;

    [TestFixture]
    public class A0_Test_GZip : BaseTest
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
        }

        [Test]
        public void T01_GZip_Doesnt_Fail()
        {
            byte[] plain = new[] {(byte) 5, (byte) 4, (byte) 3, (byte) 2, (byte) 1};
            MemoryStream mem = new MemoryStream();
            using (GZipStream gzip = new GZipStream(mem, CompressionLevel.Optimal, true))
            {
                gzip.Write(plain, 0, plain.Length);
            }

            Trace.WriteLine("Compressed 5,4,3,2,1 length is " + mem.Length);

            mem.Position = 0;
            MemoryStream copy = new MemoryStream();
            using (GZipStream ungzip = new GZipStream(mem, CompressionMode.Decompress, true))
            {
                ungzip.CopyTo(copy);
            }

            Assert.AreEqual(Convert.ToBase64String(plain), Convert.ToBase64String(copy.ToArray()));

        }
    }
}