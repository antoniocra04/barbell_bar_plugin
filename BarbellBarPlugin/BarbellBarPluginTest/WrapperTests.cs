using System;
using System.Reflection;
using BarbellBarPlugin.Kompas;
using NUnit.Framework;

namespace BarbellBarPlugin.Tests
{
    [TestFixture]
    public class WrapperTests
    {
        [Test]
        [Description("CloseActiveDocument3D: если активного документа нет (_doc3D == null), метод просто возвращается.")]
        public void CloseActiveDocument3D_DoesNothing_WhenDocIsNull()
        {
            var w = new Wrapper();
            Assert.DoesNotThrow(() => w.CloseActiveDocument3D(save: false));
        }

        [Test]
        [Description("AttachOrRunCAD: если _kompas уже установлен (не null), метод возвращается без попытки создать COM.")]
        public void AttachOrRunCAD_Returns_WhenKompasAlreadySet()
        {
            var w = new Wrapper();

            // подсовываем любое ненулевое значение в приватное поле _kompas
            var kompasField = typeof(Wrapper).GetField("_kompas", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(kompasField, Is.Not.Null);

            kompasField!.SetValue(w, new object()); // лишь бы не null

            // должно уйти по раннему return и не упасть
            Assert.DoesNotThrow(() => w.AttachOrRunCAD());
        }

        [Test]
        [Description("CreateDocument3D: если _kompas == null, выбрасывает InvalidOperationException.")]
        public void CreateDocument3D_Throws_WhenKompasNotAttached()
        {
            var w = new Wrapper();
            Assert.Throws<InvalidOperationException>(() => w.CreateDocument3D());
        }

        [Test]
        [Description("CreateCylindricalSegment: если _part == null, выбрасывает InvalidOperationException.")]
        public void CreateCylindricalSegment_Throws_WhenPartNotInitialized()
        {
            var w = new Wrapper();
            Assert.Throws<InvalidOperationException>(() =>
                w.CreateCylindricalSegment(0, 10, 30, "Seg"));
        }

        [Test]
        [Description("CreateCylindricalSegment: startX < 0 => ArgumentOutOfRangeException.")]
        public void CreateCylindricalSegment_Throws_WhenStartXNegative()
        {
            var w = new Wrapper();
            SetPrivateField(w, "_part", new object()); // подставляем заглушку, чтобы пройти первый if

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                w.CreateCylindricalSegment(-1, 10, 30, "Seg"));
        }

        [Test]
        [Description("CreateCylindricalSegment: endX <= startX => ArgumentOutOfRangeException.")]
        public void CreateCylindricalSegment_Throws_WhenEndXNotGreaterThanStartX()
        {
            var w = new Wrapper();
            SetPrivateField(w, "_part", new object());

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                w.CreateCylindricalSegment(10, 10, 30, "Seg"));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                w.CreateCylindricalSegment(10, 9, 30, "Seg"));
        }

        [Test]
        [Description("CreateCylindricalSegment: diameter <= 0 => ArgumentOutOfRangeException.")]
        public void CreateCylindricalSegment_Throws_WhenDiameterNonPositive()
        {
            var w = new Wrapper();
            SetPrivateField(w, "_part", new object());

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                w.CreateCylindricalSegment(0, 10, 0, "Seg"));

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                w.CreateCylindricalSegment(0, 10, -1, "Seg"));
        }

        [Test]
        [Description("CreateCylindricalSegment: name пустой => ArgumentException.")]
        public void CreateCylindricalSegment_Throws_WhenNameEmpty()
        {
            var w = new Wrapper();
            SetPrivateField(w, "_part", new object());

            Assert.Throws<ArgumentException>(() =>
                w.CreateCylindricalSegment(0, 10, 30, ""));

            Assert.Throws<ArgumentException>(() =>
                w.CreateCylindricalSegment(0, 10, 30, "   "));
        }

        [Test]
        [Description("CreateCylindricalSegment: длина <= 0.001 => ArgumentOutOfRangeException.")]
        public void CreateCylindricalSegment_Throws_WhenLengthTooSmall()
        {
            var w = new Wrapper();
            SetPrivateField(w, "_part", new object());

            // length = 0.001 => должно упасть (<= MinLength)
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                w.CreateCylindricalSegment(0, 0.001, 30, "Seg"));

            // length = 0.0005 => тоже упасть
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                w.CreateCylindricalSegment(0, 0.0005, 30, "Seg"));
        }

        [Test]
        [Description("InvokeClose: покрываем ветки поиска Close(...) через reflection вызов private метода на dummy-объекте.")]
        public void InvokeClose_UsesCloseBool_WhenAvailable()
        {
            var doc = new DummyDocBool();

            var mi = typeof(Wrapper).GetMethod("InvokeClose", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(mi, Is.Not.Null);

            mi!.Invoke(null, new object[] { doc, true });

            Assert.That(doc.Called, Is.True);
            Assert.That(doc.SaveArg, Is.True);
        }

        [Test]
        [Description("InvokeClose: если Close(bool) нет, но есть Close() без аргументов — используется он.")]
        public void InvokeClose_UsesCloseNoArgs_WhenBoolNotAvailable()
        {
            var doc = new DummyDocNoArgs();

            var mi = typeof(Wrapper).GetMethod("InvokeClose", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(mi, Is.Not.Null);

            mi!.Invoke(null, new object[] { doc, true });

            Assert.That(doc.Called, Is.True);
        }

        // -------- helpers --------

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var fi = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(fi, Is.Not.Null, $"Не найдено поле {fieldName}.");
            fi!.SetValue(obj, value);
        }

        private sealed class DummyDocBool
        {
            public bool Called { get; private set; }
            public bool SaveArg { get; private set; }

            public void Close(bool save)
            {
                Called = true;
                SaveArg = save;
            }
        }

        private sealed class DummyDocNoArgs
        {
            public bool Called { get; private set; }

            public void Close()
            {
                Called = true;
            }
        }
    }
}
