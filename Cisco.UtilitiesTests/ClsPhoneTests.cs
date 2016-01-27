using Cisco.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Cisco.UtilitiesTests
{
    [TestClass]
    public class ClsPhoneTests
    {
        /// <summary>
        /// Tests <see cref="ClsPhone.ProcessInboundPhoneMessage"/> by specifying data mentioned in https://github.com/Atrejoe/SPA-Call-Manager-Pro/issues/8
        /// </summary>
        [TestMethod]
        public void ParseTest()
        {
            const string message = @"NOTIFY sip:cti@pbx5.belcentrale.nl SIP/2.0 Via: SIP/2.0/UDP 10.0.0.164:5060;branch=z9hG4bK-edba58c2 From: ""CS / webworks"" <sip:0616*004@pbx5.belcentrale.nl>;tag=ef768b8611d9b415o0 To: <sip:cti@pbx5.belcentrale.nl> Call-ID: 2ec7053e-2a88f2f3@10.0.0.164 CSeq: 13466 NOTIFY Max-Forwards: 70 Event: x-spa-cti User-Agent: Linksys/SPA942-6.1.5(a) P-Station-Name: Robert ;mac=000e08d2ace0 Content-Length: 81 Content-Type: application/x-spa-status <spa-status> <call id=""1"" ref=""129"" ext=""1"" state=""answering""/> </spa-status> ";

            var actual = ClsPhone.ProcessInboundPhoneMessage(message);

            Trace.WriteLine(string.Format("Phone status : {0}", actual));

            Assert.IsNotNull(actual);
            Assert.AreEqual(actual.Status, ClsPhone.EPhoneStatus.Answering);
            Assert.AreEqual(actual.Id, 1);

        }
    }
}
