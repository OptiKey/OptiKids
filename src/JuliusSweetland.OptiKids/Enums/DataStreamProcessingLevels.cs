using JuliusSweetland.OptiKids.Properties;

namespace JuliusSweetland.OptiKids.Enums
{
    public enum DataStreamProcessingLevels
    {
        None,
        Low,
        Medium,
        High
    }

    public static partial class EnumExtensions
    {
        public static string ToDescription(this DataStreamProcessingLevels pointSource)
        {
            switch (pointSource)
            {
                case DataStreamProcessingLevels.High: return Resources.HIGH;
                case DataStreamProcessingLevels.Medium: return Resources.MEDIUM;
                case DataStreamProcessingLevels.Low: return Resources.LOW;
                case DataStreamProcessingLevels.None: return Resources.NONE;
            }

            return pointSource.ToString();
        }
    }
}
