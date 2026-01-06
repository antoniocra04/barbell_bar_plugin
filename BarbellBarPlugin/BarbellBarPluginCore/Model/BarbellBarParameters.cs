namespace BarbellBarPlugin.Model
{
    /// <summary>
    /// Параметры грифа штанги.
    /// </summary>
    public class BarParameters
    {
        /// <summary>Диаметр посадочной части, мм.</summary>
        public double SleeveDiameter { get; }

        /// <summary>Длина разделителя, мм.</summary>
        public double SeparatorLength { get; }

        /// <summary>Длина ручки (хвата), мм.</summary>
        public double HandleLength { get; }

        /// <summary>Диаметр разделителя, мм.</summary>
        public double SeparatorDiameter { get; }

        /// <summary>Длина посадочной части, мм.</summary>
        public double SleeveLength { get; }

        /// <summary>
        /// Полная длина грифа, мм:
        /// 2 · длина посадочной части + 2 · длина разделителя + длина ручки.
        /// </summary>
        public double TotalLength =>
            2 * SleeveLength + 2 * SeparatorLength + HandleLength;

        /// <summary>
        /// Создаёт набор параметров грифа штанги.
        /// </summary>
        /// <param name="sleeveDiameter">Диаметр посадочной части, мм.</param>
        /// <param name="separatorLength">Длина разделителя, мм.</param>
        /// <param name="handleLength">Длина ручки (хвата), мм.</param>
        /// <param name="separatorDiameter">Диаметр разделителя, мм.</param>
        /// <param name="sleeveLength">Длина посадочной части, мм.</param>
        public BarParameters(
            double sleeveDiameter,
            double separatorLength,
            double handleLength,
            double separatorDiameter,
            double sleeveLength)
        {
            SleeveDiameter = sleeveDiameter;
            SeparatorLength = separatorLength;
            HandleLength = handleLength;
            SeparatorDiameter = separatorDiameter;
            SleeveLength = sleeveLength;
        }
    }
}
