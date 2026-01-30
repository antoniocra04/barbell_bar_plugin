using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using BarbellBarPlugin.Core.Validation;
using BarbellBarPlugin.Kompas;
using BarbellBarPlugin.Core.Model;
using BarbellBarPlugin.Core.Validation;

namespace BarbellBarPlugin
{
    // TODO:+ XML
    /// <summary>
    /// Форма плагина для построения модели грифа в KOMPAS.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class MainForm : Form
    {
        /// <summary>
        /// Построитель модели грифа.
        /// </summary>
        private readonly Builder _builder;

        /// <summary>
        /// Настройки сериализации/десериализации JSON.
        /// </summary>
        private static readonly JsonSerializerOptions _jsonOptions =
            new JsonSerializerOptions
            {
                WriteIndented = true
            };

        /// <summary>
        /// DTO для сериализации/десериализации параметров грифа.
        /// Нужен, чтобы не зависеть от наличия set-свойств в модели
        /// BarParameters.
        /// </summary>
        private sealed class BarParametersDto
        {
            /// <summary>
            /// Диаметр посадочной части грифа.
            /// </summary>
            public double SleeveDiameter { get; set; }

            /// <summary>
            /// Длина разделителя.
            /// </summary>
            public double SeparatorLength { get; set; }

            /// <summary>
            /// Длина ручки (рабочей части хвата).
            /// </summary>
            public double HandleLength { get; set; }

            /// <summary>
            /// Диаметр разделителя.
            /// </summary>
            public double SeparatorDiameter { get; set; }

            /// <summary>
            /// Длина посадочной части грифа.
            /// </summary>
            public double SleeveLength { get; set; }

            /// <summary>
            /// Создаёт DTO на основе модели параметров грифа.
            /// </summary>
            /// <param name="parameters">Модель параметров грифа.</param>
            /// <returns>DTO для сериализации/десериализации.</returns>
            public static BarParametersDto FromModel(BarbellBarParameters parameters)
            {
                return new BarParametersDto
                {
                    SleeveDiameter = parameters.SleeveDiameter,
                    SeparatorLength = parameters.SeparatorLength,
                    HandleLength = parameters.HandleLength,
                    SeparatorDiameter = parameters.SeparatorDiameter,
                    SleeveLength = parameters.SleeveLength
                };
            }

            /// <summary>
            /// Преобразует DTO в модель параметров грифа.
            /// </summary>
            /// <returns>Модель параметров грифа.</returns>
            public BarbellBarParameters ToModel()
            {
                return new BarbellBarParameters(
                    sleeveDiameter: SleeveDiameter,
                    separatorLength: SeparatorLength,
                    handleLength: HandleLength,
                    separatorDiameter: SeparatorDiameter,
                    sleeveLength: SleeveLength);
            }
        }

        /// <summary>
        /// Инициализирует форму плагина и создаёт построитель грифа
        /// с обёрткой KOMPAS.
        /// </summary>
        public MainForm()
        {
            InitializeComponent();

            var wrapper = new Wrapper();
            _builder = new Builder(wrapper);
        }

        /// <summary>
        /// Обработчик события загрузки формы.
        /// Заполняет поля ввода параметров значениями по умолчанию.
        /// </summary>
        private void BarbelBarPlugin_Load(object sender, EventArgs e)
        {
            DiametrSleeveTextBox.Text = "30";
            LengthSeparatorTextBox.Text = "50";
            LengthHandleTextBox.Text = "1250";
            DiametrSeparatorTextBox.Text = "40";
            LengthSleeveTextBox.Text = "350";
        }

        /// <summary>
        /// Обработчик кнопки построения модели грифа.
        /// Выполняет чтение параметров, их валидацию и построение
        /// модели в KOMPAS.
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            ClearValidation();

            if (!TryReadParameters(out var parameters))
                return;

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
            catch (Exception exception)
            {
                MessageBox.Show(
                    "Ошибка при построении модели: " + exception.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Читает параметры из текстбоксов.
        /// Если есть ошибки парсинга (текст вместо числа и т.п.) –
        /// показывает MessageBox и возвращает false.
        /// </summary>
        /// <param name="parameters">
        /// Результирующий набор параметров грифа.
        /// </param>
        /// <returns>
        /// True, если все значения успешно считаны и преобразованы в
        /// числа; иначе false.
        /// </returns>
        private bool TryReadParameters(out BarbellBarParameters parameters)
        {
            parameters = null!;

            var parseErrors = new List<ValidationError>();

            var sleeveDiameter = ParseOrCollectError(
                DiametrSleeveTextBox,
                fieldName: "DiametrSleeve",
                displayName: "Диаметр посадочной части",
                parseErrors);

            var separatorLength = ParseOrCollectError(
                LengthSeparatorTextBox,
                fieldName: "LengthSeparator",
                displayName: "Длина разделителя",
                parseErrors);

            var handleLength = ParseOrCollectError(
                LengthHandleTextBox,
                fieldName: "LengthHandle",
                displayName: "Длина ручки",
                parseErrors);

            var separatorDiameter = ParseOrCollectError(
                DiametrSeparatorTextBox,
                fieldName: "DiametrSeparator",
                displayName: "Диаметр разделителя",
                parseErrors);

            var sleeveLength = ParseOrCollectError(
                LengthSleeveTextBox,
                fieldName: "LengthSleeve",
                displayName: "Длина посадочной части",
                parseErrors);

            if (parseErrors.Count > 0)
            {
                ShowValidationErrors(parseErrors);
                return false;
            }

            parameters = new BarbellBarParameters(
                sleeveDiameter,
                separatorLength,
                handleLength,
                separatorDiameter,
                sleeveLength);

            return true;
        }

        /// <summary>
        /// Парсит значение из TextBox.
        /// Если не удалось – подсвечивает поле, добавляет
        /// ValidationError в список и возвращает 0.
        /// </summary>
        /// <param name="textBox">
        /// Текстовое поле, из которого считывается значение.
        /// </param>
        /// <param name="fieldName">
        /// Внутреннее имя поля для валидации.
        /// </param>
        /// <param name="displayName">
        /// Отображаемое имя поля для сообщения пользователю.
        /// </param>
        /// <param name="errors">
        /// Список ошибок, куда будет добавлена ошибка парсинга при
        /// неуспехе.
        /// </param>
        /// <returns>
        /// Распарсенное числовое значение либо 0, если парсинг не
        /// удался.
        /// </returns>
        private double ParseOrCollectError(
            TextBox textBox,
            string fieldName,
            string displayName,
            List<ValidationError> errors)
        {
            var text = textBox.Text.Trim().Replace(',', '.');

            // TODO:+ RSDN
            if (!double.TryParse(
                    text,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out var value))
            {
                var message = $"Некорректное число в поле \"{displayName}\".";
                MarkError(textBox, message);
                errors.Add(new ValidationError(fieldName, message));
                return 0.0;
            }

            return value;
        }

        /// <summary>
        /// Сброс визуальной валидации (очистка подсветки полей и
        /// тултипов).
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
        /// Подсвечивает поле ввода и устанавливает подсказку
        /// (tooltip) с текстом ошибки.
        /// </summary>
        /// <param name="textBox">Поле, которое требуется подсветить.</param>
        /// <param name="message">
        /// Текст ошибки для отображения пользователю.
        /// </param>
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
        /// <param name="errors">
        /// Список ошибок, которые требуется отобразить пользователю.
        /// </param>
        private void ShowValidationErrors(IReadOnlyList<ValidationError> errors)
        {
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("Обнаружены ошибки ввода:");
            messageBuilder.AppendLine();

            foreach (var error in errors)
            {
                var relatedTextBox = error.FieldName switch
                {
                    "DiametrSleeve" => DiametrSleeveTextBox,
                    "LengthSeparator" => LengthSeparatorTextBox,
                    "LengthHandle" => LengthHandleTextBox,
                    "DiametrSeparator" => DiametrSeparatorTextBox,
                    "LengthSleeve" => LengthSleeveTextBox,
                    _ => null
                };

                if (relatedTextBox != null)
                    MarkError(relatedTextBox, error.Message);

                messageBuilder.AppendLine("• " + error.Message);
            }

            MessageBox.Show(
                messageBuilder.ToString(),
                "Ошибка ввода параметров",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Применяет выбранный пресет (набор параметров) к полям
        /// ввода формы.
        /// Также сбрасывает визуальную валидацию.
        /// </summary>
        /// <param name="preset">
        /// Набор параметров грифа, который нужно подставить в форму.
        /// </param>
        private void ApplyPreset(BarbellBarParameters preset)
        {
            ClearValidation();

            DiametrSleeveTextBox.Text = preset.SleeveDiameter.ToString(
                CultureInfo.InvariantCulture);

            LengthSeparatorTextBox.Text = preset.SeparatorLength.ToString(
                CultureInfo.InvariantCulture);

            LengthHandleTextBox.Text = preset.HandleLength.ToString(
                CultureInfo.InvariantCulture);

            DiametrSeparatorTextBox.Text = preset.SeparatorDiameter.ToString(
                CultureInfo.InvariantCulture);

            LengthSleeveTextBox.Text = preset.SleeveLength.ToString(
                CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Создаёт пресет параметров для мужского грифа.
        /// Значения должны соответствовать требованиям
        /// методички/валидатора.
        /// </summary>
        /// <returns>Параметры мужского грифа.</returns>
        private static BarbellBarParameters CreateMalePreset()
        {
            return new BarbellBarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 1250,
                separatorDiameter: 40,
                sleeveLength: 350);
        }

        /// <summary>
        /// Создаёт пресет параметров для женского грифа.
        /// Значения должны соответствовать требованиям
        /// методички/валидатора.
        /// </summary>
        /// <returns>Параметры женского грифа.</returns>
        private static BarbellBarParameters CreateFemalePreset()
        {
            return new BarbellBarParameters(
                sleeveDiameter: 30,
                separatorLength: 50,
                handleLength: 1200,
                separatorDiameter: 40,
                sleeveLength: 320);
        }

        /// <summary>
        /// Обработчик кнопки пресета мужского грифа.
        /// Подставляет стандартные параметры мужского грифа в форму.
        /// </summary>
        private void MalePresetButton_Click(object sender, EventArgs e)
        {
            ApplyPreset(CreateMalePreset());
        }

        /// <summary>
        /// Обработчик кнопки пресета женского грифа.
        /// Подставляет стандартные параметры женского грифа в форму.
        /// </summary>
        private void FemalePresetButton_Click(object sender, EventArgs e)
        {
            ApplyPreset(CreateFemalePreset());
        }

        /// <summary>
        /// Обработчик кнопки "Сохранить параметры".
        /// Сохраняет текущие параметры в файл (JSON).
        /// </summary>
        private void SaveParamsButton_Click(object sender, EventArgs e)
        {
            SaveParametersToFile();
        }

        /// <summary>
        /// Обработчик кнопки "Загрузить параметры".
        /// Загружает параметры из файла (JSON) и подставляет их в
        /// форму.
        /// </summary>
        private void LoadParamsButton_Click(object sender, EventArgs e)
        {
            LoadParametersFromFile();
        }

        /// <summary>
        /// Сохраняет текущие параметры из формы в JSON-файл.
        /// Сохраняет только валидные параметры.
        /// </summary>
        private void SaveParametersToFile()
        {
            ClearValidation();

            if (!TryReadParameters(out var parameters))
                return;

            var errors = BarParametersValidator.Validate(parameters);
            if (errors.Any())
            {
                ShowValidationErrors(errors);
                return;
            }

            using var saveFileDialog = new SaveFileDialog
            {
                Title = "Сохранить параметры грифа",
                Filter =
                    "Параметры грифа (*.json)|*.json|"
                    + "Все файлы (*.*)|*.*",
                DefaultExt = "json",
                AddExtension = true,
                FileName = "bar-parameters.json"
            };

            if (saveFileDialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                var parametersDto = BarParametersDto.FromModel(parameters);
                var json = JsonSerializer.Serialize(parametersDto, _jsonOptions);
                File.WriteAllText(saveFileDialog.FileName, json, Encoding.UTF8);

                MessageBox.Show(
                    "Параметры успешно сохранены.",
                    "Готово",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "Ошибка при сохранении: " + exception.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Загружает параметры грифа из JSON-файла и подставляет их в
        /// форму.
        /// После загрузки выполняет валидацию и показывает ошибки
        /// (если есть).
        /// </summary>
        private void LoadParametersFromFile()
        {
            using var openFileDialog = new OpenFileDialog
            {
                Title = "Загрузить параметры грифа",
                Filter =
                    "Параметры грифа (*.json)|*.json|"
                    + "Все файлы (*.*)|*.*",
                DefaultExt = "json",
                CheckFileExists = true
            };

            if (openFileDialog.ShowDialog(this) != DialogResult.OK)
                return;

            try
            {
                var json = File.ReadAllText(openFileDialog.FileName, Encoding.UTF8);

                var parametersDto = JsonSerializer.Deserialize<BarParametersDto>(
                    json,
                    _jsonOptions);

                if (parametersDto == null)
                {
                    throw new InvalidDataException(
                        "Файл не содержит корректных параметров.");
                }

                var parameters = parametersDto.ToModel();
                ApplyPreset(parameters);

                var errors = BarParametersValidator.Validate(parameters);
                if (errors.Any())
                {
                    ShowValidationErrors(errors);
                }
                else
                {
                    MessageBox.Show(
                        "Параметры успешно загружены.",
                        "Готово",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (JsonException)
            {
                MessageBox.Show(
                    "Файл повреждён или имеет неверный формат JSON.",
                    "Ошибка загрузки",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (InvalidDataException)
            {
                MessageBox.Show(
                    "Файл повреждён или не содержит параметров грифа.",
                    "Ошибка загрузки",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception exception)
            {
                MessageBox.Show(
                    "Ошибка при загрузке файла: " + exception.Message,
                    "Ошибка",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
