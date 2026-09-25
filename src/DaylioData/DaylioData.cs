using DaylioData.Models;
using DaylioData.Repo;

namespace DaylioData;

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
    /// Initializes a new instance of the <see cref="DaylioData"/> class from a file path.
    /// </summary>
    /// <param name="filePath">Path of a Daylio CSV file to be used to initialize <see cref="DaylioDataRepo"/>.</param>
    public DaylioData(string filePath)
    {
        _dataRepo = new(new DaylioFileAccess(filePath));
        _dataSummary = new(_dataRepo);
#pragma warning disable CS0618
        Methods.InitData(this);
#pragma warning restore CS0618
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DaylioData"/> class from a <see cref="TextReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/> providing CSV data.</param>
    public DaylioData(TextReader reader)
    {
        _dataRepo = new(new DaylioFileAccess(reader));
        _dataSummary = new(_dataRepo);
#pragma warning disable CS0618
        Methods.InitData(this);
#pragma warning restore CS0618
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DaylioData"/> class from a <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> containing CSV data.</param>
    public DaylioData(Stream stream)
    {
        _dataRepo = new(new DaylioFileAccess(stream));
        _dataSummary = new(_dataRepo);
#pragma warning disable CS0618
        Methods.InitData(this);
#pragma warning restore CS0618
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DaylioData"/> class.
    /// </summary>
    public DaylioData()
    {
    }

    /// <summary>
    /// Asynchronously initializes a new instance of the <see cref="DaylioData"/> class from a file path.
    /// </summary>
    /// <param name="filePath">Path of a Daylio CSV file to be used to initialize the repository.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous initialization operation, yielding the initialized <see cref="DaylioData"/>.</returns>
    public static async Task<DaylioData> LoadAsync(
        string filePath,
        CancellationToken cancellationToken = default)
    {
        DaylioFileAccess fileAccess = new(filePath);
        DaylioDataRepo repo = await DaylioDataRepo.CreateAsync(fileAccess, cancellationToken);
        DaylioData instance = new()
        {
            _dataRepo = repo,
            _dataSummary = new(repo)
        };
#pragma warning disable CS0618
        Methods.InitData(instance);
#pragma warning restore CS0618
        return instance;
    }

    /// <summary>
    /// Asynchronously initializes a new instance of the <see cref="DaylioData"/> class from a <see cref="TextReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/> providing CSV data.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous initialization operation, yielding the initialized <see cref="DaylioData"/>.</returns>
    public static async Task<DaylioData> LoadAsync(
        TextReader reader,
        CancellationToken cancellationToken = default)
    {
        DaylioFileAccess fileAccess = new(reader);
        DaylioDataRepo repo = await DaylioDataRepo.CreateAsync(fileAccess, cancellationToken);
        DaylioData instance = new()
        {
            _dataRepo = repo,
            _dataSummary = new(repo)
        };
#pragma warning disable CS0618
        Methods.InitData(instance);
#pragma warning restore CS0618
        return instance;
    }

    /// <summary>
    /// Asynchronously initializes a new instance of the <see cref="DaylioData"/> class from a <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> containing CSV data.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous initialization operation, yielding the initialized <see cref="DaylioData"/>.</returns>
    public static async Task<DaylioData> LoadAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        DaylioFileAccess fileAccess = new(stream);
        DaylioDataRepo repo = await DaylioDataRepo.CreateAsync(fileAccess, cancellationToken);
        DaylioData instance = new()
        {
            _dataRepo = repo,
            _dataSummary = new(repo)
        };
#pragma warning disable CS0618
        Methods.InitData(instance);
#pragma warning restore CS0618
        return instance;
    }
}
