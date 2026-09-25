using System.Globalization;
using CsvHelper.Configuration.Attributes;

namespace DaylioData.Models;

/// <summary>
/// <see cref="DaylioCSVDataModel"/> is an immutable model for deserialized Daylio CSV data.
/// </summary>
public sealed record DaylioCSVDataModel : IEquatable<DaylioCSVDataModel>
{
    private readonly string? _activities;
    private readonly IReadOnlyList<string>? _activitiesCollection;

    [Index(0)]
    public required DateOnly FullDate { get; init; }

    [Index(1)]
    public required DateOnly Date { get; init; }

    [Index(2)]
    public required string? Weekday { get; init; }

    [Index(3)]
    public required TimeOnly Time { get; init; }

    [Index(4)]
    public required string Mood { get; init; }

    [Index(5)]
    public string? Activities
    {
        get => _activities;
        init
        {
            _activities = value;
            _activitiesCollection = string.IsNullOrWhiteSpace(value)
                ? Array.Empty<string>()
                : value.Split(" | ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }
    }

    [Index(6)]
    public string? NoteTitle { get; init; }

    [Index(7)]
    public string? Note { get; init; }

    [Ignore]
    public DateTime Timestamp => FullDate.ToDateTime(Time);

    [Ignore]
    public IReadOnlyList<string> ActivitiesCollection =>
        _activitiesCollection ?? Array.Empty<string>();

    public override string ToString() =>
        string.Create(
            CultureInfo.InvariantCulture,
            $"{FullDate:yyyy-MM-dd},{Date.DayNumber}-{Date.Month},{Weekday},{Time:HH:mm},{Mood},{Activities},{NoteTitle},{Note}");

    public bool Equals(DaylioCSVDataModel? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return FullDate == other.FullDate &&
               Date == other.Date &&
               string.Equals(Weekday, other.Weekday, StringComparison.Ordinal) &&
               Time == other.Time &&
               string.Equals(Mood, other.Mood, StringComparison.Ordinal) &&
               string.Equals(Activities, other.Activities, StringComparison.Ordinal) &&
               string.Equals(NoteTitle, other.NoteTitle, StringComparison.Ordinal) &&
               string.Equals(Note, other.Note, StringComparison.Ordinal);
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(FullDate);
        hash.Add(Date);
        hash.Add(Weekday);
        hash.Add(Time);
        hash.Add(Mood);
        hash.Add(Activities);
        hash.Add(NoteTitle);
        hash.Add(Note);
        return hash.ToHashCode();
    }
}
