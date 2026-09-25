using DaylioData.Models;

namespace DaylioData.Repo;

/// <summary>
/// <see cref="DaylioDataRepo"/> Repository for Daylio data read from a CSV file.
/// </summary>
public class DaylioDataRepo
{
    private IReadOnlyList<DaylioCSVDataModel> _CSVData = Array.Empty<DaylioCSVDataModel>();
    private readonly DaylioFileAccess _fileAccess;
    private readonly Dictionary<string, short> _defaultMoods = new(StringComparer.OrdinalIgnoreCase)
    {
        { "rad", 5 },
        { "good", 4 },
        { "meh", 3 },
        { "bad", 2 },
        { "awful", 1 }
    };

    internal event Action? DataChanged;

    public IReadOnlyList<DaylioCSVDataModel> CSVData => _CSVData;
    public HashSet<string> Activities { get; private set; } = new(StringComparer.OrdinalIgnoreCase);
    public Dictionary<string, short?> Moods { get; private set; } = new(StringComparer.OrdinalIgnoreCase);

    internal DaylioDataRepo(DaylioFileAccess fileAccess, bool failFast = true)
    {
        _fileAccess = fileAccess;
        _CSVData = failFast
            ? _fileAccess.ReadFile()
            : _fileAccess.TryReadFile() ?? Array.Empty<DaylioCSVDataModel>();
        InitializeActivities();
        InitializeMoods();
    }

    internal DaylioDataRepo(DaylioFileAccess fileAccess, IReadOnlyList<DaylioCSVDataModel>? csvData)
    {
        _fileAccess = fileAccess;
        _CSVData = csvData ?? Array.Empty<DaylioCSVDataModel>();
        InitializeActivities();
        InitializeMoods();
    }

    internal static async Task<DaylioDataRepo> CreateAsync(
        DaylioFileAccess fileAccess,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DaylioCSVDataModel> csvData = await fileAccess.ReadFileAsync(cancellationToken);
        return new DaylioDataRepo(fileAccess, csvData);
    }

    internal static async Task<DaylioDataRepo?> TryCreateAsync(
        DaylioFileAccess fileAccess,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DaylioCSVDataModel>? csvData = await fileAccess.TryReadFileAsync(cancellationToken);
        return csvData is null ? null : new DaylioDataRepo(fileAccess, csvData);
    }

    public void UpdateFile(string filePath)
    {
        _fileAccess.SetFilePath(filePath);
        _CSVData = _fileAccess.TryReadFile() ?? Array.Empty<DaylioCSVDataModel>();
        Activities.Clear();
        Moods.Clear();
        InitializeActivities();
        InitializeMoods();
        DataChanged?.Invoke();
    }

    public async Task UpdateFileAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        _fileAccess.SetFilePath(filePath);
        _CSVData = (await _fileAccess.TryReadFileAsync(cancellationToken)) ?? Array.Empty<DaylioCSVDataModel>();
        Activities.Clear();
        Moods.Clear();
        InitializeActivities();
        InitializeMoods();
        DataChanged?.Invoke();
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
        foreach (DaylioCSVDataModel entry in _CSVData)
        {
            foreach (string activity in entry.ActivitiesCollection)
            {
                Activities.Add(activity);
            }
        }
    }
}
