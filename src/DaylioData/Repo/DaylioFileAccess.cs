using CsvHelper;
using DaylioData.Models;
using System.Globalization;

namespace DaylioData.Repo
{
    /// <summary>
    /// <see cref="DaylioFileAccess"/> is used for reading and parsing Daylio CSV data into memory.
    /// </summary>
    public class DaylioFileAccess
    {
        private const string FULL_DATE_HEADER = "full_date";
        private const string DATE_HEADER = "date";
        private const string WEEKDAY_HEADER = "weekday";
        private const string TIME_HEADER = "time";
        private const string MOOD_HEADER = "mood";
        private const string ACTIVITIES_HEADER = "activities";
        private const string NOTE_TITLE_HEADER = "note_title";
        private const string NOTE_HEADER = "note";
        
        private string _filePath = string.Empty;

        public static HashSet<string> CSVHeaders = new HashSet<string>()
        {
            FULL_DATE_HEADER,
            DATE_HEADER,
            WEEKDAY_HEADER,
            TIME_HEADER,
            MOOD_HEADER,
            ACTIVITIES_HEADER,
            NOTE_TITLE_HEADER,
            NOTE_HEADER
        };

        public DaylioFileAccess(string filePath)
        {
            _filePath = filePath;
        }

        public IEnumerable<DaylioCSVDataModel>? TryReadFile()
        {
            List<DaylioCSVDataModel> CSVData = new List<DaylioCSVDataModel>();
            CsvHelper.Configuration.IReaderConfiguration readerConfig = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ",",
                IgnoreBlankLines = true,
                TrimOptions = CsvHelper.Configuration.TrimOptions.Trim,
                BadDataFound = null,
                PrepareHeaderForMatch = args => args.Header.ToLower().Replace("_", string.Empty) // Conversion from snake_case headers
            };

            try
            {
                using (StreamReader streamReader = new StreamReader(_filePath))
                {
                    using (var CSVReader = new CsvReader(streamReader, readerConfig))
                    {
                        CSVReader.Read();
                        CSVReader.ReadHeader();
                        IEnumerable<DaylioCSVDataModel>readHeader = CSVReader.GetRecords<DaylioCSVDataModel>();
                        while (CSVReader.Read())
                        {
                            CSVData.Add(CSVReader.GetRecord<DaylioCSVDataModel>());
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            catch (InvalidDataException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            return CSVData;
        }

    }
}
