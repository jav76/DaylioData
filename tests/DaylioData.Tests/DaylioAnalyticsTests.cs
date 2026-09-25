namespace DaylioData.Tests;

public class DaylioAnalyticsTests
{
    private const string ANALYTICS_CSV =
        "full_date,date,weekday,time,mood,activities,note_title,note\n" +
        "2023-01-01,2023-01-01,Sunday,09:00,good,running | meditation,Title,Note\n" +
        "2023-01-02,2023-01-02,Monday,10:00,good,running | reading,Title,Note\n" +
        "2023-01-03,2023-01-03,Tuesday,11:00,rad,running | coding,Title,Note\n" +
        "2023-01-04,2023-01-04,Wednesday,12:00,rad,gaming,Title,Note\n" +
        "2023-01-06,2023-01-06,Friday,14:00,meh,reading,Title,Note\n";

    [Fact]
    public void GetMoodDistribution_CountsOccurrencesAccurately()
    {
        using StringReader reader = new(ANALYTICS_CSV);
        DaylioData daylioData = new(reader);

        IReadOnlyDictionary<string, int> distribution = daylioData.GetMoodDistribution();

        Assert.Equal(2, distribution["good"]);
        Assert.Equal(2, distribution["rad"]);
        Assert.Equal(1, distribution["meh"]);
    }

    [Fact]
    public void GetTopActivities_OrdersByFrequency()
    {
        using StringReader reader = new(ANALYTICS_CSV);
        DaylioData daylioData = new(reader);

        IReadOnlyList<KeyValuePair<string, int>> topActivities = daylioData.GetTopActivities(2);

        Assert.Equal(2, topActivities.Count);
        Assert.Equal("running", topActivities[0].Key);
        Assert.Equal(3, topActivities[0].Value);
        Assert.Equal("reading", topActivities[1].Key);
        Assert.Equal(2, topActivities[1].Value);
    }

    [Fact]
    public void GetLongestStreak_CalculatesMaxConsecutiveDays()
    {
        using StringReader reader = new(ANALYTICS_CSV);
        DaylioData daylioData = new(reader);

        // Dates are 2023-01-01, 02, 03, 04 (streak of 4), gap on 05, then 06 (streak of 1)
        int maxStreak = daylioData.GetLongestStreak();

        Assert.Equal(4, maxStreak);
    }

    [Fact]
    public void GetAverageMoodByDayOfWeek_CalculatesAccurateDayRatings()
    {
        using StringReader reader = new(ANALYTICS_CSV);
        DaylioData daylioData = new(reader);

        daylioData.DataRepo!.SetDefaultMoodLevels(); // good=4, rad=5, meh=3

        IReadOnlyDictionary<DayOfWeek, decimal> weekdayMoods = daylioData.GetAverageMoodByDayOfWeek();

        Assert.Equal(4.0m, weekdayMoods[DayOfWeek.Sunday]);
        Assert.Equal(4.0m, weekdayMoods[DayOfWeek.Monday]);
        Assert.Equal(5.0m, weekdayMoods[DayOfWeek.Tuesday]);
        Assert.Equal(5.0m, weekdayMoods[DayOfWeek.Wednesday]);
        Assert.Equal(3.0m, weekdayMoods[DayOfWeek.Friday]);
    }

    [Fact]
    public void GetActivityMoodImpact_CalculatesImpactAccurately()
    {
        using StringReader reader = new(ANALYTICS_CSV);
        DaylioData daylioData = new(reader);

        ActivityMoodImpact? runningImpact = daylioData.GetActivityMoodImpact("running");

        Assert.NotNull(runningImpact);
        Assert.Equal("running", runningImpact.Activity);
        Assert.Equal(3, runningImpact.FrequencyWith);
        Assert.Equal(2, runningImpact.FrequencyWithout);
        // With: (4 + 4 + 5) / 3 = 4.333...
        // Without: (5 + 3) / 2 = 4.0
        Assert.True(runningImpact.AverageMoodWith > 4.33m && runningImpact.AverageMoodWith < 4.34m);
        Assert.Equal(4.0m, runningImpact.AverageMoodWithout);
        Assert.True(runningImpact.Delta > 0.33m && runningImpact.Delta < 0.34m);
    }

