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
    private TextReader? _textReader;

    internal DaylioFileAccess(string filePath)
    {
        _filePath = filePath;
    }

    internal DaylioFileAccess(TextReader textReader)
    {
        _textReader = textReader;
    }

    internal DaylioFileAccess(Stream stream)
    {
        _textReader = new StreamReader(stream);
    }

    internal void SetFilePath(string filePath)
    {
        _filePath = filePath;
        _textReader = null;
    }

    public static readonly HashSet<string> CSVHeaders = new()
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

    internal IReadOnlyList<DaylioCSVDataModel> ReadFile()
    {
        if (_textReader is not null)
        {
            return Read(_textReader);
        }

        if (string.IsNullOrWhiteSpace(_filePath))
        {
            throw new ArgumentException("File path must not be null or whitespace.", nameof(_filePath));
        }

        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException($"Daylio CSV file not found at path: '{_filePath}'.", _filePath);
        }

        using StreamReader streamReader = new(_filePath);
        return Read(streamReader);
    }

    internal async Task<IReadOnlyList<DaylioCSVDataModel>> ReadFileAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_textReader is not null)
        {
            return await ReadAsync(_textReader, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(_filePath))
        {
            throw new ArgumentException("File path must not be null or whitespace.", nameof(_filePath));
        }

        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException($"Daylio CSV file not found at path: '{_filePath}'.", _filePath);
        }

        using StreamReader streamReader = new(_filePath);
        return await ReadAsync(streamReader, cancellationToken);
    }

    internal IReadOnlyList<DaylioCSVDataModel>? TryReadFile()
    {
        if (_textReader is not null)
        {
            return TryRead(_textReader);
        }

        if (string.IsNullOrWhiteSpace(_filePath) || !File.Exists(_filePath))
        {
            return null;
        }

        try
        {
            using StreamReader streamReader = new(_filePath);
            return TryRead(streamReader);
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static IReadOnlyList<DaylioCSVDataModel> Read(TextReader reader)
    {
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
            using CsvReader csvReader = new(reader, readerConfig);
            return csvReader.GetRecords<DaylioCSVDataModel>().ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidDataException("Failed to parse Daylio CSV data from the provided stream or reader.", ex);
        }
    }

    internal static IReadOnlyList<DaylioCSVDataModel>? TryRead(TextReader reader)
    {
        try
        {
            return Read(reader);
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal async Task<IReadOnlyList<DaylioCSVDataModel>?> TryReadFileAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_textReader is not null)
        {
            return await TryReadAsync(_textReader, cancellationToken);
        }

        if (string.IsNullOrWhiteSpace(_filePath) || !File.Exists(_filePath))
        {
            return null;
        }

        try
        {
            using StreamReader streamReader = new(_filePath);
            return await TryReadAsync(streamReader, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return null;
        }
    }

    internal static async Task<IReadOnlyList<DaylioCSVDataModel>> ReadAsync(
        TextReader reader,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

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
            using CsvReader csvReader = new(reader, readerConfig);
            List<DaylioCSVDataModel> records = new();
            await foreach (DaylioCSVDataModel record in csvReader.GetRecordsAsync<DaylioCSVDataModel>(cancellationToken))
            {
                records.Add(record);
            }

            return records;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InvalidDataException("Failed to parse Daylio CSV data from the provided stream or reader.", ex);
        }
    }

    internal static async Task<IReadOnlyList<DaylioCSVDataModel>?> TryReadAsync(
        TextReader reader,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await ReadAsync(reader, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
