using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SolarWinds.DLLChecker.Backend.Tests
{
    [TestFixture]
    class VersionTests
    {
        [Test]
        public void Can_Parse_String()
        {
            Version version;
            var result = Version.TryParse("4.0.30319.1", out version);

            Assert.IsTrue(result);
            Assert.AreEqual(4, version.Major);
        }


        [Test]
        public void Can_Compare_Versions()
        {
            Version version1 = new Version("2012.2.21.2391");
            Version version2 = new Version("2012.2.5.1892");

            var result = version1 > version2;

            Assert.IsTrue(result);

        }
    }
}
