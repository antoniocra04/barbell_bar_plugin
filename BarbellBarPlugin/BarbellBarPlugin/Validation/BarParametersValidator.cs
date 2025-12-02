using System;
using System.Collections.Generic;
using BarbellBarPlugin.Model;

namespace BarbellBarPlugin.Validation
{
    //TODO:+ RSDN
    /// <summary>
    /// Описывает ошибку валидации отдельного параметра грифа.
    /// </summary>
    public class ValidationError
    {
        /// <summary>Имя поля, привязанного к ошибке (используется для UI).</summary>
        public string FieldName { get; }

        /// <summary>Текст ошибки валидации.</summary>
        public string Message { get; }

        public ValidationError(string fieldName, string message)
        {
            FieldName = fieldName;
            Message = message;
        }
    }

    /// <summary>
    /// Выполняет проверку параметров грифа на соответствие допустимым диапазонам.
    /// </summary>
    public static class BarParametersValidator
    {
        //TODO:+ XML
        /// <summary>Минимальный и максимальный диаметр посадочной части (мм).</summary>
        private const double SleeveDiameterMin = 25;
        private const double SleeveDiameterMax = 40;

        /// <summary>Минимальная и максимальная длина разделителя (мм).</summary>
        private const double SeparatorLengthMin = 40;
        private const double SeparatorLengthMax = 60;

        /// <summary>Минимальная и максимальная длина ручки (мм).</summary>
        private const double HandleLengthMin = 1200;
        private const double HandleLengthMax = 1310;

        /// <summary>Минимальный и максимальный диаметр разделителя (мм).</summary>
        private const double SeparatorDiameterMin = 35;
        private const double SeparatorDiameterMax = 50;

        /// <summary>Минимальная и максимальная длина посадочной части (мм).</summary>
        private const double SleeveLengthMin = 320;
        private const double SleeveLengthMax = 420;

        //TODO:+ XML
        //TODO:+ RSDN
        /// <summary>
        /// Проверяет параметры грифа и возвращает список ошибок валидации.
        /// </summary>
        /// <param name="p">Объект с параметрами грифа.</param>
        /// <returns>Список ошибок валидации. Пустой список означает, что все параметры корректны.</returns>
        public static IReadOnlyList<ValidationError> Validate(BarParameters p)
        {
            var errors = new List<ValidationError>();

            CheckRange(
                p.SleeveDiameter,
                SleeveDiameterMin,
                SleeveDiameterMax,
                "DiametrSleeve",
                "Диаметр посадочной части",
                errors);

            CheckRange(
                p.SeparatorLength,
                SeparatorLengthMin,
                SeparatorLengthMax,
                "LengthSeparator",
                "Длинна разделителя",
                errors);

            CheckRange(
                p.HandleLength,
                HandleLengthMin,
                HandleLengthMax,
                "LengthHandle",
                "Длинна ручки",
                errors);

            CheckRange(
                p.SeparatorDiameter,
                SeparatorDiameterMin,
                SeparatorDiameterMax,
                "DiametrSeparator",
                "Диаметр разделителя",
                errors);

            CheckRange(
                p.SleeveLength,
                SleeveLengthMin,
                SleeveLengthMax,
                "LengthSleeve",
                "Длинна посадочной части",
                errors);

            // Соотношение диаметров разделителя и посадки
            if (p.SeparatorDiameter <= p.SleeveDiameter)
            {
                errors.Add(new ValidationError(
                    "DiametrSeparator",
                    "Диаметр разделителя должен быть больше диаметра посадочной части."));
            }

            // Соотношение длины ручки и двух разделителей
            if (p.HandleLength <= 2 * p.SeparatorLength)
            {
                errors.Add(new ValidationError(
                    "LengthHandle",
                    "Длинна ручки должна быть больше суммарной длины двух разделителей."));
            }

            return errors;
        }

        //TODO:+ XML
        /// <summary>
        /// Проверяет параметр на попадание в допустимый числовой диапазон.
        /// </summary>
        /// <param name="value">Проверяемое значение параметра.</param>
        /// <param name="min">Минимально допустимое значение, включительно.</param>
        /// <param name="max">Максимально допустимое значение, включительно.</param>
        /// <param name="fieldName">Имя поля в UI (для привязки ошибки).</param>
        /// <param name="displayName">Название параметра для отображения в сообщении об ошибке.</param>
        /// <param name="errors">Список ошибок, в который добавляется сообщение в случае нарушения диапазона.</param>
        private static void CheckRange(
            double value,
            double min,
            double max,
            string fieldName,
            string displayName,
            List<ValidationError> errors)
        {
            if (value < min || value > max)
            {
                errors.Add(new ValidationError(
                    fieldName,
                    $"{displayName} должен быть в диапазоне от {min:0} до {max:0} мм."));
            }
        }
    }
}
