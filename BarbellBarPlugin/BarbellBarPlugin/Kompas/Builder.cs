// Builder.cs
using System;
using BarbellBarPlugin.Core.Model;

namespace BarbellBarPlugin.Kompas
{
    /// <summary>
    /// Оркестратор построения 3D-модели грифа в KOMPAS 3D.
    /// Разбивает гриф на пять цилиндрических сегментов по оси X и
    /// передаёт их обёртке KOMPAS.
    /// </summary>
    public class Builder
    {
        /// <summary>
        /// Обёртка над KOMPAS API, выполняющая построение сегментов и
        /// операции с документом.
        /// </summary>
        private readonly Wrapper _wrapper;

        /// <summary>
        /// Параметры грифа, использованные при последнем построении.
        /// </summary>
        private BarbellBarParameters _parameters = null!;

        // TODO:+ RSDN
        /// <summary>
        /// Создаёт новый экземпляр <see cref="Builder"/>.
        /// </summary>
        /// <param name="wrapper">
        /// Объект, инкапсулирующий вызовы KOMPAS API.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Если <paramref name="wrapper"/> равен null.
        /// </exception>
        public Builder(Wrapper wrapper)
        {
            if (wrapper == null)
            {
                throw new ArgumentNullException(nameof(wrapper));
            }

            _wrapper = wrapper;
        }

        /// <summary>
        /// Параметры грифа, использованные при последнем построении.
        /// </summary>
        public BarbellBarParameters CurrentParameters
        {
            get { return _parameters; }
        }

        // TODO:+ RSDN
        /// <summary>
        /// Выполняет построение 3D-модели грифа.
        /// По умолчанию документ остаётся открытым (режим работы с
        /// пользователем).
        /// Для нагрузочного тестирования можно включить закрытие
        /// документа после построения.
        /// </summary>
        /// <param name="parameters">Параметры грифа.</param>
        /// <param name="closeDocumentAfterBuild">
        /// True — закрыть документ после построения; False — оставить
        /// документ открытым.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Если <paramref name="parameters"/> равен null.
        /// </exception>
        public void Build(
            BarbellBarParameters parameters,
            bool closeDocumentAfterBuild = false)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            _parameters = parameters;

            _wrapper.AttachOrRunCAD();
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
            // TODO:+ RSDN
            var sleeveLength = _parameters.SleeveLength;
            var separatorLength = _parameters.SeparatorLength;
            var handleLength = _parameters.HandleLength;

            var sleeveDiameter = _parameters.SleeveDiameter;
            var separatorDiameter = _parameters.SeparatorDiameter;

            var handleDiameter =
                Math.Min(sleeveDiameter, separatorDiameter) - 3.0;

            // TODO:+ {}+
            if (handleDiameter <= 0.0)
            {
                handleDiameter = separatorDiameter * 0.8;
            }

            var leftSleeveStart = 0.0;
            var leftSleeveEnd = sleeveLength;

            var leftSeparatorStart = leftSleeveEnd;
            var leftSeparatorEnd = leftSeparatorStart + separatorLength;

            var handleStart = leftSeparatorEnd;
            var handleEnd = handleStart + handleLength;

            var rightSeparatorStart = handleEnd;
            var rightSeparatorEnd = rightSeparatorStart + separatorLength;

            var rightSleeveStart = rightSeparatorEnd;
            var rightSleeveEnd = rightSleeveStart + sleeveLength;

            _wrapper.CreateCylindricalSegment(
                leftSleeveStart,
                leftSleeveEnd,
                sleeveDiameter,
                "LeftSleeve");

            _wrapper.CreateCylindricalSegment(
                leftSeparatorStart,
                leftSeparatorEnd,
                separatorDiameter,
                "LeftSeparator");

            _wrapper.CreateCylindricalSegment(
                handleStart,
                handleEnd,
                handleDiameter,
                "Handle");

            _wrapper.CreateCylindricalSegment(
                rightSeparatorStart,
                rightSeparatorEnd,
                separatorDiameter,
                "RightSeparator");

            _wrapper.CreateCylindricalSegment(
                rightSleeveStart,
                rightSleeveEnd,
                sleeveDiameter,
                "RightSleeve");
        }
    }
}
