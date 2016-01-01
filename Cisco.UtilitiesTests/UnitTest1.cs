using System.Diagnostics;
using System.Linq;
using Cisco.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cisco.UtilitiesTests
{
    [TestClass]
    public class NetUtilsTests
    {
        [TestMethod]
        public void GetLocalIpv4AddressesTest()
        {
            //Arange
            //Act
            var actual = NetUtils.GetLocalIpv4Addresses();

            //Assert
            Assert.IsNotNull(actual);
            Assert.IsTrue(actual.Any());

            foreach (var address in actual)
            {
                Trace.WriteLine(address);
            }
        }
    }
}
