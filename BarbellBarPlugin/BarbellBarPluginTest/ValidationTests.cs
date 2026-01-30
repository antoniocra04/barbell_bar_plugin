using System.Linq;

using BarbellBarPlugin.Core.Model;
using BarbellBarPlugin.Core.Validation;
using NUnit.Framework;

namespace BarbellBarPlugin.Tests
{
    [TestFixture]
    public class ValidationTests
    {
        private const double Tolerance = 1e-6;

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
        [Description(
            "Проверяет, что конструктор ValidationError выбрасывает " +
            "ArgumentException, если fieldName равен null.")]
        public void Ctor_Throws_WhenFieldNameIsNull()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => new ValidationError(null!, "Сообщение"));

            Assert.That(exception!.ParamName, Is.EqualTo("fieldName"));
        }

        [TestCase("", TestName = "Ctor_Throws_WhenFieldNameIsEmpty")]
        [TestCase("   ", TestName = "Ctor_Throws_WhenFieldNameIsWhitespace")]
        [Description(
            "Проверяет, что конструктор ValidationError выбрасывает " +
            "ArgumentException, если fieldName пустой/пробельный.")]
        public void Ctor_Throws_WhenFieldNameIsInvalid(string fieldName)
        {
            var exception = Assert.Throws<ArgumentException>(
                () => new ValidationError(fieldName, "Сообщение"));

            Assert.That(exception!.ParamName, Is.EqualTo("fieldName"));
        }

        [Test]
        [Description(
            "Проверяет, что конструктор ValidationError выбрасывает " +
            "ArgumentException, если message равен null.")]
        public void Ctor_Throws_WhenMessageIsNull()
        {
            var exception = Assert.Throws<ArgumentException>(
                () => new ValidationError("LengthSleeve", null!));

            Assert.That(exception!.ParamName, Is.EqualTo("message"));
        }

        [TestCase("", TestName = "Ctor_Throws_WhenMessageIsEmpty")]
        [TestCase("   ", TestName = "Ctor_Throws_WhenMessageIsWhitespace")]
        [Description(
            "Проверяет, что конструктор ValidationError выбрасывает " +
            "ArgumentException, если message пустой/пробельный.")]
        public void Ctor_Throws_WhenMessageIsInvalid(string message)
        {
            var exception = Assert.Throws<ArgumentException>(
                () => new ValidationError("LengthSleeve", message));

            Assert.That(exception!.ParamName, Is.EqualTo("message"));
        }

        [Test]
        //TODO: RSDN+
        [Description(
            "Проверяет, что конструктор BarbellBarParameters корректно " +
            "инициализирует все свойства.")]
        public void Constructor_AssignsProperties()
        {
            var sleeveDiameter = 30.0;
            var separatorLength = 50.0;
            var handleLength = 1200.0;
            var separatorDiameter = 40.0;
            var sleeveLength = 350.0;

            var parameters = new BarbellBarParameters(
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

            var totalLength = parameters.TotalLength;

            Assert.That(
                totalLength,
                Is.EqualTo(2000.0).Within(Tolerance));
        }

        [Test]
        //TODO: RSDN+
        [Description(
            "Проверяет, что при корректных параметрах " +
            "BarParametersValidator не возвращает ошибок.")]
        public void Validator_ValidParameters_ReturnsNoErrors()
        {
            var parameters = CreateParameters(
                sleeveDiameter: 30,    
                separatorLength: 50,   
                handleLength: 1250, 
                separatorDiameter: 40,
                sleeveLength: 350
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
                errors.Any(error => error.FieldName == expectedFieldName),
                Is.True,
                $"Ожидалась ошибка для поля '{expectedFieldName}'. " +
                //TODO: RSDN+
                $"Фактические ошибки: {string.Join(", ", errors.Select(error => error.FieldName))}");
        }

        private static BarbellBarParameters CreateParameters(
            double sleeveDiameter,
            double separatorLength,
            double handleLength,
            double separatorDiameter,
            double sleeveLength)
        {
            return new BarbellBarParameters(
                sleeveDiameter,
                separatorLength,
                handleLength,
                separatorDiameter,
                sleeveLength);
        }
    }
}
