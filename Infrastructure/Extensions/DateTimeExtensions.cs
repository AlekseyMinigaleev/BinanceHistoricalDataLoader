
namespace Infrastructure.Extensions
{
    internal static class DateTimeExtensions
    {
        public static long ToUnixTimeMilliseconds(this DateTime dateTime)
        {
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        }
    }
}
