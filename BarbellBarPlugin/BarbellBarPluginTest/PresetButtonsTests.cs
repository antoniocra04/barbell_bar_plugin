using System.Drawing;
using System.Reflection;
using System.Threading;

using NUnit.Framework;

namespace BarbellBarPlugin.Tests
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class BarbelBarPlugin_PresetButtonsTests
    {
        private const string SleeveDiameterTextBoxFieldName = "DiametrSleeveTextBox";
        private const string SeparatorLengthTextBoxFieldName = "LengthSeparatorTextBox";
        private const string HandleLengthTextBoxFieldName = "LengthHandleTextBox";
        private const string SeparatorDiameterTextBoxFieldName = "DiametrSeparatorTextBox";
        private const string SleeveLengthTextBoxFieldName = "LengthSleeveTextBox";

        [Test]
        //TODO: RSDN
        [Description(
            "Проверяет, что нажатие кнопки мужского пресета заполняет поля ожидаемыми значениями.")]
        public void MalePresetButton_Click_FillsTextBoxes_WithMalePresetValues()
        {
            using var form = new BarbellBarPlugin.MainForm();

            SetText(form, SleeveDiameterTextBoxFieldName, "999");
            SetText(form, SeparatorLengthTextBoxFieldName, "999");
            SetText(form, HandleLengthTextBoxFieldName, "999");
            SetText(form, SeparatorDiameterTextBoxFieldName, "999");
            SetText(form, SleeveLengthTextBoxFieldName, "999");

            InvokeClick(form, "MalePresetButton_Click");

            Assert.Multiple(() =>
            {
                Assert.That(GetText(form, SleeveDiameterTextBoxFieldName), Is.EqualTo("30"));
                Assert.That(GetText(form, SeparatorLengthTextBoxFieldName), Is.EqualTo("50"));
                Assert.That(GetText(form, HandleLengthTextBoxFieldName), Is.EqualTo("1250"));
                Assert.That(GetText(form, SeparatorDiameterTextBoxFieldName), Is.EqualTo("40"));
                Assert.That(GetText(form, SleeveLengthTextBoxFieldName), Is.EqualTo("350"));
            });
        }

        [Test]
        //TODO: RSDN
        [Description(
            "Проверяет, что нажатие кнопки женского пресета заполняет поля ожидаемыми значениями.")]
        public void FemalePresetButton_Click_FillsTextBoxes_WithFemalePresetValues()
        {
            using var form = new BarbellBarPlugin.MainForm();

            SetText(form, SleeveDiameterTextBoxFieldName, "999");
            SetText(form, SeparatorLengthTextBoxFieldName, "999");
            SetText(form, HandleLengthTextBoxFieldName, "999");
            SetText(form, SeparatorDiameterTextBoxFieldName, "999");
            SetText(form, SleeveLengthTextBoxFieldName, "999");

            InvokeClick(form, "FemalePresetButton_Click");

            Assert.Multiple(() =>
            {
                Assert.That(GetText(form, SleeveDiameterTextBoxFieldName), Is.EqualTo("30"));
                Assert.That(GetText(form, SeparatorLengthTextBoxFieldName), Is.EqualTo("50"));
                Assert.That(GetText(form, HandleLengthTextBoxFieldName), Is.EqualTo("1200"));
                Assert.That(GetText(form, SeparatorDiameterTextBoxFieldName), Is.EqualTo("40"));
                Assert.That(GetText(form, SleeveLengthTextBoxFieldName), Is.EqualTo("320"));
            });
        }

        [Test]
        [Description(
            "Проверяет, что нажатие кнопки пресета сбрасывает визуальную валидацию (BackColor становится White).")]
        public void PresetButton_Click_ClearsValidation_BackColorToWhite()
        {
            using var form = new BarbellBarPlugin.MainForm();

            SetBackColor(form, SleeveDiameterTextBoxFieldName, Color.MistyRose);
            SetBackColor(form, SeparatorLengthTextBoxFieldName, Color.MistyRose);
            SetBackColor(form, HandleLengthTextBoxFieldName, Color.MistyRose);
            SetBackColor(form, SeparatorDiameterTextBoxFieldName, Color.MistyRose);
            SetBackColor(form, SleeveLengthTextBoxFieldName, Color.MistyRose);

            InvokeClick(form, "MalePresetButton_Click");

            Assert.Multiple(() =>
            {
                Assert.That(GetBackColor(form, SleeveDiameterTextBoxFieldName), Is.EqualTo(Color.White));
                Assert.That(GetBackColor(form, SeparatorLengthTextBoxFieldName), Is.EqualTo(Color.White));
                Assert.That(GetBackColor(form, HandleLengthTextBoxFieldName), Is.EqualTo(Color.White));
                Assert.That(GetBackColor(form, SeparatorDiameterTextBoxFieldName), Is.EqualTo(Color.White));
                Assert.That(GetBackColor(form, SleeveLengthTextBoxFieldName), Is.EqualTo(Color.White));
            });
        }

        private static void InvokeClick(object form, string handlerName)
        {
            var handlerMethod = form
                .GetType()
                .GetMethod(handlerName, BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.That(handlerMethod, Is.Not.Null, $"Не найден обработчик {handlerName}.");

            handlerMethod!.Invoke(form, new object[] { null!, System.EventArgs.Empty });
        }

        private static void SetText(object form, string textBoxFieldName, string value)
        {
            GetTextBox(form, textBoxFieldName).Text = value;
        }

        private static string GetText(object form, string textBoxFieldName)
        {
            return GetTextBox(form, textBoxFieldName).Text;
        }

        private static void SetBackColor(object form, string textBoxFieldName, Color color)
        {
            GetTextBox(form, textBoxFieldName).BackColor = color;
        }

        private static Color GetBackColor(object form, string textBoxFieldName)
        {
            return GetTextBox(form, textBoxFieldName).BackColor;
        }

        private static System.Windows.Forms.TextBox GetTextBox(object form, string fieldName)
        {
            var fieldInfo = form
                .GetType()
                .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            Assert.That(fieldInfo, Is.Not.Null, $"Не найдено поле {fieldName}.");

            var textBox = fieldInfo!.GetValue(form) as System.Windows.Forms.TextBox;
            Assert.That(textBox, Is.Not.Null, $"{fieldName} не является TextBox.");

            return textBox!;
        }
    }
}
