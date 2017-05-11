using Cisco.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Cisco.UtilitiesTests
{
    [TestClass]
    public class ClsPhoneTestVB
    {
        [TestMethod]
        public void ParseTest2()
        {
            const string message = @"NOTIFY sip:cti@pbx5.belcentrale.nl SIP/2.0 Via: SIP/2.0/UDP 10.0.0.164:5060;branch=z9hG4bK-d76b774e From: ""CS / webworks"" <sip:0616*004@pbx5.belcentrale.nl>;tag=ef768b8611d9b415o0 To: <sip:cti@pbx5.belcentrale.nl> Call-ID: b300b412-f97e79cd@10.0.0.164 CSeq: 48184 NOTIFY Max-Forwards: 70 Event: x-spa-cti User-Agent: Linksys/SPA942-6.1.5(a) P-Station-Name: Robert ;mac=000e08d2ace0 Content-Length: 76 Content-Type: application/x-spa-status <spa-status> <call id=""1"" ref=""135"" ext=""1"" state=""idle""/> </spa-status> ";

            var actual = ClsPhone.ProcessInboundPhoneMessage(message);

            Trace.WriteLine(string.Format("Phone status : {0}", actual));

            Assert.IsNotNull(actual);
            Assert.AreEqual(actual.Status, ClsPhone.EPhoneStatus.Answering);
            Assert.AreEqual(actual.Id, 1);

        }
    }
}
