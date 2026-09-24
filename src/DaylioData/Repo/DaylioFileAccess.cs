using System.Diagnostics;
using System.Globalization;
using CsvHelper;
using DaylioData.Models;

namespace DaylioData.Repo;

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

    internal DaylioFileAccess(string filePath)
    {
        _filePath = filePath;
    }

    internal void SetFilePath(string filePath) => _filePath = filePath;

    public static HashSet<string> CSVHeaders = new()
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

    internal IEnumerable<DaylioCSVDataModel>? TryReadFile()
    {
        List<DaylioCSVDataModel> CSVData = new();
        CsvHelper.Configuration.CsvConfiguration readerConfig = new(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            Delimiter = ",",
            IgnoreBlankLines = true,
            TrimOptions = CsvHelper.Configuration.TrimOptions.Trim,
            BadDataFound = null,
            PrepareHeaderForMatch = args => args.Header.ToLower(CultureInfo.InvariantCulture).Replace("_", string.Empty)
        };

        try
        {
            using StreamReader streamReader = new(_filePath);
            using CsvReader CSVReader = new(streamReader, readerConfig);
            CSVReader.Read();
            CSVReader.ReadHeader();
            IEnumerable<DaylioCSVDataModel> readHeader = CSVReader.GetRecords<DaylioCSVDataModel>();
            while (CSVReader.Read())
            {
                CSVData.Add(CSVReader.GetRecord<DaylioCSVDataModel>());
            }
        }
        catch (IOException ex)
        {
            Debug.WriteLine(ex.Message);
            return null;
        }
        catch (InvalidDataException ex)
        {
            Debug.WriteLine(ex.Message);
            return null;
        }

        return CSVData;
    }
}
