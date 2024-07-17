using DaylioData.Models;

namespace DaylioData.Repo
{
    /// <summary>
    /// <see cref="DaylioDataRepo"/> Repository for Daylio data read from a CSV file.
    /// </summary>
    public class DaylioDataRepo
    {

        private IEnumerable<DaylioCSVDataModel>? _CSVData;
        private DaylioFileAccess? _fileAccess;
        private readonly Dictionary<string, short> _defaultMoods = new Dictionary<string, short>()
        {
            { "rad", 5 },
            { "good", 4 },
            { "meh", 3 },
            { "bad", 2 },
            { "awful", 1 }
        };

        public IEnumerable<DaylioCSVDataModel>? CSVData => _CSVData;
        public HashSet<string> Activities = new HashSet<string>();
        public Dictionary<string, short?> Moods = new Dictionary<string, short?>();

        internal DaylioDataRepo(DaylioFileAccess fileAccess)
        {
            _fileAccess = fileAccess;
            _CSVData = _fileAccess.TryReadFile();
            InitializeActivities();
            InitializeMoods();
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
        /// Sets mood levels based on Daylio's default moods. This cannot be used with custom moods. <br></br>
        /// Rad - 5, Good - 4, Meh - 3, Bad - 2, Awful - 1
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void SetDefaultMoodLevels()
        {
            foreach (KeyValuePair<string, short> mood in _defaultMoods)
            {
                if (!Moods.ContainsKey(mood.Key))
                {
                    throw new InvalidOperationException("Attempting to assign default mood levels to non-default moods.");
                }
            }

            Moods = _defaultMoods.ToDictionary(x => x.Key, x => (short?)x.Value);
        }

        /// <summary>
        /// Moods can be customized and can be any string. This will keep track of all unique moods.
        /// </summary>
        private void InitializeMoods()
        {
            if (_CSVData == null)
            {
                return;
            }

            foreach (string? mood in _CSVData.Select(x => x.Mood).Distinct())
            {
                if (mood != null)
                {
                    Moods.Add(mood, null);
                }
            }
        }

        /// <summary>
        /// There can be any number of custom activities. This will keep track of all unique activities.
        /// </summary>
        private void InitializeActivities()
        {
            if (_CSVData == null)
            {
                return;
            }

            foreach (string activitiy in _CSVData.Select(x => x.Activities?.Split(" | ")).SelectMany(x => x ?? Array.Empty<string>()))
            {
                Activities.Add(activitiy);
            }
        }
    }
}
