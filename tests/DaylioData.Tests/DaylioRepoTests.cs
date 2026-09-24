namespace DaylioData.Tests;

public class DaylioRepoTests
{
    private const string PARTIAL_MOODS_CSV =
        "full_date,date,weekday,time,mood,activities,note_title,note\n" +
        "2023-01-01,2023-01-01,Sunday,10:00,good,walking,Title,Note\n" +
        "2023-01-02,2023-01-02,Monday,12:00,rad,running,Title,Note\n" +
        "2023-01-03,2023-01-03,Tuesday,14:00,ecstatic,celebrating,Title,Note\n";

    [Fact]
    public void SetDefaultMoodLevels_WhenSomeDefaultMoodsMissing_DoesNotThrow()
    {
        using StringReader reader = new(PARTIAL_MOODS_CSV);
        DaylioData daylioData = new(reader);

        daylioData.DataRepo!.SetDefaultMoodLevels();

        Assert.Equal((short)4, daylioData.DataRepo.Moods["good"]);
        Assert.Equal((short)5, daylioData.DataRepo.Moods["rad"]);
    }

    [Fact]
    public void SetDefaultMoodLevels_PreservesCustomMoods()
    {
        using StringReader reader = new(PARTIAL_MOODS_CSV);
        DaylioData daylioData = new(reader);

        daylioData.DataRepo!.SetMoodLevel("ecstatic", 6);
        daylioData.DataRepo.SetDefaultMoodLevels();

        Assert.True(daylioData.DataRepo.Moods.ContainsKey("ecstatic"));
        Assert.Equal((short)6, daylioData.DataRepo.Moods["ecstatic"]);
    }

    [Fact]
    public void SetMoodLevel_CaseInsensitive_UpdatesLevel()
    {
        using StringReader reader = new(PARTIAL_MOODS_CSV);
        DaylioData daylioData = new(reader);

        daylioData.DataRepo!.SetMoodLevel("GOOD", 4);

        Assert.Equal((short)4, daylioData.DataRepo.Moods["good"]);
    }

    [Fact]
    public void Activities_CaseInsensitiveLookup()
    {
        using StringReader reader = new(PARTIAL_MOODS_CSV);
        DaylioData daylioData = new(reader);

        Assert.True(daylioData.DataRepo!.Activities.Contains("WALKING"));
        Assert.True(daylioData.DataRepo.Activities.Contains("running"));
    }
}
