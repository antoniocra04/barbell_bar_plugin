using System;
using BarbellBarPlugin.Kompas;
using BarbellBarPlugin.Model;
using NUnit.Framework;

namespace BarbellBarPlugin.Tests
{
    [TestFixture]
    public class BarBuilderTests
    {
        private const double Tolerance = 1e-6;

        [Test]
        //TODO: RSDN+
        [Description(
            "Проверяет, что конструктор BarBuilder выбрасывает " +
            "ArgumentNullException, если Wrapper равен null.")]
        public void Ctor_Throws_WhenWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => new BarBuilder(null!));
        }

        [Test]
        [Description(
            "Проверяет, что метод Build выбрасывает " +
            "ArgumentNullException, если параметры грифа равны null.")]
        public void Build_Throws_WhenParametersIsNull()
        {
            var fake = new FakeKompasWrapper();
            var builder = new BarBuilder(fake);

            Assert.Throws<ArgumentNullException>(
                () => builder.Build(null!));
        }

        [Test]
        [Description(
            "Проверяет, что метод Build сохраняет ссылку на текущие " +
            "параметры в свойство CurrentParameters.")]
        public void Build_SetsCurrentParameters()
        {
            var fake = new FakeKompasWrapper();
            var builder = new BarBuilder(fake);

            var parameters = new BarParameters(30, 50, 1200, 40, 350);

            builder.Build(parameters);

            Assert.That(
                builder.CurrentParameters,
                Is.SameAs(parameters));
        }

        [Test]
        [Description(
            "Проверяет, что метод Build вызывает подключение к КОМПАС " +
            "(AttachOrRunCAD) и создание нового документа (CreateDocument3D).")]
        public void Build_CallsAttachAndCreateDocument()
        {
            var fake = new FakeKompasWrapper();
            var builder = new BarBuilder(fake);

            var parameters = new BarParameters(30, 50, 1200, 40, 350);

            builder.Build(parameters);

            Assert.Multiple(() =>
            {
                Assert.That(fake.AttachCalled, Is.True);
                Assert.That(fake.CreateDocCalled, Is.True);
            });
        }

        [Test]
        [Description(
            "Проверяет, что AttachOrRunCAD вызывается только один раз " +
            "при двух вызовах Build.")]
        public void Build_AttachesOnlyOnce_WhenCalledTwice()
        {
            var fake = new FakeKompasWrapper();
            var builder = new BarBuilder(fake);

            var p = new BarParameters(30, 50, 1200, 40, 350);

            builder.Build(p);
            builder.Build(p);

            Assert.That(
                fake.AttachCallCount,
                Is.EqualTo(1),
                "AttachOrRunCAD должен вызываться только один раз.");
        }

        [Test]
        [Description(
            "Проверяет, что метод Build создаёт 5 сегментов в порядке: " +
            "LeftSleeve, LeftSeparator, Handle, RightSeparator, RightSleeve.")]
        public void Build_CreatesFiveSegments_InCorrectOrder()
        {
            var fake = new FakeKompasWrapper();
            var builder = new BarBuilder(fake);

            var parameters = new BarParameters(30, 50, 1200, 40, 350);

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
        [Description(
            "Проверяет координаты X и диаметры сегментов " +
            "(включая диаметр рукояти) на базовом наборе параметров.")]
        public void Build_SegmentsHaveCorrectCoordinatesAndDiameters()
        {
            var fake = new FakeKompasWrapper();
            var builder = new BarBuilder(fake);

            var parameters = new BarParameters(30, 50, 1200, 40, 350);

            builder.Build(parameters);

            var s = fake.Segments;
            Assert.That(s.Count, Is.EqualTo(5));

            Assert.Multiple(() =>
            {
                //TODO: within to const+
                Assert.That(
                    s[0].StartX,
                    Is.EqualTo(0.0).Within(Tolerance));

                Assert.That(
                    s[0].EndX,
                    Is.EqualTo(350.0).Within(Tolerance));

                Assert.That(
                    s[1].StartX,
                    Is.EqualTo(350.0).Within(Tolerance));

                Assert.That(
                    s[1].EndX,
                    Is.EqualTo(400.0).Within(Tolerance));

                Assert.That(
                    s[2].StartX,
                    Is.EqualTo(400.0).Within(Tolerance));

                Assert.That(
                    s[2].EndX,
                    Is.EqualTo(1600.0).Within(Tolerance));

                Assert.That(
                    s[3].StartX,
                    Is.EqualTo(1600.0).Within(Tolerance));

                Assert.That(
                    s[3].EndX,
                    Is.EqualTo(1650.0).Within(Tolerance));

                Assert.That(
                    s[4].StartX,
                    Is.EqualTo(1650.0).Within(Tolerance));

                Assert.That(
                    s[4].EndX,
                    Is.EqualTo(2000.0).Within(Tolerance));
            });

            double expectedHandleDiameter =
                Math.Min(
                    parameters.SleeveDiameter,
                    parameters.SeparatorDiameter) - 3.0;

            Assert.Multiple(() =>
            {
                //TODO: RSDN+
                Assert.That(
                    s[0].Diameter,
                    Is.EqualTo(parameters.SleeveDiameter).Within(Tolerance));

                Assert.That(
                    s[1].Diameter,
                    Is.EqualTo(parameters.SeparatorDiameter).Within(Tolerance));

                Assert.That(
                    s[2].Diameter,
                    Is.EqualTo(expectedHandleDiameter).Within(Tolerance));

                Assert.That(
                    s[3].Diameter,
                    Is.EqualTo(parameters.SeparatorDiameter).Within(Tolerance));

                Assert.That(
                    s[4].Diameter,
                    Is.EqualTo(parameters.SleeveDiameter).Within(Tolerance));
            });
        }

        [Test]
        [Description(
            "Проверяет, что если вычисленный диаметр рукояти <= 0, " +
            "используется запасной диаметр (SeparatorDiameter * 0.8).")]
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

            Assert.That(
                handle.Diameter,
                Is.EqualTo(expected).Within(Tolerance));
        }

        [Test]
        [Description(
            "Проверяет непокрытый finally: при closeDocumentAfterBuild=true " +
            "вызывается CloseActiveDocument3D(save:false).")]
        public void Build_ClosesDocumentInFinally_WhenCloseDocumentAfterBuildTrue()
        {
            var fake = new FakeKompasWrapper();
            var builder = new BarBuilder(fake);

            var parameters = new BarParameters(30, 50, 1200, 40, 350);

            builder.Build(parameters, closeDocumentAfterBuild: true);

            Assert.Multiple(() =>
            {
                Assert.That(fake.CloseDocCalled, Is.True);
                Assert.That(fake.CloseDocSaveArg, Is.False);
            });
        }

        [Test]
        [Description(
            "Проверяет finally: при closeDocumentAfterBuild=false " +
            "CloseActiveDocument3D не вызывается.")]
        public void Build_DoesNotCloseDocument_WhenCloseDocumentAfterBuildFalse()
        {
            var fake = new FakeKompasWrapper();
            var builder = new BarBuilder(fake);

            var parameters = new BarParameters(30, 50, 1200, 40, 350);

            builder.Build(parameters, closeDocumentAfterBuild: false);

            Assert.That(fake.CloseDocCalled, Is.False);
        }
    }
}
