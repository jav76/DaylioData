using System.Text.Json;

namespace DaylioData.Tests;

public class DaylioExportTests
{
    private const string SAMPLE_CSV =
        "full_date,date,weekday,time,mood,activities,note_title,note\n" +
        "2023-01-01,2023-01-01,Sunday,09:00,good,running | meditation,Morning,Good run today\n" +
        "2023-01-02,2023-01-02,Monday,10:00,good,running | reading,Quiet day,Read a chapter\n" +
        "2023-01-03,2023-01-03,Tuesday,11:00,rad,running | coding,Productive,Shipped new code\n" +
        "2023-01-04,2023-01-04,Wednesday,12:00,rad,gaming,Break,Played video games\n";

    [Fact]
    public void GenerateMarkdownReport_ContainsRequiredSectionsAndData()
    {
        using StringReader reader = new(SAMPLE_CSV);
        DaylioData daylioData = new(reader);

        string report = daylioData.GenerateMarkdownReport("Custom Habit Report");

        Assert.Contains("# Custom Habit Report", report);
        Assert.Contains("## Summary Overview", report);
        Assert.Contains("- **Total Entries**: 4", report);
        Assert.Contains("- **Total Days Tracked**: 4", report);
        Assert.Contains("## Mood Distribution", report);
        Assert.Contains("## Top Activities", report);
        Assert.Contains("## Time-of-Day Mood Trends", report);
        Assert.Contains("## Recent Mood Trends (7-Day Rolling Average)", report);
    }

    [Fact]
    public void ToJson_ProducesValidJsonWithExpectedStructure()
    {
        using StringReader reader = new(SAMPLE_CSV);
        DaylioData daylioData = new(reader);

        string json = daylioData.ToJson();

        Assert.False(string.IsNullOrWhiteSpace(json));
        using JsonDocument doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        Assert.Equal(4, root.GetProperty("totalEntries").GetInt32());
        Assert.Equal(4, root.GetProperty("totalDays").GetInt32());
        Assert.True(root.TryGetProperty("streaks", out JsonElement streaksElement));
        Assert.Equal(4, streaksElement.GetProperty("longestStreak").GetInt32());
        Assert.True(root.TryGetProperty("moodDistribution", out JsonElement moodDistElement));
        Assert.Equal(2, moodDistElement.GetProperty("good").GetInt32());
        Assert.Equal(2, moodDistElement.GetProperty("rad").GetInt32());
        Assert.True(root.TryGetProperty("habitSynergies", out JsonElement synergiesElement));
        Assert.Equal(3, synergiesElement.GetArrayLength());
        Assert.True(root.TryGetProperty("rollingMoodTrends", out JsonElement trendsElement));
        Assert.Equal(4, trendsElement.GetArrayLength());
    }

    [Fact]
    public void GenerateMarkdownReport_WithCoOccurringPairs_RendersHabitSynergiesSection()
    {
        string csvWithPairs =
            "full_date,date,weekday,time,mood,activities,note_title,note\n" +
            "2023-01-01,2023-01-01,Sunday,09:00,good,running | meditation,Morning,Run\n" +
            "2023-01-02,2023-01-02,Monday,10:00,rad,running | meditation,Morning,Run\n" +
            "2023-01-03,2023-01-03,Tuesday,11:00,meh,reading,Quiet,Read\n";

        using StringReader reader = new(csvWithPairs);
        DaylioData daylioData = new(reader);

        string report = daylioData.GenerateMarkdownReport("Synergy Report");

        Assert.Contains("## Habit Synergies (Top Co-Occurring Pairs)", report);
        Assert.Contains("meditation & running", report);
    }
}
