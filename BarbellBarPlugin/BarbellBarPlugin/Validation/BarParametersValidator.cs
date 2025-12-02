using System;
using System.Collections.Generic;
using BarbellBarPlugin.Model;

namespace BarbellBarPlugin.Validation
{
    //TODO: RSDN
    public class ValidationError
    {
        public string FieldName { get; }
        public string Message { get; }

        public ValidationError(string fieldName, string message)
        {
            FieldName = fieldName;
            Message = message;
        }
    }

    /// <summary>
    /// Валидация параметров грифа по диапазонам из формы.
    /// </summary>
    public static class BarParametersValidator
    {
        //TODO: XML
        private const double SleeveDiameterMin = 25;
        private const double SleeveDiameterMax = 40;

        private const double SeparatorLengthMin = 40;
        private const double SeparatorLengthMax = 60;

        private const double HandleLengthMin = 1200;
        private const double HandleLengthMax = 1310;

        private const double SeparatorDiameterMin = 35;
        private const double SeparatorDiameterMax = 50;

        private const double SleeveLengthMin = 320;
        private const double SleeveLengthMax = 420;

        //TODO: XML
        //TODO: RSDN
        public static IReadOnlyList<ValidationError> Validate(BarParameters p)
        {
            var errors = new List<ValidationError>();

            CheckRange(p.SleeveDiameter, SleeveDiameterMin, SleeveDiameterMax,
                "DiametrSleeve", "Диаметр посадочной части", errors);

            CheckRange(p.SeparatorLength, SeparatorLengthMin, SeparatorLengthMax,
                "LengthSeparator", "Длинна разделителя", errors);

            CheckRange(p.HandleLength, HandleLengthMin, HandleLengthMax,
                "LengthHandle", "Длинна ручки", errors);

            CheckRange(p.SeparatorDiameter, SeparatorDiameterMin, SeparatorDiameterMax,
                "DiametrSeparator", "Диаметр разделителя", errors);

            CheckRange(p.SleeveLength, SleeveLengthMin, SleeveLengthMax,
                "LengthSleeve", "Длинна посадочной части", errors);

            if (p.SeparatorDiameter <= p.SleeveDiameter)
            {
                errors.Add(new ValidationError(
                    "DiametrSeparator",
                    "Диаметр разделителя должен быть больше диаметра посадочной части."));
            }

            if (p.HandleLength <= 2 * p.SeparatorLength)
            {
                errors.Add(new ValidationError(
                    "LengthHandle",
                    "Длинна ручки должна быть больше суммарной длины двух разделителей."));
            }

            return errors;
        }

        //TODO: XML
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
