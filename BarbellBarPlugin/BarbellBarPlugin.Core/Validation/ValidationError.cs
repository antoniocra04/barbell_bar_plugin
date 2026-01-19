using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public ValidationError(string fieldName, string message)
        {
            FieldName = fieldName;
            Message = message;
        }
    }
}
