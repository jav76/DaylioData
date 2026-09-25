using DaylioData.Models;

namespace DaylioData.Repo;

/// <summary>
/// <see cref="DaylioDataRepo"/> Repository for Daylio data read from a CSV file.
/// </summary>
public class DaylioDataRepo
{
    private IEnumerable<DaylioCSVDataModel>? _CSVData;
    private readonly DaylioFileAccess? _fileAccess;
    private readonly Dictionary<string, short> _defaultMoods = new(StringComparer.OrdinalIgnoreCase)
    {
        { "rad", 5 },
        { "good", 4 },
        { "meh", 3 },
        { "bad", 2 },
        { "awful", 1 }
    };

    public IEnumerable<DaylioCSVDataModel>? CSVData => _CSVData;
    public HashSet<string> Activities { get; private set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, short?> Moods { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

    internal DaylioDataRepo(DaylioFileAccess fileAccess)
    {
        _fileAccess = fileAccess;
        _CSVData = _fileAccess.TryReadFile();
        InitializeActivities();
        InitializeMoods();
    }

    private DaylioDataRepo(DaylioFileAccess fileAccess, IEnumerable<DaylioCSVDataModel>? csvData)
    {
        _fileAccess = fileAccess;
        _CSVData = csvData;
        InitializeActivities();
        InitializeMoods();
    }

    internal static async Task<DaylioDataRepo> CreateAsync(
        DaylioFileAccess fileAccess,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<DaylioCSVDataModel>? csvData = await fileAccess.TryReadFileAsync(cancellationToken);
        return new DaylioDataRepo(fileAccess, csvData);
    }

    public void UpdateFile(string filePath)
    {
        _fileAccess?.SetFilePath(filePath);
        _CSVData = _fileAccess?.TryReadFile();
        Activities.Clear();
        Moods.Clear();
        InitializeActivities();
        InitializeMoods();
    }

    public async Task UpdateFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        _fileAccess?.SetFilePath(filePath);
        _CSVData = _fileAccess is null
            ? null
            : await _fileAccess.TryReadFileAsync(cancellationToken);
        Activities.Clear();
        Moods.Clear();
        InitializeActivities();
        InitializeMoods();
    }

    /// <summary>
    /// Used to set custom mood levels.
    /// </summary>
    /// <param name="moodName">The name of the mood to set a level for.</param>
    /// <param name="moodLevel">The level to set for the mood.</param>
    public void SetMoodLevel(string moodName, short? moodLevel)
    {
        if (Moods.ContainsKey(moodName))
        {
            Moods[moodName] = moodLevel;
        }
    }

    /// <summary>
    /// Sets mood levels based on Daylio's default moods without removing custom moods. <br></br>
    /// Rad - 5, Good - 4, Meh - 3, Bad - 2, Awful - 1
    /// </summary>
    public void SetDefaultMoodLevels()
    {
        foreach (KeyValuePair<string, short> mood in _defaultMoods)
        {
            if (Moods.ContainsKey(mood.Key))
            {
                Moods[mood.Key] = mood.Value;
            }
        }
    }

    /// <summary>
    /// Moods can be customized and can be any string. This will keep track of all unique moods.
    /// </summary>
    private void InitializeMoods()
    {
        if (_CSVData is null)
        {
            return;
        }

        foreach (DaylioCSVDataModel entry in _CSVData)
        {
            if (!string.IsNullOrWhiteSpace(entry.Mood) && !Moods.ContainsKey(entry.Mood))
            {
                Moods.Add(entry.Mood, null);
            }
        }
    }

    /// <summary>
    /// There can be any number of custom activities. This will keep track of all unique activities.
    /// </summary>
    private void InitializeActivities()
    {
        if (_CSVData is null)
        {
            return;
        }

        foreach (DaylioCSVDataModel entry in _CSVData)
        {
            foreach (string activity in entry.ActivitiesCollection)
            {
                Activities.Add(activity);
            }
        }
    }
}
