using System;
using BarbellBarPlugin.Core.Model;
using BarbellBarPlugin.Kompas;
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
            "Проверяет, что конструктор Builder выбрасывает " +
            "ArgumentNullException, если Wrapper равен null.")]
        public void Ctor_Throws_WhenWrapperIsNull()
        {
            Assert.Throws<ArgumentNullException>(
                () => new Builder(null!));
        }

        [Test]
        [Description(
            "Проверяет, что метод Build выбрасывает " +
            "ArgumentNullException, если параметры грифа равны null.")]
        public void Build_Throws_WhenParametersIsNull()
        {
            var fakeWrapper = new FakeKompasWrapper();
            var builder = new Builder(fakeWrapper);

            Assert.Throws<ArgumentNullException>(
                () => builder.Build(null!));
        }

        [Test]
        [Description(
            "Проверяет, что метод Build сохраняет ссылку на текущие " +
            "параметры в свойство CurrentParameters.")]
        public void Build_SetsCurrentParameters()
        {
            var fakeWrapper = new FakeKompasWrapper();
            var builder = new Builder(fakeWrapper);

            var parameters = new BarbellBarParameters(30, 50, 1200, 40, 350);

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
            var fakeWrapper = new FakeKompasWrapper();
            var builder = new Builder(fakeWrapper);

            var parameters = new BarbellBarParameters(30, 50, 1200, 40, 350);

            builder.Build(parameters);

            Assert.Multiple(() =>
            {
                Assert.That(fakeWrapper.AttachCalled, Is.True);
                Assert.That(fakeWrapper.CreateDocCalled, Is.True);
            });
        }

        [Test]
        [Description(
            "Проверяет, что AttachOrRunCAD вызывается при каждом вызове Build. " +
            "Повторное 'реальное' подключение отрабатывает внутри Wrapper " +
            "(проверка живости COM и переподключение при необходимости).")]
        public void Build_CallsAttachEachTime_WhenCalledTwice()
        {
            var fakeWrapper = new FakeKompasWrapper();
            var builder = new Builder(fakeWrapper);

            var parameters = new BarbellBarParameters(30, 50, 1200, 40, 350);

            builder.Build(parameters);
            builder.Build(parameters);

            Assert.That(
                fakeWrapper.AttachCallCount,
                Is.EqualTo(2),
                "AttachOrRunCAD должен вызываться при каждом вызове Build.");
        }

        [Test]
        [Description(
            "Проверяет, что метод Build создаёт 5 сегментов в порядке: " +
            "LeftSleeve, LeftSeparator, Handle, RightSeparator, RightSleeve.")]
        public void Build_CreatesFiveSegments_InCorrectOrder()
        {
            var fakeWrapper = new FakeKompasWrapper();
            var builder = new Builder(fakeWrapper);

            var parameters = new BarbellBarParameters(30, 50, 1200, 40, 350);

            builder.Build(parameters);

            var segments = fakeWrapper.Segments;

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
            var fakeWrapper = new FakeKompasWrapper();
            var builder = new Builder(fakeWrapper);

            var parameters = new BarbellBarParameters(30, 50, 1200, 40, 350);

            builder.Build(parameters);

            var segments = fakeWrapper.Segments;
            Assert.That(segments.Count, Is.EqualTo(5));

            Assert.Multiple(() =>
            {
                //TODO: within to const+
                Assert.That(
                    segments[0].StartX,
                    Is.EqualTo(0.0).Within(Tolerance));

                Assert.That(
                    segments[0].EndX,
                    Is.EqualTo(350.0).Within(Tolerance));

                Assert.That(
                    segments[1].StartX,
                    Is.EqualTo(350.0).Within(Tolerance));

                Assert.That(
                    segments[1].EndX,
                    Is.EqualTo(400.0).Within(Tolerance));

                Assert.That(
                    segments[2].StartX,
                    Is.EqualTo(400.0).Within(Tolerance));

                Assert.That(
                    segments[2].EndX,
                    Is.EqualTo(1600.0).Within(Tolerance));

                Assert.That(
                    segments[3].StartX,
                    Is.EqualTo(1600.0).Within(Tolerance));

                Assert.That(
                    segments[3].EndX,
                    Is.EqualTo(1650.0).Within(Tolerance));

                Assert.That(
                    segments[4].StartX,
                    Is.EqualTo(1650.0).Within(Tolerance));

                Assert.That(
                    segments[4].EndX,
                    Is.EqualTo(2000.0).Within(Tolerance));
            });

            var expectedHandleDiameter =
                Math.Min(
                    parameters.SleeveDiameter,
                    parameters.SeparatorDiameter) - 3.0;

            Assert.Multiple(() =>
            {
                //TODO: RSDN+
                Assert.That(
                    segments[0].Diameter,
                    Is.EqualTo(parameters.SleeveDiameter).Within(Tolerance));

                Assert.That(
                    segments[1].Diameter,
                    Is.EqualTo(parameters.SeparatorDiameter).Within(Tolerance));

                Assert.That(
                    segments[2].Diameter,
                    Is.EqualTo(expectedHandleDiameter).Within(Tolerance));

                Assert.That(
                    segments[3].Diameter,
                    Is.EqualTo(parameters.SeparatorDiameter).Within(Tolerance));

                Assert.That(
                    segments[4].Diameter,
                    Is.EqualTo(parameters.SleeveDiameter).Within(Tolerance));
            });
        }

        [Test]
        [Description(
            "Проверяет, что если вычисленный диаметр рукояти <= 0, " +
            "используется запасной диаметр (SeparatorDiameter * 0.8).")]
        public void Build_UsesFallbackHandleDiameter_WhenComputedDiameterIsNonPositive()
        {
            var fakeWrapper = new FakeKompasWrapper();
            var builder = new Builder(fakeWrapper);

            var parameters = new BarbellBarParameters(
                sleeveDiameter: 1,
                separatorLength: 50,
                handleLength: 1200,
                separatorDiameter: 1,
                sleeveLength: 350);

            builder.Build(parameters);

            var handleSegment = fakeWrapper.Segments[2];
            var expectedDiameter = parameters.SeparatorDiameter * 0.8;

            Assert.That(
                handleSegment.Diameter,
                Is.EqualTo(expectedDiameter).Within(Tolerance));
        }

        [Test]
        [Description(
            "Проверяет непокрытый finally: при closeDocumentAfterBuild=true " +
            "вызывается CloseActiveDocument3D(save:false).")]
        public void Build_ClosesDocumentInFinally_WhenCloseDocumentAfterBuildTrue()
        {
            var fakeWrapper = new FakeKompasWrapper();
            var builder = new Builder(fakeWrapper);

            var parameters = new BarbellBarParameters(30, 50, 1200, 40, 350);

            builder.Build(parameters, closeDocumentAfterBuild: true);

            Assert.Multiple(() =>
            {
                Assert.That(fakeWrapper.CloseDocCalled, Is.True);
                Assert.That(fakeWrapper.CloseDocSaveArg, Is.False);
            });
        }

        [Test]
        [Description(
            "Проверяет finally: при closeDocumentAfterBuild=false " +
            "CloseActiveDocument3D не вызывается.")]
        public void Build_DoesNotCloseDocument_WhenCloseDocumentAfterBuildFalse()
        {
            var fakeWrapper = new FakeKompasWrapper();
            var builder = new Builder(fakeWrapper);

            var parameters = new BarbellBarParameters(30, 50, 1200, 40, 350);

            builder.Build(parameters, closeDocumentAfterBuild: false);

            Assert.That(fakeWrapper.CloseDocCalled, Is.False);
        }
    }
}
