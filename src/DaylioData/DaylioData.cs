using DaylioData.Models;
using DaylioParser.Repo;

namespace DaylioData
{
    public class DaylioData
    {
        private DaylioDataRepo _dataRepo;
        private DaylioDataSummary _dataSummary;

        public DaylioDataSummary DataSummary => _dataSummary;

        public DaylioData(string filePath)
        {
            _dataRepo = new DaylioDataRepo(new DaylioFileAccess(filePath));
            _dataSummary = new DaylioDataSummary(_dataRepo);
        }
    }
}
