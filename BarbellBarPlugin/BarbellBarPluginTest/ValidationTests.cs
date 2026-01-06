using System.Linq;
using BarbellBarPlugin.Model;
using BarbellBarPlugin.Validation;
using NUnit.Framework;

namespace BarbellBarPlugin.Tests
{
    [TestFixture]
    public class ValidationTests
    {
        [Test]
        [Description(
            "Проверяет, что конструктор ValidationError " +
            "корректно сохраняет имя поля и текст сообщения.")]
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

        [Test]
        //TODO: RSDN+
        [Description(
            "Проверяет, что конструктор BarParameters корректно " +
            "инициализирует все свойства.")]
        public void Constructor_AssignsProperties()
        {
            double sleeveDiameter = 30;
            double separatorLength = 50;
            double handleLength = 1200;
            double separatorDiameter = 40;
            double sleeveLength = 350;

            var parameters = new BarParameters(
                sleeveDiameter,
                separatorLength,
                handleLength,
                separatorDiameter,
                sleeveLength);

            Assert.Multiple(() =>
            {
                Assert.That(
                    parameters.SleeveDiameter,
                    Is.EqualTo(sleeveDiameter));

                Assert.That(
                    parameters.SeparatorLength,
                    Is.EqualTo(separatorLength));

                Assert.That(
                    parameters.HandleLength,
                    Is.EqualTo(handleLength));

                Assert.That(
                    parameters.SeparatorDiameter,
                    Is.EqualTo(separatorDiameter));

                Assert.That(
                    parameters.SleeveLength,
                    Is.EqualTo(sleeveLength));
            });
        }

        [Test]
        //TODO: RSDN+
        [Description(
            "Проверяет корректный расчёт свойства TotalLength " +
            "(суммарной длины грифа).")]
        public void TotalLength_CalculatedCorrectly()
        {
            var parameters = CreateParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 1200,
                separatorDiameter: 40,
                sleeveLength: 350);

            double total = parameters.TotalLength;

            Assert.That(
                total,
                Is.EqualTo(2000.0).Within(1e-6));
        }

        [Test]
        //TODO: RSDN+
        [Description(
            "Проверяет, что при корректных параметрах " +
            "BarParametersValidator не возвращает ошибок.")]
        public void Validator_ValidParameters_ReturnsNoErrors()
        {
            var parameters = CreateParameters(
                sleeveDiameter: 30,    // 25–40
                separatorLength: 50,   // 40–60
                handleLength: 1250,    // 1200–1310
                separatorDiameter: 40, // 35–50
                sleeveLength: 350      // 320–420
            );

            var errors = BarParametersValidator.Validate(parameters);

            Assert.That(errors, Is.Empty);
        }

        [TestCase(
            10, 50, 1250, 40, 350, "DiametrSleeve",
            TestName =
                "Validator_SleeveDiameterOutOfRange_AddsError(DiametrSleeve)")]
        [TestCase(
            30, 10, 1250, 40, 350, "LengthSeparator",
            TestName =
                "Validator_SeparatorLengthOutOfRange_AddsError(LengthSeparator)")]
        [TestCase(
            30, 50, 200, 40, 350, "LengthHandle",
            TestName =
                "Validator_HandleLengthOutOfRange_AddsError(LengthHandle)")]
        [TestCase(
            30, 50, 1250, 10, 350, "DiametrSeparator",
            TestName =
                "Validator_SeparatorDiameterOutOfRange_AddsError(DiametrSeparator)")]
        [TestCase(
            30, 50, 1250, 40, 100, "LengthSleeve",
            TestName =
                "Validator_SleeveLengthOutOfRange_AddsError(LengthSleeve)")]
        [TestCase(
            35, 50, 1250, 35, 350, "DiametrSeparator",
            TestName =
                "Validator_SeparatorNotGreaterThanSleeve_AddsError(DiametrSeparator)")]
        [TestCase(
            30, 50, 80, 40, 350, "LengthHandle",
            TestName =
                "Validator_HandleTooShortComparedToSeparators_AddsError(LengthHandle)")]
        [Description(
            "Проверяет, что при некорректных значениях " +
            "параметров валидатор добавляет ошибку для ожидаемого поля.")]
        public void Validator_InvalidParameters_AddsExpectedError(
            double sleeveDiameter,
            double separatorLength,
            double handleLength,
            double separatorDiameter,
            double sleeveLength,
            string expectedFieldName)
        {
            var parameters = CreateParameters(
                sleeveDiameter: sleeveDiameter,
                separatorLength: separatorLength,
                handleLength: handleLength,
                separatorDiameter: separatorDiameter,
                sleeveLength: sleeveLength);

            var errors = BarParametersValidator.Validate(parameters);

            Assert.That(
                errors.Any(e => e.FieldName == expectedFieldName),
                Is.True,
                $"Ожидалась ошибка для поля '{expectedFieldName}'. " +
                //TODO: RSDN+
                $"Фактические ошибки: {string.Join(", ", errors.Select(e => e.FieldName))}");
        }

        private static BarParameters CreateParameters(
            double sleeveDiameter,
            double separatorLength,
            double handleLength,
            double separatorDiameter,
            double sleeveLength)
        {
            return new BarParameters(
                sleeveDiameter,
                separatorLength,
                handleLength,
                separatorDiameter,
                sleeveLength);
        }
    }
}
