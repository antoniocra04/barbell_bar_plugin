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
        /// </summary>
        public double TotalLength =>
            2 * SleeveLength + 2 * SeparatorLength + HandleLength;

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
