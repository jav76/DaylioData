using System.Diagnostics.CodeAnalysis;
using DaylioData.Models;
using DaylioData.Repo;

namespace DaylioData;

/// <summary>
/// Primary entry point and façade for accessing, querying, and analyzing Daylio data.
/// </summary>
public class DaylioData
{
    private readonly DaylioDataRepo _dataRepo;
    private readonly DaylioDataSummary _dataSummary;

    public DaylioDataSummary DataSummary => _dataSummary;
    public DaylioDataRepo DataRepo => _dataRepo;

    /// <summary>
    /// Initializes a new instance of the <see cref="DaylioData"/> class from a file path.
    /// </summary>
    /// <param name="filePath">Path of a Daylio CSV file.</param>
    /// <exception cref="ArgumentException">Thrown when filePath is null or whitespace.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
    /// <exception cref="InvalidDataException">Thrown when the CSV content cannot be parsed.</exception>
    public DaylioData(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be null or whitespace.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Daylio CSV file not found at path: '{filePath}'.", filePath);
        }

        _dataRepo = new(new DaylioFileAccess(filePath), failFast: true);
        _dataSummary = new(_dataRepo);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DaylioData"/> class from a <see cref="TextReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/> providing CSV data.</param>
    /// <exception cref="ArgumentNullException">Thrown when reader is null.</exception>
    /// <exception cref="InvalidDataException">Thrown when the CSV content cannot be parsed.</exception>
    public DaylioData(TextReader reader)
    {
        ArgumentNullException.ThrowIfNull(reader);

        _dataRepo = new(new DaylioFileAccess(reader), failFast: true);
        _dataSummary = new(_dataRepo);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DaylioData"/> class from a <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> containing CSV data.</param>
    /// <exception cref="ArgumentNullException">Thrown when stream is null.</exception>
    /// <exception cref="InvalidDataException">Thrown when the CSV content cannot be parsed.</exception>
    public DaylioData(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        _dataRepo = new(new DaylioFileAccess(stream), failFast: true);
        _dataSummary = new(_dataRepo);
    }

    internal DaylioData(DaylioDataRepo repo)
    {
        _dataRepo = repo;
        _dataSummary = new(_dataRepo);
    }

    /// <summary>
    /// Attempts to load Daylio data from a file without throwing file or parsing exceptions.
    /// </summary>
    /// <param name="filePath">Path of a Daylio CSV file.</param>
    /// <param name="daylioData">When successful, contains the initialized <see cref="DaylioData"/> instance.</param>
    /// <returns><c>true</c> if the file was successfully loaded and parsed; otherwise, <c>false</c>.</returns>
    public static bool TryLoad(string filePath, [NotNullWhen(true)] out DaylioData? daylioData)
    {
        daylioData = null;

        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return false;
        }

        try
        {
            DaylioFileAccess fileAccess = new(filePath);
            IReadOnlyList<DaylioCSVDataModel>? records = fileAccess.TryReadFile();
            if (records is null)
            {
                return false;
            }

            DaylioDataRepo repo = new(fileAccess, records);
            daylioData = new DaylioData(repo);
            return true;
        }
        catch (Exception)
        {
            daylioData = null;
            return false;
        }
    }

    /// <summary>
    /// Asynchronously attempts to load Daylio data from a file without throwing file or parsing exceptions.
    /// </summary>
    /// <param name="filePath">Path of a Daylio CSV file.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A tuple containing a success flag and the loaded <see cref="DaylioData"/> instance if successful.</returns>
    public static async Task<(bool Success, DaylioData? Data)> TryLoadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
        {
            return (false, null);
        }

