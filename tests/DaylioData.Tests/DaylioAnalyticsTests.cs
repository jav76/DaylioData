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
}
