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
    /// Инкапсулирует запуск/подключение KOMPAS и базовые операции
    /// 3D-моделирования.
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
        private ksDocument3D _document3D;

        /// <summary>
        /// Верхняя деталь (part) текущего 3D-документа.
        /// </summary>
        private ksPart _topPart;

        /// <summary>
        /// Запускает KOMPAS, если он ещё не запущен,
        /// или присоединяется к уже работающему экземпляру.
        /// Если ранее полученная COM-ссылка устарела (KOMPAS закрыт),
        /// автоматически переподключается.
        /// </summary>
        public virtual void AttachOrRunCAD()
        {
            if (_kompas != null)
            {
                try
                {
                    var isVisible = _kompas.Visible;
                    return;
                }
                catch (COMException)
                {
                    ReleaseComObject(_kompas);
                    _kompas = null;
                }
                catch
                {
                    ReleaseComObject(_kompas);
                    _kompas = null;
                }
            }

            try
            {
                var progIdType = Type.GetTypeFromProgID("KOMPAS.Application.5");

                if (progIdType == null)
                {
                    throw new InvalidOperationException(
                        "Не удалось получить ProgID KOMPAS.Application.5.");
                }

                var instance = Activator.CreateInstance(progIdType);
                _kompas = (KompasObject)instance;

                _kompas.Visible = true;
                _kompas.ActivateControllerAPI();
            }
            catch (Exception exception)
            {
                ReleaseComObject(_kompas);
                _kompas = null;

                throw new Exception(
                    "Не удалось запустить или подключиться к KOMPAS.",
                    exception);
            }
        }

        /// <summary>
        /// Создаёт новый 3D-документ и получает ссылку на верхний Part.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Если KOMPAS не подключён.
        /// </exception>
        public virtual void CreateDocument3D()
        {
            if (_kompas == null)
            {
                throw new InvalidOperationException(
                    "Компас не подключён.");
            }

            _document3D = (ksDocument3D)_kompas.Document3D();
            _document3D.Create(false, true);

            _topPart = (ksPart)_document3D.GetPart(
                (short)Part_Type.pTop_Part);
        }

        // TODO:+ RSDN
        /// <summary>
        /// Закрывает активный 3D-документ.
        /// После закрытия освобождает COM-объекты документа и детали
        /// (Part).
        /// </summary>
        /// <param name="save">
        /// True — сохранить документ перед закрытием; False — закрыть
        /// без сохранения.
        /// </param>
        public virtual void CloseActiveDocument3D(bool save = false)
        {
            if (_document3D == null)
                return;

            try
            {
                InvokeClose(_document3D, save);
            }
            finally
            {
                ReleaseComObject(_topPart);
                _topPart = null;

                ReleaseComObject(_document3D);
                _document3D = null;
            }
        }

        // TODO:+ RSDN
        /// <summary>
        /// Создаёт цилиндр вдоль оси X между <paramref name="startX"/>
        /// и <paramref name="endX"/>.
        /// Предполагается, что <paramref name="startX"/> ≥ 0 и
        /// <paramref name="endX"/> &gt; <paramref name="startX"/>.
        /// </summary>
        /// <param name="startX">
        /// Начальная координата сегмента по оси X.
        /// </param>
        /// <param name="endX">
        /// Конечная координата сегмента по оси X.
        /// </param>
        /// <param name="diameter">Диаметр сегмента.</param>
        /// <param name="name">
        /// Имя (метка) сегмента в дереве построения.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// Если 3D-документ/деталь не инициализированы.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Если координаты или диаметр некорректны.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Если имя сегмента пустое.
        /// </exception>
        public virtual void CreateCylindricalSegment(
            double startX,
            double endX,
            double diameter,
            string name)
        {
            if (_topPart == null)
            {
                // TODO:+ RSDN
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
                // TODO:+ RSDN
                throw new ArgumentException(
                    "Имя сегмента не может быть пустым.",
                    nameof(name));
            }

            const double MinimumLength = 0.001;

            var length = endX - startX;
            if (length <= MinimumLength)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(length),
                    "Недопустимая длина цилиндра: "
                        + $"{length:0.###} мм.");
            }

            var basePlane = _topPart.GetDefaultEntity(
                (short)Obj3dType.o3d_planeYOZ);

            var offsetPlane = _topPart.NewEntity(
                (short)Obj3dType.o3d_planeOffset);

            var offsetDefinition =
                (ksPlaneOffsetDefinition)offsetPlane.GetDefinition();

            offsetDefinition.SetPlane(basePlane);
            offsetDefinition.direction = true;
            offsetDefinition.offset = startX;

            offsetPlane.Create();

            var sketch = _topPart.NewEntity(
                (short)Obj3dType.o3d_sketch);

            var sketchDefinition =
                (ksSketchDefinition)sketch.GetDefinition();

            sketchDefinition.SetPlane(offsetPlane);
            sketch.Create();

            var document2D =
                (ksDocument2D)sketchDefinition.BeginEdit();

            document2D.ksCircle(0, 0, diameter / 2.0, 1);
            sketchDefinition.EndEdit();

            var extrusion = _topPart.NewEntity(
                (short)Obj3dType.o3d_baseExtrusion);

            var extrusionDefinition =
                (ksBaseExtrusionDefinition)extrusion.GetDefinition();

            extrusionDefinition.directionType =
                (short)Direction_Type.dtNormal;

            extrusionDefinition.SetSketch(sketch);

            extrusionDefinition.SetSideParam(
                true,
                (short)End_Type.etBlind,
                length,
                0,
                false);

            extrusionDefinition.SetThinParam(false, 0, 0, 0);

            extrusion.Create();

            try
            {
                dynamic extrusionEntity = extrusion;
                extrusionEntity.Name = name;
            }
            catch
            {
            }
        }

        // TODO:+ RSDN
        /// <summary>
        /// Вызывает закрытие документа через reflection/dynamic, чтобы
        /// не зависеть от точной сигнатуры метода Close в API.
        /// </summary>
        /// <param name="document">3D-документ KOMPAS.</param>
        /// <param name="save">Признак сохранения.</param>
        private static void InvokeClose(object document, bool save)
        {
            var documentType = document.GetType();

            var closeWithBool =
                documentType.GetMethod(
                    "Close",
                    new[] { typeof(bool) });

            if (closeWithBool != null)
            {
                closeWithBool.Invoke(document, new object[] { save });
                return;
            }

            var closeWithInt =
                documentType.GetMethod(
                    "Close",
                    new[] { typeof(int) });

            if (closeWithInt != null)
            {
                closeWithInt.Invoke(document, new object[] { save ? 1 : 0 });
                return;
            }

            var closeWithShort =
                documentType.GetMethod(
                    "Close",
                    new[] { typeof(short) });

            if (closeWithShort != null)
            {
                closeWithShort.Invoke(
                    document,
                    new object[] { (short)(save ? 1 : 0) });
                return;
            }

            var closeWithoutArgs =
                documentType.GetMethod(
                    "Close",
                    Type.EmptyTypes);

            if (closeWithoutArgs != null)
            {
                closeWithoutArgs.Invoke(document, null);
                return;
            }

            dynamic dynamicDocument = document;

            try
            {
                dynamicDocument.Close(save);
            }
            catch
            {
                try
                {
                    dynamicDocument.Close();
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Освобождает COM-объект (FinalReleaseComObject).
        /// </summary>
        /// <param name="comObject">COM-объект.</param>
        private static void ReleaseComObject(object comObject)
        {
            if (comObject == null)
                return;

            if (Marshal.IsComObject(comObject))
            {
                try
                {
                    Marshal.FinalReleaseComObject(comObject);
                }
                catch
                {
                }
            }
        }
    }
}
