using System.Collections.Generic;
using BarbellBarPlugin.Kompas;

namespace BarbellBarPlugin.Tests
{
    /// <summary>
    /// Фейковый враппер для тестов.
    /// Запоминает все созданные сегменты в списке.
    /// </summary>
    public class FakeKompasWrapper : Wrapper
    {
        public record Segment(double StartX, double EndX, double Diameter, string Name);

        public List<Segment> Segments { get; } = new();

        public bool AttachCalled { get; private set; }
        public bool CreateDocCalled { get; private set; }

        public override void AttachOrRunCAD()
        {
            AttachCalled = true;
        }

        public override void CreateDocument3D()
        {
            CreateDocCalled = true;
        }

        public override void CreateCylindricalSegment(double startX, double endX, double diameter, string name)
        {
            Segments.Add(new Segment(startX, endX, diameter, name));
        }
    }
}
