
namespace H3Control.Tests
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;

    using Links;

    using NUnit.Framework;

    using Universe;

    [TestFixture]
    public class F0_Test_HttpClientExtentions : BaseTest
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
        }

        [Test]
        public void T01_Get_GitHub_Returns_200()
        {
            using (HttpClient client = new HttpClient())
            {
                var t = client.GetAsStringAsync("https://github.com");
                string result = t.Result;
                Assert.IsTrue(result.IndexOf("google", StringComparison.InvariantCulture) >= 0);
            }
        }

        [Test]
        public void T02_Get_Nyan_Schema_Fails_into_ArgumentException()
        {
            using (HttpClient client = new HttpClient())
            {
                var nyanUrl = "nyan://google.com";
                try
                {
                    var task = client.GetAsStringAsync(nyanUrl);
                    string result = task.Result;
                    Assert.Fail();
                }
                catch (Exception ex)
                {
                    var exInfo = ex.Get();
                    Trace.WriteLine("Expected exception has been catched:" + Environment.NewLine + exInfo);
                    Assert.IsTrue(exInfo.IndexOf(nyanUrl) >= 0);
                }
            }
        }

        [Test]
        public void T03_Get_404_Schema_Fails_into_404()
        {
            using (HttpClient client = new HttpClient())
            {
                var url404 = "https://google.com/404-billiard";
                try
                {
                    var task = client.GetAsStringAsync(url404);
                    string result = task.Result;
                    Assert.Fail();
                }
                catch (Exception ex)
                {
                    var exInfo = ex.Get();
                    Trace.WriteLine("Exception has been catched:" + Environment.NewLine + exInfo);
                    Assert.IsTrue(exInfo.IndexOf(url404) >= 0);
                }
            }
        }

    }
}
