using System;
using BarbellBarPlugin.Kompas;
using BarbellBarPlugin.Model;
using BarbellBarPlugin.Validation;
using NUnit.Framework;

namespace BarbellBarPlugin.Tests
{

    [TestFixture]
    public class ValidationErrorTests
    {
        [Test]
        public void Ctor_Sets_FieldName_And_Message()
        {
            const string fieldName = "LengthSleeve";
            const string message = "Ошибка";

            var error = new ValidationError(fieldName, message);

            Assert.Multiple(() =>
            {
                Assert.That(error.FieldName, Is.EqualTo(fieldName));
                Assert.That(error.Message, Is.EqualTo(message));
            });
        }
    }

    //TODO: XML
    [TestFixture]
    public class BarBuilderTests
    {
        [Test]
        public void Ctor_Throws_WhenWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BarBuilder(null!));
        }

        [Test]
        public void Build_Throws_WhenParametersIsNull()
        {
            var fake = new FakeKompasWrapper();
            var builder = new BarBuilder(fake);

            Assert.Throws<ArgumentNullException>(() => builder.Build(null!));
        }

        [Test]
        public void Build_SetsCurrentParameters()
        {
            var fake = new FakeKompasWrapper();
            var builder = new BarBuilder(fake);

            var parameters = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 1200,
                separatorDiameter: 40,
                sleeveLength: 350);

            builder.Build(parameters);

            Assert.That(builder.CurrentParameters, Is.SameAs(parameters));
        }

        [Test]
        public void Build_CallsAttachAndCreateDocument()
        {
            var fake = new FakeKompasWrapper();
            var builder = new BarBuilder(fake);

            var parameters = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 1200,
                separatorDiameter: 40,
                sleeveLength: 350);

            builder.Build(parameters);

            Assert.Multiple(() =>
            {
                Assert.That(fake.AttachCalled, Is.True);
                Assert.That(fake.CreateDocCalled, Is.True);
            });
        }

        [Test]
        public void Build_CreatesFiveSegments_InCorrectOrder()
        {
            var fake = new FakeKompasWrapper();
            var builder = new BarBuilder(fake);

            var parameters = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 1200,
                separatorDiameter: 40,
                sleeveLength: 350);

            builder.Build(parameters);

            var segments = fake.Segments;

            Assert.That(segments.Count, Is.EqualTo(5));

            Assert.Multiple(() =>
            {
                Assert.That(segments[0].Name, Is.EqualTo("LeftSleeve"));
                Assert.That(segments[1].Name, Is.EqualTo("LeftSeparator"));
                Assert.That(segments[2].Name, Is.EqualTo("Handle"));
                Assert.That(segments[3].Name, Is.EqualTo("RightSeparator"));
                Assert.That(segments[4].Name, Is.EqualTo("RightSleeve"));
            });
        }

        [Test]
        public void Build_SegmentsHaveCorrectCoordinatesAndDiameters()
        {
            var fake = new FakeKompasWrapper();
            var builder = new BarBuilder(fake);

            var parameters = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 1200,
                separatorDiameter: 40,
                sleeveLength: 350);

            builder.Build(parameters);

            var s = fake.Segments;
            Assert.That(s.Count, Is.EqualTo(5));

            Assert.Multiple(() =>
            {
                Assert.That(s[0].StartX, Is.EqualTo(0.0).Within(1e-6));
                Assert.That(s[0].EndX, Is.EqualTo(350.0).Within(1e-6));

                Assert.That(s[1].StartX, Is.EqualTo(350.0).Within(1e-6));
                Assert.That(s[1].EndX, Is.EqualTo(400.0).Within(1e-6));

                Assert.That(s[2].StartX, Is.EqualTo(400.0).Within(1e-6));
                Assert.That(s[2].EndX, Is.EqualTo(1600.0).Within(1e-6));

                Assert.That(s[3].StartX, Is.EqualTo(1600.0).Within(1e-6));
                Assert.That(s[3].EndX, Is.EqualTo(1650.0).Within(1e-6));

                Assert.That(s[4].StartX, Is.EqualTo(1650.0).Within(1e-6));
                Assert.That(s[4].EndX, Is.EqualTo(2000.0).Within(1e-6));
            });

            double expectedHandleDiameter =
                Math.Min(parameters.SleeveDiameter, parameters.SeparatorDiameter) - 3.0;

            Assert.Multiple(() =>
            {
                Assert.That(s[0].Diameter, Is.EqualTo(parameters.SleeveDiameter).Within(1e-6));
                Assert.That(s[1].Diameter, Is.EqualTo(parameters.SeparatorDiameter).Within(1e-6));
                Assert.That(s[2].Diameter, Is.EqualTo(expectedHandleDiameter).Within(1e-6));
                Assert.That(s[3].Diameter, Is.EqualTo(parameters.SeparatorDiameter).Within(1e-6));
                Assert.That(s[4].Diameter, Is.EqualTo(parameters.SleeveDiameter).Within(1e-6));
            });
        }

        [Test]
        public void Build_UsesFallbackHandleDiameter_WhenComputedDiameterIsNonPositive()
        {
            var fake = new FakeKompasWrapper();
            var builder = new BarBuilder(fake);

            var parameters = new BarParameters(
                sleeveDiameter: 1,
                separatorLength: 50,
                handleLength: 1200,
                separatorDiameter: 1,
                sleeveLength: 350);

            builder.Build(parameters);

            var handle = fake.Segments[2];

            double expected = parameters.SeparatorDiameter * 0.8;

            Assert.That(handle.Diameter, Is.EqualTo(expected).Within(1e-6));
        }
    }
}
