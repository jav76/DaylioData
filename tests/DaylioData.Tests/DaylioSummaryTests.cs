using System.Globalization;
using DaylioData.Models;

namespace DaylioData.Tests;

public class DaylioSummaryTests
{
    private const string SAMPLE_CSV_HEADER = "full_date,date,weekday,time,mood,activities,note_title,note\n";

    [Fact]
    public void TotalActivitiesCount_WithMultiWordActivities_CountsCorrectly()
    {
        string csv = SAMPLE_CSV_HEADER +
            "2023-01-01,2023-01-01,Sunday,10:00,good,morning run | reading book,Title,Note\n" +
            "2023-01-02,2023-01-02,Monday,12:00,rad,weight lifting | healthy eating | coding,Title,Note\n";

        using StringReader reader = new(csv);
        DaylioData daylioData = new(reader);

        Assert.NotNull(daylioData.DataSummary);
        Assert.Equal(5, daylioData.DataSummary.TotalActivitiesCount);
        Assert.Equal(5, daylioData.DataSummary.DistinctActivitiesCount);
    }

    [Fact]
    public void AverageEntriesPerDay_WhenZeroDays_ReturnsZeroNotNaN()
    {
        string csv = SAMPLE_CSV_HEADER;

        using StringReader reader = new(csv);
        DaylioData daylioData = new(reader);

        Assert.NotNull(daylioData.DataSummary);
        Assert.Equal(0, daylioData.DataSummary.TotalDays);
        Assert.Equal(0.0, daylioData.DataSummary.AverageEntriesPerDay);
    }

    [Fact]
    public void NoteTotalWordCount_WithEmptyOrWhitespaceNotes_ReturnsZero()
    {
        string csv = SAMPLE_CSV_HEADER +
            "2023-01-01,2023-01-01,Sunday,10:00,good,walking,,\n" +
            "2023-01-02,2023-01-02,Monday,12:00,rad,walking,,   \n";

        using StringReader reader = new(csv);
        DaylioData daylioData = new(reader);

        Assert.NotNull(daylioData.DataSummary);
        Assert.Equal(0, daylioData.DataSummary.NoteTotalWordCount);
    }

    [Fact]
    public void NoteTotalWordCount_WithWords_CountsAccurately()
    {
        string csv = SAMPLE_CSV_HEADER +
            "2023-01-01,2023-01-01,Sunday,10:00,good,walking,,Hello world today\n" +
            "2023-01-02,2023-01-02,Monday,12:00,rad,walking,,Another sunny day\n";

        using StringReader reader = new(csv);
        DaylioData daylioData = new(reader);

        Assert.NotNull(daylioData.DataSummary);
        Assert.Equal(6, daylioData.DataSummary.NoteTotalWordCount);
    }

    [Fact]
    public void EarliestAndLatestEntry_SameDayDifferentTimes_OrdersCorrectlyByTimestamp()
    {
        string csv = SAMPLE_CSV_HEADER +
            "2023-05-10,2023-05-10,Wednesday,20:00,bad,gaming,Evening,Late entry\n" +
            "2023-05-10,2023-05-10,Wednesday,08:30,rad,running,Morning,Early entry\n" +
            "2023-05-10,2023-05-10,Wednesday,13:00,good,lunch,Afternoon,Mid entry\n";

        using StringReader reader = new(csv);
        DaylioData daylioData = new(reader);

        Assert.NotNull(daylioData.DataSummary);
        DaylioCSVDataModel? earliest = daylioData.DataSummary.EarliestEntry;
        DaylioCSVDataModel? latest = daylioData.DataSummary.LatestEntry;

        Assert.NotNull(earliest);
        Assert.NotNull(latest);
        Assert.Equal(new TimeOnly(8, 30), earliest.Time);
        Assert.Equal(new TimeOnly(20, 0), latest.Time);
    }

    [Fact]
    public void GetSummary_IncludesAllSummaryProperties()
    {
        string csv = SAMPLE_CSV_HEADER +
            "2023-01-01,2023-01-01,Sunday,10:00,good,walking,Title,Short note\n";

        using StringReader reader = new(csv);
        DaylioData daylioData = new(reader);

        Assert.NotNull(daylioData.DataSummary);
        string summary = daylioData.DataSummary.GetSummary();

        Assert.Contains("TotalEntries: 1", summary);
        Assert.Contains("TotalDays: 1", summary);
        Assert.Contains("DistinctActivitiesCount: 1", summary);
        Assert.Contains("TotalActivitiesCount: 1", summary);
    }

    [Fact]
    public void DaylioCSVDataModel_RecordValueEquality_MatchesEquivalentRecords()
    {
        DaylioCSVDataModel entry1 = new()
        {
            FullDate = new DateOnly(2023, 1, 1),
            Date = new DateOnly(2023, 1, 1),
            Weekday = "Sunday",
            Time = new TimeOnly(10, 0),
            Mood = "good",
            Activities = "reading | coffee",
            NoteTitle = "Breakfast",
            Note = "Morning reading"
        };

        DaylioCSVDataModel entry2 = new()
        {
            FullDate = new DateOnly(2023, 1, 1),
            Date = new DateOnly(2023, 1, 1),
            Weekday = "Sunday",
            Time = new TimeOnly(10, 0),
            Mood = "good",
            Activities = "reading | coffee",
            NoteTitle = "Breakfast",
            Note = "Morning reading"
        };

        Assert.Equal(entry1, entry2);
        Assert.Equal(2, entry1.ActivitiesCollection.Count);
        Assert.Equal("reading", entry1.ActivitiesCollection[0]);
        Assert.Equal("coffee", entry1.ActivitiesCollection[1]);

        DaylioCSVDataModel differentWeekday = entry1 with { Weekday = "Monday" };
        Assert.NotEqual(entry1, differentWeekday);
    }

    [Fact]
    public void DaylioCSVDataModel_ToString_UsesInvariantCulture()
    {
        DaylioCSVDataModel entry = new()
        {
            FullDate = new DateOnly(2023, 1, 1),
            Date = new DateOnly(2023, 1, 1),
            Weekday = "Sunday",
            Time = new TimeOnly(10, 0),
            Mood = "good",
            Activities = "walking",
            NoteTitle = "Walk",
            Note = "Morning"
        };

        string output = entry.ToString();
        Assert.StartsWith("2023-01-01,", output);
        Assert.Contains(",10:00,", output);
    }
}
