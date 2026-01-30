using System.Collections.Generic;

using BarbellBarPlugin.Kompas;

namespace BarbellBarPlugin.Tests
{
    /// <summary>
    /// Тестовая реализация обёртки KOMPAS.
    /// Наследуется от <see cref="Wrapper"/> и переопределяет методы так,
    /// чтобы не вызывать реальный KOMPAS, а только логировать вызовы.
    /// </summary>
    public class FakeKompasWrapper : Wrapper
    {
        /// <summary>
        /// Описание построенного цилиндрического сегмента грифа.
        /// </summary>
        /// <param name="StartX">Начальная координата по оси X.</param>
        /// <param name="EndX">Конечная координата по оси X.</param>
        /// <param name="Diameter">Диаметр цилиндра.</param>
        /// <param name="Name">Логическое имя сегмента (ручка, посадка и т.п.).</param>
        public record Segment(double StartX, double EndX, double Diameter, string Name);

        /// <summary>
        /// Коллекция всех сегментов, построенных в ходе теста.
        /// </summary>
        public List<Segment> Segments { get; } = new();

        /// <summary>
        /// Флаг, показывающий, что был вызван AttachOrRunCAD.
        /// </summary>
        public bool AttachCalled { get; private set; }

        /// <summary>
        /// Количество вызовов AttachOrRunCAD.
        /// </summary>
        public int AttachCallCount { get; private set; }

        /// <summary>
        /// Флаг, показывающий, что был вызван CreateDocument3D.
        /// </summary>
        public bool CreateDocCalled { get; private set; }

        /// <summary>
        /// Количество вызовов CreateDocument3D.
        /// </summary>
        public int CreateDocCallCount { get; private set; }

        /// <summary>
        /// Флаг, показывающий, что был вызван CloseActiveDocument3D.
        /// </summary>
        public bool CloseDocCalled { get; private set; }

        /// <summary>
        /// Количество вызовов CloseActiveDocument3D.
        /// </summary>
        public int CloseDocCallCount { get; private set; }

        /// <summary>
        /// Значение параметра save, с которым был вызван CloseActiveDocument3D.
        /// </summary>
        public bool? CloseDocSaveArg { get; private set; }

        /// <inheritdoc />
        public override void AttachOrRunCAD()
        {
            AttachCalled = true;
            AttachCallCount++;
        }

        /// <inheritdoc />
        public override void CreateDocument3D()
        {
            CreateDocCalled = true;
            CreateDocCallCount++;
        }

        /// <inheritdoc />
        public override void CloseActiveDocument3D(bool save = false)
        {
            CloseDocCalled = true;
            CloseDocCallCount++;
            CloseDocSaveArg = save;
        }

        /// <summary>
        /// Логирует параметры создаваемого цилиндрического сегмента,
        /// добавляя их в коллекцию <see cref="Segments"/> вместо реального построения в KOMPAS.
        /// </summary>
        //TODO: RSDN
        public override void CreateCylindricalSegment(
            double startX,
            double endX,
            double diameter,
            string name)
        {
            Segments.Add(new Segment(startX, endX, diameter, name));
        }

        /// <summary>
        /// Утилита для тестов: сбросить флаги/счётчики/сегменты между вызовами Build, если надо.
        /// </summary>
        public void Reset()
        {
            Segments.Clear();

            AttachCalled = false;
            CreateDocCalled = false;
            CloseDocCalled = false;

            AttachCallCount = 0;
            CreateDocCallCount = 0;
            CloseDocCallCount = 0;

            CloseDocSaveArg = null;
        }
    }
}
