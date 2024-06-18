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

        public IEnumerable<DaylioCSVDataModel>? CSVData => _CSVData;
        public HashSet<string> Activities = new HashSet<string>();
        public HashSet<string> Moods = new HashSet<string>();

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
            InitializeActivities();
            InitializeMoods();
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
                    Moods.Add(mood);
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
