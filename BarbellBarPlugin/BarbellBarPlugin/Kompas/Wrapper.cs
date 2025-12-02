using System;
using Kompas6API5;
using Kompas6Constants;
using Kompas6Constants3D;

namespace BarbellBarPlugin.Kompas
{
    /// <summary>
    /// Обёртка для работы с KOMPAS API5.
    /// </summary>
    public class Wrapper
    {
        //TODO: XML
        private KompasObject _kompas;
        private ksDocument3D _doc3D;
        private ksPart _part;

        /// <summary>
        /// Запускает или присоединяется к Компасу.
        /// </summary>
        public virtual void AttachOrRunCAD()
        {
            if (_kompas != null)
                return;

            try
            {
                Type t = Type.GetTypeFromProgID("KOMPAS.Application.5");
                _kompas = (KompasObject)Activator.CreateInstance(t);
                _kompas.Visible = true;
                _kompas.ActivateControllerAPI();
            }
            catch
            {
                throw new Exception("Не удалось запустить или подключиться к KOMPAS.");
            }
        }

        /// <summary>
        /// Создает новый 3D-документ.
        /// </summary>
        public virtual void CreateDocument3D()
        {
            if (_kompas == null)
                throw new Exception("Компас не подключён.");

            _doc3D = (ksDocument3D)_kompas.Document3D();
            _doc3D.Create(false, true);
            _part = (ksPart)_doc3D.GetPart((short)Part_Type.pTop_Part);
        }

        /// <summary>
        /// Создаёт цилиндр вдоль оси X между startX и endX (startX >= 0, endX > startX).
        /// </summary>
        /// //TODO: RSDN
        public virtual void CreateCylindricalSegment(double startX, double endX, double diameter, string name)
        {
            //TODO: refactor
            double length = endX - startX;
            if (length <= 0.001)
                //TODO: refactor
                throw new Exception("Нулевая или отрицательная длина цилиндра.");

            // 1. Базовая плоскость YOZ
            ksEntity basePlane = _part.GetDefaultEntity((short)Obj3dType.o3d_planeYOZ);

            // 2. Смещённая плоскость на расстояние startX вдоль X
            ksEntity offsetPlane = _part.NewEntity((short)Obj3dType.o3d_planeOffset);
            var offsetDef = (ksPlaneOffsetDefinition)offsetPlane.GetDefinition();
            offsetDef.SetPlane(basePlane);
            offsetDef.direction = true;      
            offsetDef.offset = startX;       
            offsetPlane.Create();

            // 3. Эскиз окружности на этой плоскости
            ksEntity sketch = _part.NewEntity((short)Obj3dType.o3d_sketch);
            var sketchDef = (ksSketchDefinition)sketch.GetDefinition();
            sketchDef.SetPlane(offsetPlane);
            sketch.Create();

            ksDocument2D doc2D = (ksDocument2D)sketchDef.BeginEdit();
            doc2D.ksCircle(0, 0, diameter / 2.0, 1);
            sketchDef.EndEdit();

            // 4. Выдавливание вдоль +X
            ksEntity extrude = _part.NewEntity((short)Obj3dType.o3d_baseExtrusion);
            var extrudeDef = (ksBaseExtrusionDefinition)extrude.GetDefinition();

            extrudeDef.directionType = (short)Direction_Type.dtNormal;
            extrudeDef.SetSketch(sketch);
            extrudeDef.SetSideParam(
                true,
                (short)End_Type.etBlind,
                length,
                0,
                false);
            extrudeDef.SetThinParam(false, 0, 0, 0);

            extrude.Create();
        }
    }
}
