using System;
using System.Globalization;

public static class DateConversionUtility
{
    private static readonly CultureInfo HijriCulture = new CultureInfo("ar-SA");
    private static readonly HijriCalendar HijriCalendar = new HijriCalendar();
    private static readonly GregorianCalendar GregorianCalendar = new GregorianCalendar();

    static DateConversionUtility()
    {
        HijriCulture.DateTimeFormat.Calendar = HijriCalendar;
    }

    public enum OutputCalendar
    {
        Hijri,
        Gregorian
    }

    public static DateTime ConvertDate(string dateString, OutputCalendar outputCalendar)
    {
        DateTime date;
        bool likelyGregorian;

        // Check if the format is likely Hijri (year-month-day)
        if (DateTime.TryParseExact(dateString, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            likelyGregorian = false; // It's likely a Hijri date
        }
        // Check if the format is likely Gregorian (day-month-year)
        else if (DateTime.TryParseExact(dateString, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            likelyGregorian = true; // It's likely a Gregorian date
        }
        else
        {
            throw new ArgumentException("Invalid date format.");
        }

        if (outputCalendar == OutputCalendar.Hijri)
        {
            return likelyGregorian ? ConvertToHijri(date) : date; // Convert to Hijri or return as is
        }
        else
        {
            return likelyGregorian ? date : ConvertToGregorian(date); // Convert to Gregorian or return as is
        }
    }

    private static DateTime ConvertToHijri(DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, HijriCalendar);
    }

    private static DateTime ConvertToGregorian(DateTime date)
    {
        return HijriCalendar.ToDateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
    }
}