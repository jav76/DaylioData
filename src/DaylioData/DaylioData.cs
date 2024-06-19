using DaylioData.Models;
using DaylioData.Repo;

namespace DaylioData
{
    /// <summary>
    /// Base data access class for Daylio data.
    /// </summary>
    public class DaylioData
    {
        private DaylioDataRepo? _dataRepo;
        private DaylioDataSummary? _dataSummary;

        public DaylioDataSummary? DataSummary => _dataSummary;
        public DaylioDataRepo? DataRepo => _dataRepo;

        /// <summary>
        /// Initializes a new instance of the <see cref="DaylioData"/> class.
        /// </summary>
        /// <param name="filePath"> Path of a Daylio CSV file to be used to initialize <see cref="DaylioDataRepo"/>.</param>
        public DaylioData(string filePath)
        {
            _dataRepo = new DaylioDataRepo(new DaylioFileAccess(filePath));
            _dataSummary = new DaylioDataSummary(_dataRepo);
            Methods.InitData(this);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DaylioData"/> class.
        /// </summary>
        public DaylioData()
        {

        }
    }
}
