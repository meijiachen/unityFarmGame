using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : SingletonMonobehaviour<TimeManager>
{
    private const int SecondsPerMinute = 60;
    private const int MinutesPerHour = 60;
    private const int HoursPerDay = 24;
    private const int SpringDays = 31;
    private const int SummerDays = 30;
    private const int AutumnDays = 31;
    private const int WinterDays = 28;

    private static readonly string[] WeekDays = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

    private int gameYear = 1;
    private Season gameSeason = Season.Spring;
    private int gameDay = 1;
    private int gameHour = 6;
    private int gameMinute = 30;
    private int gameSecond = 0;
    private string gameDayOfWeek = "Mon";

    private bool gameClockPaused = false;

    private float gametick = 0f;

    private void Start()
    {
        gameDayOfWeek = GetGameDayOfWeek();
        EventHandler.CallGameTimeChangedEvent(
            new GameDateTime(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond),
            TimeChangeFlags.Initial);
    }

    private void Update()
    {
        if (!gameClockPaused)
        {
            GameTick();
        }
    }

    private void GameTick()
    {
        gametick += Time.deltaTime;
        if (gametick >= Settings.secondsPerGameSecond)
        {
            gametick -= Settings.secondsPerGameSecond;
            UpdateGameSecond();
        }
    }

    private void UpdateGameSecond()
    {
        gameSecond++;

        if (gameSecond < SecondsPerMinute)
        {
            return;
        }

        gameSecond = 0;
        gameMinute++;
        bool isHourAdvanced = false;
        bool isDayAdvanced = false;
        bool isSeasonAdvanced = false;
        bool isYearAdvanced = false;

        if (gameMinute >= MinutesPerHour)
        {
            gameMinute = 0;
            gameHour++;
            isHourAdvanced = true;
        }

        if (gameHour >= HoursPerDay)
        {
            gameHour = 0;
            gameDay++;
            isDayAdvanced = true;
            gameDayOfWeek = GetGameDayOfWeek();
        }

        int daysInCurrentSeason = GetDaysInSeason(gameSeason);
        if (gameDay > daysInCurrentSeason)
        {
            gameDay = 1;

            if (gameSeason < Season.Winter)
            {
                gameSeason++;
                isSeasonAdvanced = true;
            }
            else
            {
                gameSeason = Season.Spring;
                isSeasonAdvanced = true;
                gameYear++;
                if (gameYear > 9999)
                {
                    gameYear = 1;
                }
                isYearAdvanced = true;
            }
        }

        TimeChangeFlags flags = TimeChangeFlags.Minute;
        if (isHourAdvanced) flags |= TimeChangeFlags.Hour;
        if (isDayAdvanced) flags |= TimeChangeFlags.Day;
        if (isSeasonAdvanced) flags |= TimeChangeFlags.Season;
        if (isYearAdvanced) flags |= TimeChangeFlags.Year;

        EventHandler.CallGameTimeChangedEvent(
            new GameDateTime(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond),
            flags);
    }

    private string GetGameDayOfWeek()
    {
        int totalDaysSinceStart = 0;

        for (int year = 1; year < gameYear; year++)
        {
            totalDaysSinceStart += SpringDays + SummerDays + AutumnDays + WinterDays;
        }

        for (Season season = Season.Spring; season < gameSeason; season++)
        {
            totalDaysSinceStart += GetDaysInSeason(season);
        }

        totalDaysSinceStart += gameDay - 1;
        return WeekDays[totalDaysSinceStart % WeekDays.Length];
    }

    private int GetDaysInSeason(Season season)
    {
        switch (season)
        {
            case Season.Spring:
                return SpringDays;
            case Season.Summer:
                return SummerDays;
            case Season.Autumn:
                return AutumnDays;
            case Season.Winter:
                return WinterDays;
            default:
                return SpringDays;
        }
    }

    public void SetGameClockPaused(bool isPaused)
    {
        gameClockPaused = isPaused;
    }

    public void ToggleGameClockPaused()
    {
        gameClockPaused = !gameClockPaused;
    }

    public void TestAdvanceGameMinute()
    {
        for (int i = 0; i < 60; i++)
        {
            UpdateGameSecond();
        }
    }

    public void TestAdvanceGameDay()
    {
        for (int i = 0; i < 86400; i++)
        {
            UpdateGameSecond();
        }
    }
    
}
