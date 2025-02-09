using System.ComponentModel;

namespace Domain.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum enumerationValue)
        {
            var type = enumerationValue.GetType();
            var memberInfos = type.GetMember(enumerationValue.ToString());
            if (memberInfos.Length > 0)
            {
                var descriptionAttributes = memberInfos[0].GetCustomAttributes(
                    typeof(DescriptionAttribute),
                    false);

                if (descriptionAttributes.Length > 0)
                    return ((DescriptionAttribute)descriptionAttributes[0]).Description;
            }

            return enumerationValue.ToString();
        }
    }
}
