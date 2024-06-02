using DaylioParser;
using DaylioParser.Models;
using DaylioParser.Repo;
using System.Reflection;
using System.Text;

namespace DaylioData.Models
{
    /// <summary>
    /// <see cref="DaylioDataSummary"/> contains various summary properties for Daylio data.
    /// </summary>
    public class DaylioDataSummary
    {

        private DaylioDataRepo _daylioDataRepo;

        /// <summary>
        /// Number of total entries.
        /// </summary>
        [SummaryProperty]
        public int TotalEntries =>
            _daylioDataRepo.CSVData?.Count() ?? 0;

        /// <summary>
        /// Number of total days with entries.
        /// </summary>
        [SummaryProperty]
        public int TotalDays =>
            _daylioDataRepo.CSVData?.Select(x => x.FullDate)
                .Distinct()
                .Count()
                ?? 0;

        /// <summary>
        /// Number of distinct activities in all entries.
        /// </summary>
        [SummaryProperty]
        public int DistinctActivitiesCount =>
            _daylioDataRepo.Activities.Count();

        /// <summary>
        /// Total count of all activities in all entries.
        /// </summary>
        [SummaryProperty]
        public int TotalActivitiesCount =>
            _daylioDataRepo.CSVData?.Sum(x => x.Activities?.Split(' ').Length ?? 0)
                ?? 0;
        
        /// <summary>
        /// The earliest <see cref="DaylioCSVDataModel"/> entry.
        /// </summary>
        [SummaryProperty]
        public DaylioCSVDataModel? EarliestEntry =>
            _daylioDataRepo.CSVData?.OrderBy(x => x.FullDate)
                .First();

        /// <summary>
        /// The latest <see cref="DaylioCSVDataModel"/> entry.
        /// </summary>
        [SummaryProperty]
        public DaylioCSVDataModel? LatestEntry =>
            _daylioDataRepo.CSVData?.OrderBy(x => x.FullDate)
                .Last();

        /// <summary>
        /// The total word count of all notes in all entries.
        /// </summary>
        [SummaryProperty]
        public int NoteTotalWordCount =>
            _daylioDataRepo.CSVData?.Sum(x => x.Note?.Split(' ').Length ?? 0)
                ?? 0;

        /// <summary>
        /// The average number of entries per day.
        /// </summary>
        [SummaryProperty]
        public double AverageEntriesPerDay => TotalEntries / (double)TotalDays;

        public DaylioDataSummary(DaylioDataRepo daylioData)
        {
            _daylioDataRepo = daylioData;
        }

        /// <summary>
        /// Used to get a string of all <see cref="SummaryPropertyAttribute"/> properties.
        /// </summary>
        /// <returns>A string of lines of {PropertyName}: {PropertyValue}</returns>
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
