using System;
using BarbellBarPlugin.Model;

namespace BarbellBarPlugin.Kompas
{
    /// <summary>
    /// Оркестратор построения 3D-модели грифа в KOMPAS 3D.
    /// Разбивает гриф на пять цилиндрических сегментов по оси X и передаёт их обёртке KOMPAS.
    /// </summary>
    public class BarBuilder
    {
        /// <summary>
        /// Обёртка над KOMPAS API, выполняющая построение сегментов и операции с документом.
        /// </summary>
        private readonly Wrapper _wrapper;

        /// <summary>
        /// Признак того, что подключение к КОМПАС уже выполнено.
        /// </summary>
        private bool _isCadAttached;

        /// <summary>
        /// Параметры грифа, использованные при последнем построении.
        /// </summary>
        private BarParameters _parameters = null!;

        /// <summary>
        /// Создаёт новый экземпляр <see cref="BarBuilder"/>.
        /// </summary>
        /// <param name="wrapper">Объект, инкапсулирующий вызовы KOMPAS API.</param>
        /// //TODO: RSDN
        /// <exception cref="ArgumentNullException">Если <paramref name="wrapper"/> равен null.</exception>
        public BarBuilder(Wrapper wrapper)
        {
            _wrapper = wrapper ?? throw new ArgumentNullException(nameof(wrapper));
        }

        /// <summary>
        /// Параметры грифа, использованные при последнем построении.
        /// </summary>
        public BarParameters CurrentParameters => _parameters;

        /// <summary>
        /// Выполняет построение 3D-модели грифа.
        /// По умолчанию документ остаётся открытым (режим работы с пользователем).
        /// Для нагрузочного тестирования можно включить закрытие документа после построения.
        /// </summary>
        /// <param name="parameters">Параметры грифа.</param>
        /// //TODO: RSDN
        /// <param name="closeDocumentAfterBuild">True — закрыть документ после построения; False — оставить документ открытым.</param>
        /// <exception cref="ArgumentNullException">Если <paramref name="parameters"/> равен null.</exception>
        public void Build(BarParameters parameters, bool closeDocumentAfterBuild = false)
        {
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));

            if (!_isCadAttached)
            {
                _wrapper.AttachOrRunCAD();
                _isCadAttached = true;
            }

            _wrapper.CreateDocument3D();

            try
            {
                BuildBar();
            }
            finally
            {
                if (closeDocumentAfterBuild)
                {
                    _wrapper.CloseActiveDocument3D(save: false);
                }
            }
        }

        /// <summary>
        /// Строит гриф вдоль оси X.
        /// Все координаты сегментов рассчитываются от нуля.
        /// </summary>
        private void BuildBar()
        {
            //TODO: RSDN
            double Ls = _parameters.SleeveLength;
            double Ld = _parameters.SeparatorLength;
            double Lh = _parameters.HandleLength;

            double handleDiameter =
                Math.Min(_parameters.SleeveDiameter, _parameters.SeparatorDiameter) - 3.0;

            //TODO: {}
            if (handleDiameter <= 0)
                handleDiameter = _parameters.SeparatorDiameter * 0.8;

            double leftSleeveStart = 0.0;
            double leftSleeveEnd = Ls;

            double leftSepStart = leftSleeveEnd;
            double leftSepEnd = leftSepStart + Ld;

            double handleStart = leftSepEnd;
            double handleEnd = handleStart + Lh;

            double rightSepStart = handleEnd;
            double rightSepEnd = rightSepStart + Ld;

            double rightSleeveStart = rightSepEnd;
            double rightSleeveEnd = rightSleeveStart + Ls;

            _wrapper.CreateCylindricalSegment(
                leftSleeveStart,
                leftSleeveEnd,
                _parameters.SleeveDiameter,
                "LeftSleeve");

            _wrapper.CreateCylindricalSegment(
                leftSepStart,
                leftSepEnd,
                _parameters.SeparatorDiameter,
                "LeftSeparator");

            _wrapper.CreateCylindricalSegment(
                handleStart,
                handleEnd,
                handleDiameter,
                "Handle");

            _wrapper.CreateCylindricalSegment(
                rightSepStart,
                rightSepEnd,
                _parameters.SeparatorDiameter,
                "RightSeparator");

            _wrapper.CreateCylindricalSegment(
                rightSleeveStart,
                rightSleeveEnd,
                _parameters.SleeveDiameter,
                "RightSleeve");
        }
    }
}
