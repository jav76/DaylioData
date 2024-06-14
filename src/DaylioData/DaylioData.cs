using DaylioData.Models;
using DaylioData.Repo;

namespace DaylioData
{
    /// <summary>
    /// Base data access class for Daylio data.
    /// </summary>
    public class DaylioData
    {
        private DaylioDataRepo _dataRepo;
        private DaylioDataSummary _dataSummary;

        public DaylioDataSummary DataSummary => _dataSummary;
        public DaylioDataRepo DataRepo => _dataRepo;

        /// <summary>
        /// Initialize a new <see cref="DaylioData"/> class for a given <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath"> Path of a Daylio CSV file.</param>
        public DaylioData(string filePath)
        {
            _dataRepo = new DaylioDataRepo(new DaylioFileAccess(filePath));
            _dataSummary = new DaylioDataSummary(_dataRepo);
        }
    }
}
