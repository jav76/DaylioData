using DaylioParser;
using DaylioParser.Models;
using DaylioParser.Repo;
using System.Reflection;
using System.Text;

namespace DaylioData.Models
{
    public class DaylioDataSummary
    {

        private DaylioDataRepo _daylioDataRepo;

        [SummaryProperty]
        public int TotalEntries =>
            _daylioDataRepo.CSVData?.Count() ?? 0;

        [SummaryProperty]
        public int TotalDays =>
            _daylioDataRepo.CSVData?.Select(x => x.FullDate)
                .Distinct()
                .Count()
                ?? 0;

        [SummaryProperty]
        public int DistinctActivitiesCount =>
            _daylioDataRepo.Activities.Count();

        [SummaryProperty]
        public int TotalActivitiesCount =>
            _daylioDataRepo.CSVData?.Sum(x => x.Activities?.Split(' ').Length ?? 0)
                ?? 0;

        [SummaryProperty]
        public DaylioCSVDataModel? EarliestEntry =>
            _daylioDataRepo.CSVData?.OrderBy(x => x.FullDate)
                .First();

        [SummaryProperty]
        public DaylioCSVDataModel? LatestEntry =>
            _daylioDataRepo.CSVData?.OrderBy(x => x.FullDate)
                .Last();

        [SummaryProperty]
        public int NoteTotalWordCount =>
            _daylioDataRepo.CSVData?.Sum(x => x.Note?.Split(' ').Length ?? 0)
                ?? 0;

        [SummaryProperty]
        public double AverageEntriesPerDay => TotalEntries / (double)TotalDays;

        public DaylioDataSummary(DaylioDataRepo daylioData)
        {
            _daylioDataRepo = daylioData;
        }

        public string GetSummary()
        {
            StringBuilder sb = new StringBuilder();
            IEnumerable<PropertyInfo> properties = typeof(DaylioDataSummary).GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(SummaryPropertyAttribute)));

            foreach (PropertyInfo property in properties)
            {
                sb.AppendLine($"{property.Name}: {property.GetValue(this)}");
            }

            return sb.ToString();
        }

    }
}
