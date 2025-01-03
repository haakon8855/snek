namespace Web;

public static class DateTimeExtensions
{
	public static string ToLocalDateString(this DateTime utcTime)
	{
		var timezone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
		var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, timezone);

		return $"{localTime.ToShortDateString()} {localTime.ToShortTimeString()}";
	}
}