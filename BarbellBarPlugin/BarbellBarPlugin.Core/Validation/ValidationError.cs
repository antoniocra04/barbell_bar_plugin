using System;

namespace BarbellBarPlugin.Core.Validation
{
    /// <summary>
    /// Описывает ошибку валидации отдельного параметра грифа.
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Имя поля, привязанного к ошибке (используется для UI).
        /// </summary>
        public string FieldName { get; }

        /// <summary>
        /// Текст ошибки валидации.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Создаёт новую ошибку валидации параметра.
        /// </summary>
        /// <param name="fieldName">
        /// Имя поля, с которым связана ошибка.
        /// </param>
        /// <param name="message">
        /// Текст сообщения об ошибке.
        /// </param>
        /// <exception cref="ArgumentException">
        /// Если <paramref name="fieldName"/> или <paramref name="message"/>
        /// пустые или содержат только пробелы.
        /// </exception>
        public ValidationError(string fieldName, string message)
        {
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                throw new ArgumentException(
                    "Имя поля не может быть пустым.",
                    nameof(fieldName));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException(
                    "Сообщение об ошибке не может быть пустым.",
                    nameof(message));
            }

            FieldName = fieldName;
            Message = message;
        }
    }
}
