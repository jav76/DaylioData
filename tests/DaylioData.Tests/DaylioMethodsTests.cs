using DaylioData.Models;

namespace DaylioData.Tests;

public class DaylioMethodsTests
{
    private const string SAMPLE_CSV =
        "full_date,date,weekday,time,mood,activities,note_title,note\n" +
        "2023-01-01,2023-01-01,Sunday,08:00,good,reading | coffee,Breakfast,Morning reading\n" +
        "2023-01-01,2023-01-01,Sunday,14:00,rad,gym | coding,Project,Built a feature\n" +
        "2023-01-02,2023-01-02,Monday,09:00,meh,work,Office,Busy morning\n" +
        "2023-01-02,2023-01-02,Monday,21:00,bad,gaming,Night,Stayed up late\n" +
        "2023-01-03,2023-01-03,Tuesday,11:00,rad,coding | music,Code,Writing unit tests\n";

    [Fact]
    public void GetEntriesInRange_SpanningMultipleDaysAndTimes_FiltersAccurately()
    {
        using StringReader reader = new(SAMPLE_CSV);
        DaylioData daylioData = new(reader);

        DateTime start = new(2023, 1, 1, 12, 0, 0);
        DateTime end = new(2023, 1, 2, 12, 0, 0);

        List<DaylioCSVDataModel> inRange = daylioData.GetEntriesInRange(start, end)!.ToList();

        Assert.Equal(2, inRange.Count);
        Assert.Equal(new TimeOnly(14, 0), inRange[0].Time);
        Assert.Equal(new TimeOnly(9, 0), inRange[1].Time);
    }

    [Fact]
    public void GetEntriesInRange_StaticMethod_MatchesExtensionResult()
    {
        using StringReader reader = new(SAMPLE_CSV);
        DaylioData daylioData = new(reader);

        DateTime start = new(2023, 1, 1, 0, 0, 0);
        DateTime end = new(2023, 1, 3, 23, 59, 59);

        List<DaylioCSVDataModel> extensionResult = daylioData.GetEntriesInRange(start, end)!.ToList();
        List<DaylioCSVDataModel> staticResult = Methods.GetEntriesInRange(daylioData, start, end)!.ToList();

        Assert.Equal(5, extensionResult.Count);
        Assert.Equal(extensionResult.Count, staticResult.Count);
    }

    [Fact]
    public void GetEntriesWithActivity_CaseInsensitive_ReturnsMatchingEntries()
    {
        using StringReader reader = new(SAMPLE_CSV);
        DaylioData daylioData = new(reader);

        List<DaylioCSVDataModel> codingEntries = daylioData.GetEntriesWithActivity("CODING")!.ToList();

        Assert.Equal(2, codingEntries.Count);
    }

    [Fact]
    public void GetEntriesWithActivity_NonExistent_ReturnsNull()
    {
        using StringReader reader = new(SAMPLE_CSV);
        DaylioData daylioData = new(reader);

        IEnumerable<DaylioCSVDataModel>? result = daylioData.GetEntriesWithActivity("skydiving");

        Assert.Null(result);
    }

    [Fact]
    public void GetEntriesWithMood_CaseInsensitive_ReturnsMatchingEntries()
    {
        using StringReader reader = new(SAMPLE_CSV);
        DaylioData daylioData = new(reader);

        List<DaylioCSVDataModel> radEntries = daylioData.GetEntriesWithMood("RAD")!.ToList();

        Assert.Equal(2, radEntries.Count);
    }

    [Fact]
    public void GetActivityCount_ReturnsCorrectNumber()
    {
        using StringReader reader = new(SAMPLE_CSV);
        DaylioData daylioData = new(reader);

        int? count = daylioData.GetActivityCount("coding");

        Assert.Equal(2, count);
    }

    [Fact]
    public void GetEntriesWithString_FindsSubstringInNotes()
    {
        using StringReader reader = new(SAMPLE_CSV);
        DaylioData daylioData = new(reader);

        List<DaylioCSVDataModel> matches = daylioData.GetEntriesWithString("unit tests")!.ToList();

        Assert.Single(matches);
        Assert.Equal("Code", matches[0].NoteTitle);
    }

    [Fact]
    public void GetAverageActivityMood_WithMoodLevelsSet_CalculatesCorrectAverage()
    {
        using StringReader reader = new(SAMPLE_CSV);
        DaylioData daylioData = new(reader);

        daylioData.DataRepo!.SetDefaultMoodLevels();

        decimal? average = daylioData.GetAverageActivityMood("coding");

        Assert.NotNull(average);
        Assert.Equal(5.0m, average.Value);
    }

    [Fact]
    public void GetAverageActivityMood_WhenUnrelatedMoodUnset_StillCalculatesForConfiguredActivity()
    {
        using StringReader reader = new(SAMPLE_CSV);
        DaylioData daylioData = new(reader);

        daylioData.DataRepo!.SetMoodLevel("good", 4);

        decimal? average = daylioData.GetAverageActivityMood("reading");

        Assert.NotNull(average);
        Assert.Equal(4.0m, average.Value);
    }
}
