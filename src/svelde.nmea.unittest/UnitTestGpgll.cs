using Microsoft.VisualStudio.TestTools.UnitTesting;
using svelde.nmea.parser;

namespace svelde.nmea.unittest;

[TestClass]
public sealed class UnitTestGpgll

{
  [TestMethod]
  public void TestMethodParse()
  {
    // ARRANGE

    var m = "$GPGLL,4513.13795,N,01859.19702,E,143717.00,A,A*6C";

    // ACT

    var n = new GpgllMessage();

    n.Parse(m);

    // ASSERT

    Assert.AreEqual("45.21896583", n.Latitude.ToString());
    Assert.AreEqual("18.98661700", n.Longitude.ToString());
    Assert.AreEqual("143717.00", n.FixTaken);
    Assert.AreEqual("A", n.DataValid);
    Assert.AreEqual("Autonomous", n.ModeIndicator.Mode);
  }
}