    [Fact]
    public void GetAllActivityMoodImpacts_RanksByDeltaDescending()
    {
        using StringReader reader = new(ANALYTICS_CSV);
        DaylioData daylioData = new(reader);

        IReadOnlyList<ActivityMoodImpact> impacts = daylioData.GetAllActivityMoodImpacts(minOccurrences: 1);

        Assert.NotEmpty(impacts);
        // Verify descending order by Delta
        for (int i = 1; i < impacts.Count; i++)
        {
            Assert.True(impacts[i - 1].Delta >= impacts[i].Delta);
        }
    }

    [Fact]
    public void GetMoodByTimeOfDay_GroupsByCanonicalPeriods()
    {
        using StringReader reader = new(ANALYTICS_CSV);
        DaylioData daylioData = new(reader);

        IReadOnlyDictionary<TimeOfDayPeriod, TimeOfDayMood> trends = daylioData.GetMoodByTimeOfDay();

        Assert.True(trends.ContainsKey(TimeOfDayPeriod.Morning));
        Assert.True(trends.ContainsKey(TimeOfDayPeriod.Afternoon));
        Assert.Equal(3, trends[TimeOfDayPeriod.Morning].EntryCount); // 09:00, 10:00, 11:00
        Assert.Equal(2, trends[TimeOfDayPeriod.Afternoon].EntryCount); // 12:00, 14:00
    }

    [Fact]
    public void GetStreakDetails_ReturnsComprehensiveStreakInformation()
    {
        using StringReader reader = new(ANALYTICS_CSV);
        DaylioData daylioData = new(reader);

        StreakDetails details = daylioData.GetStreakDetails();

        Assert.Equal(4, details.LongestStreak);
        Assert.Equal(1, details.CurrentStreak);
        Assert.Equal(new DateOnly(2023, 1, 6), details.StreakEndDate);
        Assert.Equal(new DateOnly(2023, 1, 6), details.StreakStartDate);
    }

    [Fact]
    public void GetRollingMoodTrends_NormalizesMultipleEntriesPerDayAndAppliesRollingWindow()
    {
        string multiEntryCsv =
            "full_date,date,weekday,time,mood,activities,note_title,note\n" +
            "2023-01-01,2023-01-01,Sunday,09:00,good,walking,Morning,Walk\n" +
            "2023-01-01,2023-01-01,Sunday,19:00,rad,reading,Evening,Book\n" +
            "2023-01-02,2023-01-02,Monday,10:00,meh,coding,Work,Code\n" +
            "2023-01-03,2023-01-03,Tuesday,12:00,good,running,Lunch,Run\n";

        using StringReader reader = new(multiEntryCsv);
        DaylioData daylioData = new(reader);

        // Day 1: good(4) + rad(5) = avg 4.5, count 2. Rolling (1 day): 4.5
        // Day 2: meh(3) = avg 3.0, count 1. Rolling (2 days in 7d): (4.5 + 3.0) / 2 = 3.75
        // Day 3: good(4) = avg 4.0, count 1. Rolling (3 days in 7d): (4.5 + 3.0 + 4.0) / 3 = 3.8333...
        IReadOnlyList<DailyRollingMood> trends = daylioData.GetRollingMoodTrends(windowDays: 7);

        Assert.Equal(3, trends.Count);
        Assert.Equal(new DateOnly(2023, 1, 1), trends[0].Date);
        Assert.Equal(4.5m, trends[0].DailyAverageMood);
        Assert.Equal(4.5m, trends[0].RollingAverageMood);
        Assert.Equal(2, trends[0].EntryCount);

        Assert.Equal(new DateOnly(2023, 1, 2), trends[1].Date);
        Assert.Equal(3.0m, trends[1].DailyAverageMood);
        Assert.Equal(3.75m, trends[1].RollingAverageMood);
        Assert.Equal(1, trends[1].EntryCount);

        Assert.Equal(new DateOnly(2023, 1, 3), trends[2].Date);
        Assert.Equal(4.0m, trends[2].DailyAverageMood);
        Assert.True(trends[2].RollingAverageMood > 3.83m && trends[2].RollingAverageMood < 3.84m);
    }

    [Fact]
    public void GetRollingMoodTrends_WithEmptyData_ReturnsEmptyList()
    {
        using StringReader reader = new("full_date,date,weekday,time,mood,activities,note_title,note\n");
        DaylioData daylioData = new(reader);

        IReadOnlyList<DailyRollingMood> trends = daylioData.GetRollingMoodTrends(windowDays: 7);
        Assert.Empty(trends);
    }
}
