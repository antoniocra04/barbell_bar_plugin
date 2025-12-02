using System.Linq;
using BarbellBarPlugin.Model;
using BarbellBarPlugin.Validation;
using NUnit.Framework;

namespace BarbellBarPlugin.Tests
{
    /// <summary>
    /// Набор тестов для проверки поведения модели <see cref="BarParameters"/>
    /// и базовой валидации входных параметров грифа.
    /// </summary>
    [TestFixture]
    public class BarParametersTests
    {
        //TODO: description
        /// <summary>
        /// Конструктор корректно инициализирует все свойства модели.
        /// </summary>
        [Test]
        public void Constructor_AssignsProperties()
        {
            // arrange
            double sleeveDiameter = 30;
            double separatorLength = 50;
            double handleLength = 1200;
            double separatorDiameter = 40;
            double sleeveLength = 350;

            // act
            var parameters = new BarParameters(
                sleeveDiameter,
                separatorLength,
                handleLength,
                separatorDiameter,
                sleeveLength);

            // assert
            Assert.Multiple(() =>
            {
                Assert.That(parameters.SleeveDiameter, Is.EqualTo(sleeveDiameter));
                Assert.That(parameters.SeparatorLength, Is.EqualTo(separatorLength));
                Assert.That(parameters.HandleLength, Is.EqualTo(handleLength));
                Assert.That(parameters.SeparatorDiameter, Is.EqualTo(separatorDiameter));
                Assert.That(parameters.SleeveLength, Is.EqualTo(sleeveLength));
            });
        }

        //TODO: description
        /// <summary>
        /// Свойство TotalLength корректно возвращает суммарную длину грифа.
        /// </summary>
        [Test]
        public void TotalLength_CalculatedCorrectly()
        {
            // arrange
            var parameters = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 1200,
                separatorDiameter: 40,
                sleeveLength: 350);

            // act
            double total = parameters.TotalLength;

            // assert
            Assert.That(total, Is.EqualTo(2000.0).Within(1e-6));
        }

        //TODO: description
        /// <summary>
        /// Для набора корректных параметров валидатор не возвращает ошибок.
        /// </summary>
        [Test]
        public void Validator_ValidParameters_ReturnsNoErrors()
        {
            // arrange
            var parameters = new BarParameters(
                sleeveDiameter: 30,   // 25–40
                separatorLength: 50,  // 40–60
                handleLength: 1250,   // 1200–1310
                separatorDiameter: 40,// 35–50
                sleeveLength: 350     // 320–420
            );

            // act
            var errors = BarParametersValidator.Validate(parameters);

            // assert
            Assert.That(errors.Count, Is.EqualTo(0));
        }

        //TODO: duplication
        //TODO: description
        /// <summary>
        /// При выходе диаметра посадочной части за допустимый диапазон появляется ошибка.
        /// </summary>
        [Test]
        public void Validator_SleeveDiameterOutOfRange_AddsError()
        {
            var parameters = new BarParameters(
                sleeveDiameter: 10,   // меньше 25
                separatorLength: 50,
                handleLength: 1250,
                separatorDiameter: 40,
                sleeveLength: 350
            );

            var errors = BarParametersValidator.Validate(parameters);

            Assert.That(errors.Any(e => e.FieldName == "DiametrSleeve"), Is.True);
        }

        //TODO: duplication
        //TODO: description
        /// <summary>
        /// При выходе длины разделителя за допустимый диапазон появляется ошибка.
        /// </summary>
        [Test]
        public void Validator_SeparatorLengthOutOfRange_AddsError()
        {
            var parameters = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 10,  // меньше 40
                handleLength: 1250,
                separatorDiameter: 40,
                sleeveLength: 350
            );

            var errors = BarParametersValidator.Validate(parameters);

            Assert.That(errors.Any(e => e.FieldName == "LengthSeparator"), Is.True);
        }
        //TODO: duplication

        //TODO: description
        /// <summary>
        /// При выходе длины рукояти за допустимый диапазон появляется ошибка.
        /// </summary>
        [Test]
        public void Validator_HandleLengthOutOfRange_AddsError()
        {
            var parameters = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 200,    // меньше 1200
                separatorDiameter: 40,
                sleeveLength: 350
            );

            var errors = BarParametersValidator.Validate(parameters);

            Assert.That(errors.Any(e => e.FieldName == "LengthHandle"), Is.True);
        }
        //TODO: duplication

        //TODO: description
        /// <summary>
        /// При выходе диаметра разделителя за допустимый диапазон появляется ошибка.
        /// </summary>
        [Test]
        public void Validator_SeparatorDiameterOutOfRange_AddsError()
        {
            var parameters = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 1250,
                separatorDiameter: 10, // меньше 35
                sleeveLength: 350
            );

            var errors = BarParametersValidator.Validate(parameters);

            Assert.That(errors.Any(e => e.FieldName == "DiametrSeparator"), Is.True);
        }
        //TODO: duplication

        //TODO: description
        /// <summary>
        /// При выходе длины посадочной части за допустимый диапазон появляется ошибка.
        /// </summary>
        [Test]
        public void Validator_SleeveLengthOutOfRange_AddsError()
        {
            var parameters = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 1250,
                separatorDiameter: 40,
                sleeveLength: 100     // меньше 320
            );

            var errors = BarParametersValidator.Validate(parameters);

            Assert.That(errors.Any(e => e.FieldName == "LengthSleeve"), Is.True);
        }
        //TODO: duplication

        //TODO: description
        /// <summary>
        /// Если диаметр разделителя не больше диаметра посадочной части — добавляется ошибка.
        /// </summary>
        [Test]
        public void Validator_SeparatorNotGreaterThanSleeve_AddsError()
        {
            var parameters = new BarParameters(
                sleeveDiameter: 35,
                separatorLength: 50,
                handleLength: 1250,
                separatorDiameter: 35, // не больше
                sleeveLength: 350
            );

            var errors = BarParametersValidator.Validate(parameters);

            Assert.That(errors.Any(e => e.FieldName == "DiametrSeparator"), Is.True);
        }
        //TODO: duplication

        //TODO: description
        /// <summary>
        /// Если рукоять короче суммы двух разделителей — валидатор добавляет ошибку.
        /// </summary>
        [Test]
        public void Validator_HandleTooShortComparedToSeparators_AddsError()
        {
            var parameters = new BarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 80,  // меньше 2 * 50
                separatorDiameter: 40,
                sleeveLength: 350
            );

            var errors = BarParametersValidator.Validate(parameters);

            Assert.That(errors.Any(e => e.FieldName == "LengthHandle"), Is.True);
        }
    }
}
