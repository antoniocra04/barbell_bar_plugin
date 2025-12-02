using System;
using BarbellBarPlugin.Model;

namespace BarbellBarPlugin.Kompas
{
    /// <summary>
    /// Оркестратор построения 3D-модели грифа в KOMPAS 3D.
    /// Разбивает гриф на пять цилиндрических сегментов по оси X и отдаёт их обёртке KOMPAS.
    /// </summary>
    public class BarBuilder
    {
        //TODO:+ XML
        /// <summary>
        /// Обёртка над KOMPAS API, выполняющая построение сегментов.
        /// </summary>
        private readonly Wrapper _wrapper;

        //TODO:+ XML
        /// <summary>
        /// Параметры грифа, использованные при последнем построении.
        /// </summary>
        private BarParameters _parameters = null!;

        //TODO:+ XML
        /// <summary>
        /// Создаёт новый экземпляр <see cref="BarBuilder"/>.
        /// </summary>
        /// <param name="wrapper">Объект, инкапсулирующий вызовы KOMPAS API.</param>
        /// <exception cref="ArgumentNullException">Если <paramref name="wrapper"/> равен null.</exception>
        public BarBuilder(Wrapper wrapper)
        {
            _wrapper = wrapper ?? throw new ArgumentNullException(nameof(wrapper));
        }

        //TODO:+ XML
        /// <summary>
        /// Параметры грифа, использованные при последнем построении.
        /// </summary>
        public BarParameters CurrentParameters => _parameters;

        /// <summary>
        /// Главный сценарий построения.
        /// Подключает KOMPAS, создаёт документ и строит все сегменты грифа.
        /// </summary>
        public void Build(BarParameters parameters)
        {
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));

            _wrapper.AttachOrRunCAD();
            _wrapper.CreateDocument3D();

            BuildBar();
        }

        /// <summary>
        /// Строит гриф вдоль оси X без использования промежуточной переменной x.
        /// Все координаты считаются явно от нуля.
        /// </summary>
        private void BuildBar()
        {
            double Ls = _parameters.SleeveLength;     // длинна посадочной части
            double Ld = _parameters.SeparatorLength;  // длинна разделителя
            double Lh = _parameters.HandleLength;     // длинна ручки

            //TODO:+ RSDN
            // Диаметр рукояти должен быть меньше, чем диаметр посадки/разделителя.
            double handleDiameter =
                Math.Min(_parameters.SleeveDiameter, _parameters.SeparatorDiameter) - 3.0;

            //TODO:+ RSDN
            // Запасной вариант: если параметры кривые — берем 80% от разделителя.
            if (handleDiameter <= 0)
                handleDiameter = _parameters.SeparatorDiameter * 0.8;

            //TODO:+ WTF

            // 1. Левая посадка
            double leftSleeveStart = 0.0;
            double leftSleeveEnd = Ls;

            // 2. Левый разделитель
            double leftSepStart = leftSleeveEnd;
            double leftSepEnd = leftSepStart + Ld;

            // 3. Рукоять
            double handleStart = leftSepEnd;
            double handleEnd = handleStart + Lh;

            // 4. Правый разделитель
            double rightSepStart = handleEnd;
            double rightSepEnd = rightSepStart + Ld;

            // 5. Правая посадка
            double rightSleeveStart = rightSepEnd;
            double rightSleeveEnd = rightSleeveStart + Ls;


            //
            // Построение сегментов в KOMPAS
            //

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
