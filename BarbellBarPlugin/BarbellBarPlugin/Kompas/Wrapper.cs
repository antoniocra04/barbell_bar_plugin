using System;
using Kompas6API5;
using Kompas6Constants;
using Kompas6Constants3D;

namespace BarbellBarPlugin.Kompas
{
    /// <summary>
    /// Обёртка для работы с KOMPAS API5.
    /// Инкапсулирует запуск/подключение KOMPAS и базовые операции 3D-моделирования.
    /// </summary>
    public class Wrapper
    {
        /// <summary>
        /// Экземпляр приложения KOMPAS.
        /// </summary>
        private KompasObject _kompas;

        /// <summary>
        /// Текущий трёхмерный документ.
        /// </summary>
        private ksDocument3D _doc3D;

        /// <summary>
        /// Верхняя деталь (part) текущего 3D-документа.
        /// </summary>
        private ksPart _part;

        /// <summary>
        /// Запускает KOMPAS, если он ещё не запущен,
        /// или присоединяется к уже работающему экземпляру.
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
        /// Создаёт новый 3D-документ и получает ссылку на верхний Part.
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
        /// Создаёт цилиндр вдоль оси X между <paramref name="startX"/> и <paramref name="endX"/>.
        /// Предполагается, что <paramref name="startX"/> ≥ 0 и <paramref name="endX"/> &gt; <paramref name="startX"/>.
        /// </summary>
        //TODO: RSDN
        public virtual void CreateCylindricalSegment(double startX, double endX, double diameter, string name)
        {
            // Вычисляем длину и проверяем на минимально допустимое значение.
            //TODO: RSDN
            const double MinLength = 0.001;

            double length = endX - startX;
            if (length <= MinLength)
                throw new Exception($"Недопустимая длина цилиндра: {length:0.###} мм.");

            // 1. Базовая плоскость YOZ
            ksEntity basePlane = _part.GetDefaultEntity((short)Obj3dType.o3d_planeYOZ);

            // 2. Смещённая плоскость на расстояние startX вдоль X
            ksEntity offsetPlane = _part.NewEntity((short)Obj3dType.o3d_planeOffset);
            var offsetDef = (ksPlaneOffsetDefinition)offsetPlane.GetDefinition();
            offsetDef.SetPlane(basePlane);
            offsetDef.direction = true;     
            offsetDef.offset = startX;       
            offsetPlane.Create();

            // 3. Эскиз окружности на смещённой плоскости
            ksEntity sketch = _part.NewEntity((short)Obj3dType.o3d_sketch);
            var sketchDef = (ksSketchDefinition)sketch.GetDefinition();
            sketchDef.SetPlane(offsetPlane);
            sketch.Create();

            ksDocument2D doc2D = (ksDocument2D)sketchDef.BeginEdit();
            doc2D.ksCircle(0, 0, diameter / 2.0, 1);
            sketchDef.EndEdit();

            // 4. Выдавливание вдоль +X на длину сегмента
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
