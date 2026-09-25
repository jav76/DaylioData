using System.Globalization;
using System.Reflection;
using System.Text;
using DaylioData.Repo;

namespace DaylioData.Models;

/// <summary>
/// <see cref="DaylioDataSummary"/> contains cached summary metrics for Daylio data calculated in a single pass.
/// </summary>
public class DaylioDataSummary
{
    private readonly DaylioDataRepo _daylioDataRepo;

    private int _totalEntries;
    private int _totalDays;
    private int _distinctActivitiesCount;
    private int _totalActivitiesCount;
    private DaylioCSVDataModel? _earliestEntry;
    private DaylioCSVDataModel? _latestEntry;
    private int _noteTotalWordCount;
    private double _averageEntriesPerDay;

    /// <summary>
    /// Number of total entries.
    /// </summary>
    [SummaryProperty]
    public int TotalEntries => _totalEntries;

    /// <summary>
    /// Number of total days with entries.
    /// </summary>
    [SummaryProperty]
    public int TotalDays => _totalDays;

    /// <summary>
    /// Number of distinct activities in all entries.
    /// </summary>
    [SummaryProperty]
    public int DistinctActivitiesCount => _distinctActivitiesCount;

    /// <summary>
    /// Total count of all activities in all entries.
    /// </summary>
    [SummaryProperty]
    public int TotalActivitiesCount => _totalActivitiesCount;

    /// <summary>
    /// The earliest <see cref="DaylioCSVDataModel"/> entry.
    /// </summary>
    [SummaryProperty]
    public DaylioCSVDataModel? EarliestEntry => _earliestEntry;

    /// <summary>
    /// The latest <see cref="DaylioCSVDataModel"/> entry.
    /// </summary>
    [SummaryProperty]
    public DaylioCSVDataModel? LatestEntry => _latestEntry;

    /// <summary>
    /// The total word count of all notes in all entries.
    /// </summary>
    [SummaryProperty]
    public int NoteTotalWordCount => _noteTotalWordCount;

    /// <summary>
    /// The average number of entries per day.
    /// </summary>
    [SummaryProperty]
    public double AverageEntriesPerDay => _averageEntriesPerDay;

    public DaylioDataSummary(DaylioDataRepo daylioData)
    {
        _daylioDataRepo = daylioData;
        _daylioDataRepo.DataChanged += Refresh;
        ComputeMetrics();
    }

    /// <summary>
    /// Recalculates all summary metrics from the underlying repository.
    /// </summary>
    public void Refresh()
    {
        ComputeMetrics();
    }

    private void ComputeMetrics()
    {
        IReadOnlyList<DaylioCSVDataModel> records = _daylioDataRepo.CSVData;
        _totalEntries = records.Count;

        if (records.Count == 0)
        {
            _totalDays = 0;
            _distinctActivitiesCount = _daylioDataRepo.Activities.Count;
            _totalActivitiesCount = 0;
            _earliestEntry = null;
            _latestEntry = null;
            _noteTotalWordCount = 0;
            _averageEntriesPerDay = 0.0;
            return;
        }

        HashSet<DateOnly> distinctDays = new();
        int totalActivities = 0;
        int totalWordCount = 0;
        DaylioCSVDataModel earliest = records[0];
        DaylioCSVDataModel latest = records[0];

        foreach (DaylioCSVDataModel entry in records)
        {
            distinctDays.Add(entry.FullDate);
            totalActivities += entry.ActivitiesCollection.Count;

            if (!string.IsNullOrWhiteSpace(entry.Note))
            {
                totalWordCount += entry.Note.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            }

            if (entry.Timestamp < earliest.Timestamp)
            {
                earliest = entry;
            }

            if (entry.Timestamp > latest.Timestamp)
            {
                latest = entry;
            }
        }

        _totalDays = distinctDays.Count;
        _distinctActivitiesCount = _daylioDataRepo.Activities.Count;
        _totalActivitiesCount = totalActivities;
        _earliestEntry = earliest;
        _latestEntry = latest;
        _noteTotalWordCount = totalWordCount;
        _averageEntriesPerDay = _totalDays == 0 ? 0.0 : _totalEntries / (double)_totalDays;
    }

    /// <summary>
    /// Used to get a string of all <see cref="SummaryPropertyAttribute"/> properties.
    /// </summary>
    /// <returns>A string of lines of {PropertyName}: {PropertyValue}</returns>
    public string GetSummary()
    {
        StringBuilder sb = new();
        IEnumerable<PropertyInfo> properties = typeof(DaylioDataSummary).GetProperties()
            .Where(x => Attribute.IsDefined(x, typeof(SummaryPropertyAttribute)));

        foreach (PropertyInfo property in properties)
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"{property.Name}: {property.GetValue(this)}");
        }

        return sb.ToString();
    }
}
