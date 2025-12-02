using BarbellBarPlugin.Kompas;
using BarbellBarPlugin.Model;
using NUnit.Framework;

namespace BarbellBarPlugin.Tests
{
    /// <summary>
    /// Набор тестов для проверки корректности построения грифа в <see cref="BarBuilder"/>.
    /// Использует <see cref="FakeKompasWrapper"/> вместо реального KOMPAS.
    /// </summary>
    [TestFixture]
    public class BarBuilderTests
    {
        /// <summary>
        /// При вызове Build должны быть вызваны методы подключения к KOMPAS
        /// и создания 3D-документа.
        /// </summary>
        [Test]
        public void Build_CallsAttachAndCreateDocument()
        {
            // arrange
            var fake = new FakeKompasWrapper();
            var builder = new BarBuilder(fake);

            var parameters = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 1200,
                separatorDiameter: 40,
                sleeveLength: 350);

            // act
            builder.Build(parameters);

            // assert
            Assert.Multiple(() =>
            {
                Assert.That(fake.AttachCalled, Is.True, "Ожидался вызов AttachOrRunCAD.");
                Assert.That(fake.CreateDocCalled, Is.True, "Ожидался вызов CreateDocument3D.");
            });
        }

        /// <summary>
        /// Метод Build создаёт пять сегментов в ожидаемом порядке:
        /// левая посадка, левый разделитель, рукоять, правый разделитель, правая посадка.
        /// </summary>
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

            Assert.That(segments.Count, Is.EqualTo(5), "Должно быть 5 сегментов.");

            Assert.Multiple(() =>
            {
                Assert.That(segments[0].Name, Is.EqualTo("LeftSleeve"));
                Assert.That(segments[1].Name, Is.EqualTo("LeftSeparator"));
                Assert.That(segments[2].Name, Is.EqualTo("Handle"));
                Assert.That(segments[3].Name, Is.EqualTo("RightSeparator"));
                Assert.That(segments[4].Name, Is.EqualTo("RightSleeve"));
            });
        }

        /// <summary>
        /// Все сегменты грифа создаются с корректными координатами и диаметрами.
        /// </summary>
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

            // Проверяем координаты по X
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

            // Проверяем диаметры
            double expectedHandleDiameter =
                System.Math.Min(parameters.SleeveDiameter, parameters.SeparatorDiameter) - 3.0;

            Assert.Multiple(() =>
            {
                Assert.That(s[0].Diameter, Is.EqualTo(parameters.SleeveDiameter).Within(1e-6));
                Assert.That(s[1].Diameter, Is.EqualTo(parameters.SeparatorDiameter).Within(1e-6));
                Assert.That(s[2].Diameter, Is.EqualTo(expectedHandleDiameter).Within(1e-6));
                Assert.That(s[3].Diameter, Is.EqualTo(parameters.SeparatorDiameter).Within(1e-6));
                Assert.That(s[4].Diameter, Is.EqualTo(parameters.SleeveDiameter).Within(1e-6));
            });
        }
    }
}
