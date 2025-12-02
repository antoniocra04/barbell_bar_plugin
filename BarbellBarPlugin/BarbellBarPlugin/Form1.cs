using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BarbellBarPlugin.Kompas;
using BarbellBarPlugin.Model;
using BarbellBarPlugin.Validation;

namespace BarbellBarPlugin
{
    //TODO: XML
    public partial class BarbelBarPlugin : Form
    {
        private readonly BarBuilder _builder;

        public BarbelBarPlugin()
        {
            InitializeComponent();

            var wrapper = new Wrapper();
            _builder = new BarBuilder(wrapper);
        }

        private void BarbelBarPlugin_Load(object sender, EventArgs e)
        {
            DiametrSleeveTextBox.Text = "30";
            LengthSeparatorTextBox.Text = "50";
            LengthHandleTextBox.Text = "1250";
            DiametrSeparatorTextBox.Text = "40";
            LengthSleeveTextBox.Text = "350";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ClearValidation();

            if (!TryReadParameters(out var parameters))
            {
                return;
            }

            var errors = BarParametersValidator.Validate(parameters);

            if (errors.Any())
            {
                ShowValidationErrors(errors);
                return;
            }

            try
            {
                _builder.Build(parameters);
                MessageBox.Show(
                    "Модель грифа успешно построена.",
                    "Готово",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при построении модели: " + ex.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Читает параметры из текстбоксов.
        /// Если есть ошибки парсинга (текст вместо числа и т.п.) – показывает MessageBox и возвращает false.
        /// </summary>
        private bool TryReadParameters(out BarParameters parameters)
        {
            parameters = null!;

            var parseErrors = new List<ValidationError>();

            double sleeveDiameter = ParseOrCollectError(
                DiametrSleeveTextBox,
                "DiametrSleeve",
                "Диаметр посадочной части",
                parseErrors);

            double separatorLength = ParseOrCollectError(
                LengthSeparatorTextBox,
                "LengthSeparator",
                "Длинна разделителя",
                parseErrors);

            double handleLength = ParseOrCollectError(
                LengthHandleTextBox,
                "LengthHandle",
                "Длинна ручки",
                parseErrors);

            double separatorDiameter = ParseOrCollectError(
                DiametrSeparatorTextBox,
                "DiametrSeparator",
                "Диаметр разделителя",
                parseErrors);

            double sleeveLength = ParseOrCollectError(
                LengthSleeveTextBox,
                "LengthSleeve",
                "Длинна посадочной части",
                parseErrors);

            if (parseErrors.Count > 0)
            {
                ShowValidationErrors(parseErrors);
                return false;
            }

            parameters = new BarParameters(
                sleeveDiameter,
                separatorLength,
                handleLength,
                separatorDiameter,
                sleeveLength);

            return true;
        }

        /// <summary>
        /// Парсит значение из TextBox.
        /// Если не удалось – красим поле, кладём ValidationError в список, возвращаем 0.
        /// </summary>
        private double ParseOrCollectError(
            TextBox textBox,
            string fieldName,
            string displayName,
            List<ValidationError> errors)
        {
            string text = textBox.Text.Trim().Replace(',', '.');

            //TODO: RSDN
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                string msg = $"Некорректное число в поле \"{displayName}\".";
                MarkError(textBox, msg);
                errors.Add(new ValidationError(fieldName, msg));
                return 0.0;
            }

            return value;
        }

        /// <summary>
        /// Сброс визуальной валидации.
        /// </summary>
        private void ClearValidation()
        {
            toolTip1.RemoveAll();

            DiametrSleeveTextBox.BackColor = Color.White;
            LengthSeparatorTextBox.BackColor = Color.White;
            LengthHandleTextBox.BackColor = Color.White;
            DiametrSeparatorTextBox.BackColor = Color.White;
            LengthSleeveTextBox.BackColor = Color.White;
        }

        /// <summary>
        /// Подсветить текстбокс и повесить тултип с текстом ошибки.
        /// </summary>
        private void MarkError(TextBox textBox, string message)
        {
            textBox.BackColor = Color.MistyRose;
            toolTip1.SetToolTip(textBox, message);
        }

        /// <summary>
        /// Показать все ошибки валидации:
        /// - подсветить поля красным,
        /// - показать MessageBox со списком ошибок.
        /// Работает и для ошибок парсинга, и для ошибок диапазонов.
        /// </summary>
        private void ShowValidationErrors(IReadOnlyList<ValidationError> errors)
        {
            //TODO: RSDN
            var sb = new StringBuilder();
            sb.AppendLine("Обнаружены ошибки ввода:");
            sb.AppendLine();

            foreach (var error in errors)
            {
                TextBox tb = error.FieldName switch
                {
                    "DiametrSleeve" => DiametrSleeveTextBox,
                    "LengthSeparator" => LengthSeparatorTextBox,
                    "LengthHandle" => LengthHandleTextBox,
                    "DiametrSeparator" => DiametrSeparatorTextBox,
                    "LengthSleeve" => LengthSleeveTextBox,
                    _ => null
                };

                if (tb != null)
                    MarkError(tb, error.Message);

                sb.AppendLine("• " + error.Message);
            }

            MessageBox.Show(
                sb.ToString(),
                "Ошибка ввода параметров",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}
