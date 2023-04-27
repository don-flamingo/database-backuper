namespace DatabaseBackuper.Framework;

public static class DateTimeExtensions
{
    public static string ToRFC3339(this DateTime date)
    {
        return date.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss.fffK");
    }
}