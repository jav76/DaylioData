using System.Text;
using DaylioData.Models;
using DaylioData.Repo;

namespace DaylioData.Tests;

public class DaylioFileAccessTests
{
    private const string VALID_CSV =
        "full_date,date,weekday,time,mood,activities,note_title,note\n" +
        "2023-03-01,2023-03-01,Wednesday,09:15,good,work | coffee,Sprint Planning,Discussion notes\n";

    [Fact]
    public void TryRead_WithValidTextReader_ReturnsRecords()
    {
        using StringReader reader = new(VALID_CSV);
        IReadOnlyList<DaylioCSVDataModel>? records = DaylioFileAccess.TryRead(reader);

        Assert.NotNull(records);
        Assert.Single(records);
        Assert.Equal("good", records[0].Mood);
        Assert.Equal("Sprint Planning", records[0].NoteTitle);
        Assert.Equal(new DateOnly(2023, 3, 1), records[0].FullDate);
        Assert.Equal(new TimeOnly(9, 15), records[0].Time);
        Assert.Equal(new DateTime(2023, 3, 1, 9, 15, 0), records[0].Timestamp);
    }

    [Fact]
    public void TryRead_WithMemoryStream_ReturnsRecords()
    {
        byte[] bytes = Encoding.UTF8.GetBytes(VALID_CSV);
        using MemoryStream stream = new(bytes);
        using StreamReader streamReader = new(stream);

        IReadOnlyList<DaylioCSVDataModel>? records = DaylioFileAccess.TryRead(streamReader);

        Assert.NotNull(records);
        Assert.Single(records);
    }

    [Fact]
    public void TryRead_WithCorruptedHeader_ReturnsNullWithoutThrowing()
    {
        string corruptCsv = "some_random_column,another_column\n1,2\n";
        using StringReader reader = new(corruptCsv);

        IReadOnlyList<DaylioCSVDataModel>? records = DaylioFileAccess.TryRead(reader);

        Assert.Null(records);
    }

    [Fact]
    public void TryReadFile_WithNonExistentPath_ReturnsNullWithoutThrowing()
    {
        DaylioFileAccess fileAccess = new("non_existent_file_path_12345.csv");
        IReadOnlyList<DaylioCSVDataModel>? records = fileAccess.TryReadFile();

        Assert.Null(records);
    }

    [Fact]
    public void ReadFile_WithNonExistentPath_ThrowsFileNotFoundException()
    {
        DaylioFileAccess fileAccess = new("non_existent_file_path_12345.csv");
        Assert.Throws<FileNotFoundException>(() => fileAccess.ReadFile());
    }

    [Fact]
    public void ReadFile_WithCorruptedHeader_ThrowsInvalidDataExceptionWithInnerException()
    {
        string corruptCsv = "some_random_column,another_column\n1,2\n";
        using StringReader reader = new(corruptCsv);
        DaylioFileAccess fileAccess = new(reader);

        InvalidDataException ex = Assert.Throws<InvalidDataException>(() => fileAccess.ReadFile());
        Assert.NotNull(ex.InnerException);
    }

    [Fact]
    public async Task ReadFileAsync_WithCorruptedHeader_ThrowsInvalidDataExceptionWithInnerException()
    {
        string corruptCsv = "some_random_column,another_column\n1,2\n";
        using StringReader reader = new(corruptCsv);
        DaylioFileAccess fileAccess = new(reader);

        InvalidDataException ex = await Assert.ThrowsAsync<InvalidDataException>(() => fileAccess.ReadFileAsync());
        Assert.NotNull(ex.InnerException);
    }
}
