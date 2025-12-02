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
        /// Флаг, показывающий, что был вызван CreateDocument3D.
        /// </summary>
        public bool CreateDocCalled { get; private set; }

        /// <inheritdoc />
        public override void AttachOrRunCAD()
        {
            AttachCalled = true;
        }

        /// <inheritdoc />
        public override void CreateDocument3D()
        {
            CreateDocCalled = true;
        }

        /// <inheritdoc />
        public override void CreateCylindricalSegment(double startX, double endX, double diameter, string name)
        {
            Segments.Add(new Segment(startX, endX, diameter, name));
        }
    }
}
