using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using Kompas6API5;
using Kompas6Constants;
using Kompas6Constants3D;

namespace BarbellBarPlugin.Kompas
{
    /// <summary>
    /// Обёртка для работы с KOMPAS API5.
    /// Инкапсулирует запуск/подключение KOMPAS и базовые операции 3D-моделирования.
    /// </summary>
    [ExcludeFromCodeCoverage]
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
            {
                return;
            }

            try
            {
                Type type = Type.GetTypeFromProgID("KOMPAS.Application.5");
                if (type == null)
                {
                    throw new InvalidOperationException(
                        "Не удалось получить ProgID KOMPAS.Application.5.");
                }

                object instance = Activator.CreateInstance(type);
                _kompas = (KompasObject)instance;

                _kompas.Visible = true;
                _kompas.ActivateControllerAPI();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Не удалось запустить или подключиться к KOMPAS.",
                    ex);
            }
        }

        /// <summary>
        /// Создаёт новый 3D-документ и получает ссылку на верхний Part.
        /// </summary>
        /// <exception cref="InvalidOperationException">Если KOMPAS не подключён.</exception>
        public virtual void CreateDocument3D()
        {
            if (_kompas == null)
            {
                throw new InvalidOperationException("Компас не подключён.");
            }

            _doc3D = (ksDocument3D)_kompas.Document3D();
            _doc3D.Create(false, true);

            _part = (ksPart)_doc3D.GetPart((short)Part_Type.pTop_Part);
        }

        /// <summary>
        /// Закрывает активный 3D-документ.
        /// После закрытия освобождает COM-объекты документа и детали (Part).
        /// </summary>
        /// //TODO: RSDN+
        /// <param name="save">True — сохранить документ перед закрытием; False — закрыть без сохранения.</param>
        public virtual void CloseActiveDocument3D(bool save = false)
        {
            if (_doc3D == null)
            {
                return;
            }

            try
            {
                InvokeClose(_doc3D, save);
            }
            finally
            {
                ReleaseComObject(_part);
                _part = null;

                ReleaseComObject(_doc3D);
                _doc3D = null;
            }
        }

        //TODO: RSDN+
        /// <summary>
        /// Создаёт цилиндр вдоль оси X между <paramref name="startX"/> и <paramref name="endX"/>.
        /// Предполагается, что <paramref name="startX"/> ≥ 0 и <paramref name="endX"/> &gt; <paramref name="startX"/>.
        /// </summary>
        /// <param name="startX">Начальная координата сегмента по оси X.</param>
        /// <param name="endX">Конечная координата сегмента по оси X.</param>
        /// <param name="diameter">Диаметр сегмента.</param>
        /// <param name="name">Имя (метка) сегмента в дереве построения.</param>
        /// <exception cref="InvalidOperationException">Если 3D-документ/деталь не инициализированы.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Если координаты или диаметр некорректны.</exception>
        /// <exception cref="ArgumentException">Если имя сегмента пустое.</exception>
        public virtual void CreateCylindricalSegment(
            double startX,
            double endX,
            double diameter,
            string name)
        {
            if (_part == null)
            {
                //TODO: RSDN+
                throw new InvalidOperationException(
                    "Часть не инициализирована. Вызовите CreateDocument3D().");
            }

            if (startX < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(startX),
                    "Начальная координата startX не может быть меньше нуля.");
            }

            if (endX <= startX)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(endX),
                    "Конечная координата endX должна быть больше startX.");
            }

            if (diameter <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(diameter),
                    "Диаметр цилиндра должен быть положительным.");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                //TODO: RSDN+
                throw new ArgumentException(
                    "Имя сегмента не может быть пустым.",
                    nameof(name));
            }

            const double MinLength = 0.001;

            double length = endX - startX;
            if (length <= MinLength)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(length),
                    $"Недопустимая длина цилиндра: {length:0.###} мм.");
            }

            ksEntity basePlane =
                _part.GetDefaultEntity((short)Obj3dType.o3d_planeYOZ);

            ksEntity offsetPlane =
                _part.NewEntity((short)Obj3dType.o3d_planeOffset);

            ksPlaneOffsetDefinition offsetDef =
                (ksPlaneOffsetDefinition)offsetPlane.GetDefinition();

            offsetDef.SetPlane(basePlane);
            offsetDef.direction = true;
            offsetDef.offset = startX;

            offsetPlane.Create();

            ksEntity sketch =
                _part.NewEntity((short)Obj3dType.o3d_sketch);

            ksSketchDefinition sketchDef =
                (ksSketchDefinition)sketch.GetDefinition();

            sketchDef.SetPlane(offsetPlane);
            sketch.Create();

            ksDocument2D doc2D =
                (ksDocument2D)sketchDef.BeginEdit();

            doc2D.ksCircle(0, 0, diameter / 2.0, 1);
            sketchDef.EndEdit();

            ksEntity extrude =
                _part.NewEntity((short)Obj3dType.o3d_baseExtrusion);

            ksBaseExtrusionDefinition extrudeDef =
                (ksBaseExtrusionDefinition)extrude.GetDefinition();

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

            try
            {
                dynamic extrudeDynamic = extrude;
                extrudeDynamic.Name = name;
            }
            catch
            {
            }
        }

        /// <summary>
        /// //TODO: RSDN+
        /// Вызывает закрытие документа через reflection/dynamic, чтобы не зависеть от точной сигнатуры метода Close в API.
        /// </summary>
        /// <param name="doc">3D-документ KOMPAS.</param>
        /// <param name="save">Признак сохранения.</param>
        private static void InvokeClose(object doc, bool save)
        {
            Type type = doc.GetType();

            MethodInfo closeBool =
                type.GetMethod("Close", new[] { typeof(bool) });

            if (closeBool != null)
            {
                closeBool.Invoke(doc, new object[] { save });
                return;
            }

            MethodInfo closeInt =
                type.GetMethod("Close", new[] { typeof(int) });

            if (closeInt != null)
            {
                closeInt.Invoke(doc, new object[] { save ? 1 : 0 });
                return;
            }

            MethodInfo closeShort =
                type.GetMethod("Close", new[] { typeof(short) });

            if (closeShort != null)
            {
                closeShort.Invoke(doc, new object[] { (short)(save ? 1 : 0) });
                return;
            }

            MethodInfo closeNoArgs =
                type.GetMethod("Close", Type.EmptyTypes);

            if (closeNoArgs != null)
            {
                closeNoArgs.Invoke(doc, null);
                return;
            }

            dynamic d = doc;

            try
            {
                d.Close(save);
            }
            catch
            {
                try
                {
                    d.Close();
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Освобождает COM-объект (FinalReleaseComObject).
        /// </summary>
        /// <param name="obj">COM-объект.</param>
        private static void ReleaseComObject(object obj)
        {
            if (obj == null)
            {
                return;
            }

            if (Marshal.IsComObject(obj))
            {
                try
                {
                    Marshal.FinalReleaseComObject(obj);
                }
                catch
                {
                }
            }
        }
    }
}
