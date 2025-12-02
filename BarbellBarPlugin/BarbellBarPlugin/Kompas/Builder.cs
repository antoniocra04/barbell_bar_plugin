using System;
using BarbellBarPlugin.Kompas;
using BarbellBarPlugin.Model;

namespace BarbellBarPlugin.Kompas
{
    /// <summary>
    /// Оркестратор построения модели грифа.
    /// </summary>
    public class BarBuilder
    {
        //TODO: XML
        private readonly Wrapper _wrapper;
        //TODO: XML
        private BarParameters _parameters = null!;

        //TODO: XML
        public BarBuilder(Wrapper wrapper)
        {
            _wrapper = wrapper ?? throw new ArgumentNullException(nameof(wrapper));
        }

        //TODO: XML
        public BarParameters CurrentParameters => _parameters;

        /// <summary>
        /// Главный сценарий построения модели.
        /// </summary>
        public void Build(BarParameters parameters)
        {
            _parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));

            _wrapper.AttachOrRunCAD();
            _wrapper.CreateDocument3D();

            BuildBar();
        }

        /// <summary>
        /// Строит гриф вдоль оси X
        /// </summary>
        private void BuildBar()
        {
            double sleeveLen = _parameters.SleeveLength;
            double sepLen = _parameters.SeparatorLength;
            double handleLen = _parameters.HandleLength;

            double handleDiameter =
                //TODO: RSDN
                Math.Min(_parameters.SleeveDiameter, _parameters.SeparatorDiameter) - 3.0;

            if (handleDiameter <= 0)
            {
                //TODO: RSDN
                handleDiameter = _parameters.SeparatorDiameter * 0.8;
            }

            //TODO: WTF?
            double x = 0.0;

            double leftSleeveStart = x;
            double leftSleeveEnd = leftSleeveStart + sleeveLen;
            x = leftSleeveEnd;

            double leftSepStart = x;
            double leftSepEnd = leftSepStart + sepLen;
            x = leftSepEnd;

            double handleStart = x;
            double handleEnd = handleStart + handleLen;
            x = handleEnd;

            double rightSepStart = x;
            double rightSepEnd = rightSepStart + sepLen;
            x = rightSepEnd;

            double rightSleeveStart = x;
            double rightSleeveEnd = rightSleeveStart + sleeveLen;


            // 1. Левая посадочная часть
            _wrapper.CreateCylindricalSegment(
                leftSleeveStart,
                leftSleeveEnd,
                _parameters.SleeveDiameter,
                "LeftSleeve");

            // 2. Левый разделитель
            _wrapper.CreateCylindricalSegment(
                leftSepStart,
                leftSepEnd,
                _parameters.SeparatorDiameter,
                "LeftSeparator");

            // 3. Ручка
            _wrapper.CreateCylindricalSegment(
                handleStart,
                handleEnd,
                handleDiameter,
                "Handle");

            // 4. Правый разделитель
            _wrapper.CreateCylindricalSegment(
                rightSepStart,
                rightSepEnd,
                _parameters.SeparatorDiameter,
                "RightSeparator");

            // 5. Правая посадочная часть
            _wrapper.CreateCylindricalSegment(
                rightSleeveStart,
                rightSleeveEnd,
                _parameters.SleeveDiameter,
                "RightSleeve");
        }
    }
}
