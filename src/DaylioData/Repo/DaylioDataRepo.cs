using DaylioParser.Models;

namespace DaylioParser.Repo
{
    public class DaylioDataRepo
    {

        private IEnumerable<DaylioCSVDataModel>? _CSVData;

        public IEnumerable<DaylioCSVDataModel>? CSVData => _CSVData;
        public HashSet<string> Activities = new HashSet<string>();

        public DaylioDataRepo(DaylioFileAccess fileAccess)
        {

            _CSVData = fileAccess.TryReadFile();
            InitializeActivities();
        }

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
