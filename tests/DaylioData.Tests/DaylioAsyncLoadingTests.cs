using System.Text;
using DaylioData.Models;

namespace DaylioData.Tests;

public class DaylioAsyncLoadingTests
{
    private const string SAMPLE_CSV =
        "full_date,date,weekday,time,mood,activities,note_title,note\n" +
        "2023-01-01,2023-01-01,Sunday,10:00,good,walking | reading,Morning,Great start\n" +
        "2023-01-02,2023-01-02,Monday,12:00,rad,running | coding,Work,Productive day\n" +
        "2023-01-03,2023-01-03,Tuesday,14:00,meh,reading,Afternoon,Average day\n";

    [Fact]
    public async Task LoadAsync_WithTextReader_SuccessfullyLoadsData()
    {
        using StringReader reader = new(SAMPLE_CSV);
        DaylioData daylioData = await DaylioData.LoadAsync(reader);

        Assert.NotNull(daylioData.DataRepo);
        Assert.NotNull(daylioData.DataSummary);
        Assert.Equal(3, daylioData.DataSummary.TotalEntries);
        Assert.Equal(3, daylioData.DataSummary.TotalDays);
        Assert.True(daylioData.DataRepo.Activities.Contains("walking"));
        Assert.True(daylioData.DataRepo.Activities.Contains("reading"));
        Assert.True(daylioData.DataRepo.Activities.Contains("running"));
        Assert.True(daylioData.DataRepo.Activities.Contains("coding"));
    }

    [Fact]
    public async Task LoadAsync_WithStream_SuccessfullyLoadsData()
    {
        byte[] bytes = Encoding.UTF8.GetBytes(SAMPLE_CSV);
        using MemoryStream stream = new(bytes);
        DaylioData daylioData = await DaylioData.LoadAsync(stream);

        Assert.NotNull(daylioData.DataRepo);
        Assert.NotNull(daylioData.DataRepo.CSVData);
        Assert.Equal(3, daylioData.DataRepo.CSVData.Count);
    }

    [Fact]
    public async Task LoadAsync_WithFilePath_SuccessfullyLoadsData()
    {
        string tempFilePath = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFilePath, SAMPLE_CSV);
            DaylioData daylioData = await DaylioData.LoadAsync(tempFilePath);

            Assert.NotNull(daylioData.DataRepo);
            Assert.Equal(3, daylioData.DataSummary.TotalEntries);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Fact]
    public void Constructor_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() => new DaylioData("non_existent_daylio_file_12345.csv"));
    }

    [Fact]
    public void Constructor_WithNullOrWhitespaceFilePath_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new DaylioData("   "));
    }

    [Fact]
    public void TryLoad_WithNonExistentFile_ReturnsFalse()
    {
        bool success = DaylioData.TryLoad("non_existent_daylio_file_12345.csv", out DaylioData? daylioData);

        Assert.False(success);
        Assert.Null(daylioData);
    }

    [Fact]
    public void TryLoad_WithValidFile_ReturnsTrueAndPopulatesData()
    {
        string tempFilePath = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFilePath, SAMPLE_CSV);
            bool success = DaylioData.TryLoad(tempFilePath, out DaylioData? daylioData);

            Assert.True(success);
            Assert.NotNull(daylioData);
            Assert.Equal(3, daylioData.DataSummary.TotalEntries);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Fact]
    public async Task TryLoadAsync_WithNonExistentFile_ReturnsFalse()
    {
        (bool success, DaylioData? daylioData) = await DaylioData.TryLoadAsync("non_existent_daylio_file_12345.csv");

        Assert.False(success);
        Assert.Null(daylioData);
    }

    [Fact]
    public async Task TryLoadAsync_WithValidFile_ReturnsTrueAndPopulatesData()
    {
        string tempFilePath = Path.GetTempFileName();
        try
        {
            await File.WriteAllTextAsync(tempFilePath, SAMPLE_CSV);
            (bool success, DaylioData? daylioData) = await DaylioData.TryLoadAsync(tempFilePath);

            Assert.True(success);
            Assert.NotNull(daylioData);
            Assert.Equal(3, daylioData.DataSummary.TotalEntries);
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Fact]
    public async Task LoadAsync_WithEmptyStream_ReturnsEmptyDataset()
    {
        using MemoryStream emptyStream = new(Array.Empty<byte>());
        DaylioData daylioData = await DaylioData.LoadAsync(emptyStream);

        Assert.NotNull(daylioData.DataRepo);
        Assert.Empty(daylioData.DataRepo.CSVData);
        Assert.Equal(0, daylioData.DataSummary.TotalEntries);
    }

    [Fact]
    public async Task LoadAsync_WithCancelledToken_ThrowsOperationCanceledException()
    {
        using CancellationTokenSource cts = new();
        cts.Cancel();

        using StringReader reader = new(SAMPLE_CSV);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await DaylioData.LoadAsync(reader, cts.Token);
        });
    }

    [Fact]
    public async Task UpdateFileAsync_UpdatesRepositoryDataAndRefreshesSummary()
    {
        string tempFilePath1 = Path.GetTempFileName();
        string tempFilePath2 = Path.GetTempFileName();

        try
        {
            await File.WriteAllTextAsync(tempFilePath1, SAMPLE_CSV);
            string secondCsv =
                "full_date,date,weekday,time,mood,activities,note_title,note\n" +
                "2023-02-01,2023-02-01,Wednesday,09:00,rad,yoga,Morning,Fresh\n";
            await File.WriteAllTextAsync(tempFilePath2, secondCsv);

            DaylioData daylioData = await DaylioData.LoadAsync(tempFilePath1);
            Assert.Equal(3, daylioData.DataSummary.TotalEntries);

            await daylioData.DataRepo.UpdateFileAsync(tempFilePath2);
            Assert.Single(daylioData.DataRepo.CSVData);
            Assert.Equal(1, daylioData.DataSummary.TotalEntries);
            Assert.True(daylioData.DataRepo.Activities.Contains("yoga"));
            Assert.False(daylioData.DataRepo.Activities.Contains("running"));
        }
        finally
        {
            if (File.Exists(tempFilePath1))
            {
                File.Delete(tempFilePath1);
            }
            if (File.Exists(tempFilePath2))
            {
                File.Delete(tempFilePath2);
            }
        }
    }
}
