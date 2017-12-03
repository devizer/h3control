using Universe.TinyGZip;

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
        public override void TestFixtureSetUp()
        {
            Console.WriteLine("Process: '" + Process.GetCurrentProcess().ProcessName + "'");
        }

        [Test]
        public void T01_GZip_Doesnt_Fail()
        {
            if (!CrossInfo.IsSystemGZipSupported)
            {
                Trace.WriteLine("System GZip isnt supported at the runtime");
                return;
            }

            try
            {
                Trace.WriteLine("Is System GZip supported: " + GZipExtentions.IsSystemGZipSupported);
                byte[] plain = {(byte) 6, 5, 4, 3, 2, 1};
                MemoryStream memGZipped = new MemoryStream();
                using (Stream gzip = GZipExtentions.CreateCompressor(memGZipped, true))
                {
                    gzip.Write(plain, 0, plain.Length);
                }

                Trace.WriteLine("Compressed {6,5,4,3,2,1} length is " + memGZipped.Length);

                memGZipped.Position = 0;
                MemoryStream copy = new MemoryStream();
                using (Stream ungzip = GZipExtentions.CreateDecompressor(memGZipped, true))
                {
                    ungzip.CopyTo(copy);
                }

                Assert.AreEqual(Convert.ToBase64String(plain), Convert.ToBase64String(copy.ToArray()));
            }
            catch (NotSupportedException)
            {
                Trace.WriteLine("GZip streams are not supported on this mono configuration. Please use SharpZipLib and DotNetZip");
            }


        }
    }
}