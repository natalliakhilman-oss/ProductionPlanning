using System.ComponentModel;

namespace ProductionPlanning.Extensions
{
    public static class EnumExtensions
    {
        public static string ToStringX(this Enum enumerate)
        {
            var type = enumerate.GetType();
            var fieldInfo = type.GetField(enumerate.ToString());
            var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : enumerate.ToString();
        }
    }
}
