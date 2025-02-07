using Domain.Exceptions;
using Domain.Models.Candlestick;
using System.Reflection;
using System.Runtime.Serialization;

namespace Domain.Extensions
{
    public static class IntervalExtensions
    {
        public static string ToApiString(this Interval interval)
        {
            var type = typeof(Interval);
            var memberInfo = type
                .GetMember(interval.ToString())
                .FirstOrDefault();

            if (memberInfo != null)
            {
                var attribute = memberInfo
                    .GetCustomAttribute<EnumMemberAttribute>();

                return 
                    attribute?.Value 
                    ?? throw new MissingApiStringAttributeException(interval.ToString());
            }

            throw new MissingApiStringAttributeException(interval.ToString());
        }

        public static Interval FromApiString(string apiString)
        {
            var type = typeof(Interval);
            foreach (var member in type.GetMembers())
            {
                var attribute = member
                    .GetCustomAttribute<EnumMemberAttribute>();

                if (attribute != null && attribute.Value == apiString)
                    return (Interval)Enum.Parse(type, member.Name);
            }

            throw new UnknownIntervalException(apiString);
        }
    }
}