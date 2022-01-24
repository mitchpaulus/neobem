using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using NUnit.Framework;
using src;

namespace test
{
    [TestFixture]
    public class BclTests
    {

        [Test]
        public void TestBclRead()
        {
            string uuid = "908accf0-5ea7-0130-b19d-14109fdf0b37";
            Bcl bcl = new Bcl();
            var response = bcl.GetByUUID(uuid);
            Console.Write(response);
        }
    }
}
