using CsvHelper.Configuration.Attributes;

namespace DaylioData.Models;

/// <summary>
/// <see cref="DaylioCSVDataModel"/> is a model for deserialized Daylio CSV data.
/// </summary>
public class DaylioCSVDataModel
{
    [Index(0)]
    public required DateOnly FullDate { get; set; }

    [Index(1)]
    public required DateOnly Date { get; set; }

    [Index(2)]
    public required string? Weekday { get; set; }

    [Index(3)]
    public required TimeOnly Time { get; set; }

    [Index(4)]
    public required string Mood { get; set; }

    [Index(5)]
    public string? Activities { get; set; }

    [Index(6)]
    public string? NoteTitle { get; set; }

    [Index(7)]
    public string? Note { get; set; }

    [Ignore]
    public DateTime Timestamp => FullDate.ToDateTime(Time);

    public override string ToString() =>
        $"{FullDate.ToShortDateString()},{Date.DayNumber}-{Date.Month},{Weekday},{Time},{Mood},{Activities},{NoteTitle},{Note}";

    [Ignore]
    public IReadOnlyList<string> ActivitiesCollection =>
        string.IsNullOrWhiteSpace(Activities)
            ? Array.Empty<string>()
            : Activities.Split(" | ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