        try
        {
            DaylioFileAccess fileAccess = new(filePath);
            DaylioDataRepo? repo = await DaylioDataRepo.TryCreateAsync(fileAccess, cancellationToken);
            if (repo is null)
            {
                return (false, null);
            }

            return (true, new DaylioData(repo));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return (false, null);
        }
    }

    /// <summary>
    /// Asynchronously initializes a new instance of the <see cref="DaylioData"/> class from a file path.
    /// </summary>
    /// <param name="filePath">Path of a Daylio CSV file.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task yielding the initialized <see cref="DaylioData"/>.</returns>
    public static async Task<DaylioData> LoadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be null or whitespace.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Daylio CSV file not found at path: '{filePath}'.", filePath);
        }

        DaylioFileAccess fileAccess = new(filePath);
        DaylioDataRepo repo = await DaylioDataRepo.CreateAsync(fileAccess, cancellationToken);
        return new DaylioData(repo);
    }

    /// <summary>
    /// Asynchronously initializes a new instance of the <see cref="DaylioData"/> class from a <see cref="TextReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/> providing CSV data.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task yielding the initialized <see cref="DaylioData"/>.</returns>
    public static async Task<DaylioData> LoadAsync(
        TextReader reader,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(reader);

        DaylioFileAccess fileAccess = new(reader);
        DaylioDataRepo repo = await DaylioDataRepo.CreateAsync(fileAccess, cancellationToken);
        return new DaylioData(repo);
    }

    /// <summary>
    /// Asynchronously initializes a new instance of the <see cref="DaylioData"/> class from a <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> containing CSV data.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task yielding the initialized <see cref="DaylioData"/>.</returns>
    public static async Task<DaylioData> LoadAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        DaylioFileAccess fileAccess = new(stream);
        DaylioDataRepo repo = await DaylioDataRepo.CreateAsync(fileAccess, cancellationToken);
        return new DaylioData(repo);
    }

    /// <summary>
    /// Gets the <see cref="DaylioCSVDataModel"/> with the earliest entry date, or null if empty.
    /// </summary>
    public DaylioCSVDataModel? GetEarliestEntry() => _dataSummary.EarliestEntry;

    /// <summary>
    /// Gets the <see cref="DaylioCSVDataModel"/> with the latest entry date, or null if empty.
    /// </summary>
    public DaylioCSVDataModel? GetLatestEntry() => _dataSummary.LatestEntry;

    /// <summary>
    /// Gets entries within a specified date range (inclusive).
    /// </summary>
    /// <param name="startDate">The earliest (inclusive) <see cref="DateTime"/> of entries.</param>
    /// <param name="endDate">The latest (inclusive) <see cref="DateTime"/> of entries.</param>
    /// <returns>A read-only list of entries within the specified range.</returns>
    public IReadOnlyList<DaylioCSVDataModel> GetEntriesInRange(DateTime startDate, DateTime endDate)
    {
        return _dataRepo.CSVData
            .Where(entry => entry.Timestamp >= startDate && entry.Timestamp <= endDate)
            .ToList();
    }

    /// <summary>
    /// Gets entries that include the specified activity.
    /// </summary>
    /// <param name="activity">The activity name to search for (case-insensitive).</param>
    /// <returns>A read-only list of matching entries, or empty if the activity was not found.</returns>
    public IReadOnlyList<DaylioCSVDataModel> GetEntriesWithActivity(string activity)
    {
        if (string.IsNullOrWhiteSpace(activity) || !_dataRepo.Activities.Contains(activity))
        {
            return Array.Empty<DaylioCSVDataModel>();
        }

        return _dataRepo.CSVData
            .Where(entry => entry.ActivitiesCollection
                .Any(entryActivity => entryActivity.Equals(activity, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    /// <summary>
    /// Gets entries that have the specified mood.
    /// </summary>
    /// <param name="mood">The mood name to search for (case-insensitive).</param>
    /// <returns>A read-only list of matching entries, or empty if the mood was not found.</returns>
    public IReadOnlyList<DaylioCSVDataModel> GetEntriesWithMood(string mood)
    {
        if (string.IsNullOrWhiteSpace(mood) || !_dataRepo.Moods.ContainsKey(mood))
        {
            return Array.Empty<DaylioCSVDataModel>();
        }

        return _dataRepo.CSVData
            .Where(entry => entry.Mood.Equals(mood, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Gets the number of entries that include the specified activity.
    /// </summary>
    /// <param name="activity">The activity name to count (case-insensitive).</param>
    /// <returns>The number of entries containing the activity, or 0 if not found.</returns>
    public int GetActivityCount(string activity)
    {
        if (string.IsNullOrWhiteSpace(activity) || !_dataRepo.Activities.Contains(activity))
        {
            return 0;
        }

        return _dataRepo.CSVData.Count(entry => entry.ActivitiesCollection
            .Any(entryActivity => entryActivity.Equals(activity, StringComparison.OrdinalIgnoreCase)));
    }

    /// <summary>
    /// Gets entries that contain the search string in their note or note title.
    /// </summary>
    /// <param name="searchString">The string to search for.</param>
    /// <param name="comparisonMethod">The <see cref="StringComparison"/> method to use.</param>
    /// <returns>A read-only list of matching entries.</returns>
    public IReadOnlyList<DaylioCSVDataModel> GetEntriesWithString(
        string searchString,
        StringComparison comparisonMethod = StringComparison.CurrentCulture)
    {
        if (string.IsNullOrEmpty(searchString))
        {
            return Array.Empty<DaylioCSVDataModel>();
        }

        return _dataRepo.CSVData
            .Where(entry =>
                (!string.IsNullOrWhiteSpace(entry.Note) && entry.Note.Contains(searchString, comparisonMethod)) ||
                (!string.IsNullOrWhiteSpace(entry.NoteTitle) && entry.NoteTitle.Contains(searchString, comparisonMethod)))
            .ToList();
    }

    /// <summary>
    /// Gets an average mood rating for a specified activity based on configured mood levels.
    /// </summary>
    /// <param name="activity">The activity name to get an average rating for.</param>
    /// <returns>The average mood rating, or null if no mood levels are configured for the matching entries.</returns>
    public decimal? GetAverageActivityMood(string activity)
    {
        if (string.IsNullOrWhiteSpace(activity) || !_dataRepo.Activities.Contains(activity))
        {
            return null;
        }

        long moodSum = 0;
        uint count = 0;

        foreach (DaylioCSVDataModel entry in _dataRepo.CSVData)
        {
            if (entry.ActivitiesCollection.Any(entryActivity =>
                entryActivity.Equals(activity, StringComparison.OrdinalIgnoreCase)))
            {
                if (_dataRepo.Moods.TryGetValue(entry.Mood, out short? moodLevel) && moodLevel.HasValue)
                {
                    moodSum += moodLevel.Value;
                    count++;
                }
            }
        }

        return count == 0 ? null : (decimal)moodSum / count;
    }
}
