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
        [Test]
        [Description("Проверяет, что нажатие кнопки мужского пресета заполняет поля ожидаемыми значениями.")]
        public void MalePresetButton_Click_FillsTextBoxes_WithMalePresetValues()
        {
            var form = new BarbellBarPlugin.BarbelBarPlugin();

            SetText(form, "DiametrSleeveTextBox", "999");
            SetText(form, "LengthSeparatorTextBox", "999");
            SetText(form, "LengthHandleTextBox", "999");
            SetText(form, "DiametrSeparatorTextBox", "999");
            SetText(form, "LengthSleeveTextBox", "999");

            InvokeClick(form, "MalePresetButton_Click");

            Assert.Multiple(() =>
            {
                Assert.That(GetText(form, "DiametrSleeveTextBox"), Is.EqualTo("30"));
                Assert.That(GetText(form, "LengthSeparatorTextBox"), Is.EqualTo("50"));
                Assert.That(GetText(form, "LengthHandleTextBox"), Is.EqualTo("1250"));
                Assert.That(GetText(form, "DiametrSeparatorTextBox"), Is.EqualTo("40"));
                Assert.That(GetText(form, "LengthSleeveTextBox"), Is.EqualTo("350"));
            });
        }

        [Test]
        [Description("Проверяет, что нажатие кнопки женского пресета заполняет поля ожидаемыми значениями.")]
        public void FemalePresetButton_Click_FillsTextBoxes_WithFemalePresetValues()
        {
            var form = new BarbellBarPlugin.BarbelBarPlugin();

            SetText(form, "DiametrSleeveTextBox", "999");
            SetText(form, "LengthSeparatorTextBox", "999");
            SetText(form, "LengthHandleTextBox", "999");
            SetText(form, "DiametrSeparatorTextBox", "999");
            SetText(form, "LengthSleeveTextBox", "999");

            InvokeClick(form, "FemalePresetButton_Click");

            Assert.Multiple(() =>
            {
                Assert.That(GetText(form, "DiametrSleeveTextBox"), Is.EqualTo("30"));
                Assert.That(GetText(form, "LengthSeparatorTextBox"), Is.EqualTo("50"));
                Assert.That(GetText(form, "LengthHandleTextBox"), Is.EqualTo("1200"));
                Assert.That(GetText(form, "DiametrSeparatorTextBox"), Is.EqualTo("40"));
                Assert.That(GetText(form, "LengthSleeveTextBox"), Is.EqualTo("320"));
            });
        }

        [Test]
        [Description("Проверяет, что нажатие кнопки пресета сбрасывает визуальную валидацию (BackColor становится White).")]
        public void PresetButton_Click_ClearsValidation_BackColorToWhite()
        {
            var form = new BarbellBarPlugin.BarbelBarPlugin();

            SetBackColor(form, "DiametrSleeveTextBox", Color.MistyRose);
            SetBackColor(form, "LengthSeparatorTextBox", Color.MistyRose);
            SetBackColor(form, "LengthHandleTextBox", Color.MistyRose);
            SetBackColor(form, "DiametrSeparatorTextBox", Color.MistyRose);
            SetBackColor(form, "LengthSleeveTextBox", Color.MistyRose);

            InvokeClick(form, "MalePresetButton_Click");

            Assert.Multiple(() =>
            {
                Assert.That(GetBackColor(form, "DiametrSleeveTextBox"), Is.EqualTo(Color.White));
                Assert.That(GetBackColor(form, "LengthSeparatorTextBox"), Is.EqualTo(Color.White));
                Assert.That(GetBackColor(form, "LengthHandleTextBox"), Is.EqualTo(Color.White));
                Assert.That(GetBackColor(form, "DiametrSeparatorTextBox"), Is.EqualTo(Color.White));
                Assert.That(GetBackColor(form, "LengthSleeveTextBox"), Is.EqualTo(Color.White));
            });
        }

        private static void InvokeClick(object form, string handlerName)
        {
            var mi = form.GetType().GetMethod(handlerName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(mi, Is.Not.Null, $"Не найден обработчик {handlerName}.");

            mi!.Invoke(form, new object[] { null!, System.EventArgs.Empty });
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
            var fi = form.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(fi, Is.Not.Null, $"Не найдено поле {fieldName}.");

            var tb = fi!.GetValue(form) as System.Windows.Forms.TextBox;
            Assert.That(tb, Is.Not.Null, $"{fieldName} не является TextBox.");

            return tb!;
        }
    }
}
