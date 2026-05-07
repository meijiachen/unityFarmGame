public readonly struct GameDateTime
{
    public readonly int Year;
    public readonly Season Season;
    public readonly int Day;
    public readonly string DayOfWeek;
    public readonly int Hour;
    public readonly int Minute;
    public readonly int Second;

    public GameDateTime(int year, Season season, int day, string dayOfWeek, int hour, int minute, int second)
    {
        Year = year;
        Season = season;
        Day = day;
        DayOfWeek = dayOfWeek;
        Hour = hour;
        Minute = minute;
        Second = second;
    }
}

