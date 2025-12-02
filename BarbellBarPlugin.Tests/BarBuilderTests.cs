using BarbellBarPlugin.Kompas;
using BarbellBarPlugin.Model;
using BarbellBarPlugin.Tests;
using BarbellBarPlugin.Kompas;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BarbellBarPlugin.Tests
{
    [TestClass]
    public class BarBuilderTests
    {
        [TestMethod]
        public void Build_CallsAttachAndCreateDocument()
        {
            var fake = new FakeKompasWrapper();
            var builder = new BarBuilder(fake);

            var p = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 1200,
                separatorDiameter: 40,
                sleeveLength: 350);

            builder.Build(p);

            Assert.IsTrue(fake.AttachCalled, "Ожидался вызов AttachOrRunCAD.");
            Assert.IsTrue(fake.CreateDocCalled, "Ожидался вызов CreateDocument3D.");
        }

        [TestMethod]
        public void Build_CreatesFiveSegments_InCorrectOrder()
        {
            var fake = new FakeKompasWrapper();
            var builder = new BarBuilder(fake);

            var p = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 1200,
                separatorDiameter: 40,
                sleeveLength: 350);

            builder.Build(p);

            var segments = fake.Segments;

            Assert.AreEqual(5, segments.Count, "Должно быть 5 сегментов.");

            Assert.AreEqual("LeftSleeve", segments[0].Name);
            Assert.AreEqual("LeftSeparator", segments[1].Name);
            Assert.AreEqual("Handle", segments[2].Name);
            Assert.AreEqual("RightSeparator", segments[3].Name);
            Assert.AreEqual("RightSleeve", segments[4].Name);
        }

        [TestMethod]
        public void Build_SegmentsHaveCorrectCoordinatesAndDiameters()
        {
            var fake = new FakeKompasWrapper();
            var builder = new BarBuilder(fake);

            var p = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 1200,
                separatorDiameter: 40,
                sleeveLength: 350);

            builder.Build(p);

            var s = fake.Segments;
            Assert.AreEqual(5, s.Count);

            Assert.AreEqual(0.0, s[0].StartX, 1e-6);
            Assert.AreEqual(350.0, s[0].EndX, 1e-6);

            Assert.AreEqual(350.0, s[1].StartX, 1e-6);
            Assert.AreEqual(400.0, s[1].EndX, 1e-6);

            Assert.AreEqual(400.0, s[2].StartX, 1e-6);
            Assert.AreEqual(1600.0, s[2].EndX, 1e-6);

            Assert.AreEqual(1600.0, s[3].StartX, 1e-6);
            Assert.AreEqual(1650.0, s[3].EndX, 1e-6);

            Assert.AreEqual(1650.0, s[4].StartX, 1e-6);
            Assert.AreEqual(2000.0, s[4].EndX, 1e-6);

            double expectedHandleDiameter =
                System.Math.Min(p.SleeveDiameter, p.SeparatorDiameter) - 3.0;

            Assert.AreEqual(p.SleeveDiameter, s[0].Diameter, 1e-6);
            Assert.AreEqual(p.SeparatorDiameter, s[1].Diameter, 1e-6);
            Assert.AreEqual(expectedHandleDiameter, s[2].Diameter, 1e-6);
            Assert.AreEqual(p.SeparatorDiameter, s[3].Diameter, 1e-6);
            Assert.AreEqual(p.SleeveDiameter, s[4].Diameter, 1e-6);
        }
    }
}
