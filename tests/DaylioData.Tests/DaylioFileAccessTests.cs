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
        IEnumerable<DaylioCSVDataModel>? records = DaylioFileAccess.TryRead(reader);

        Assert.NotNull(records);
        List<DaylioCSVDataModel> list = records.ToList();
        Assert.Single(list);
        Assert.Equal("good", list[0].Mood);
        Assert.Equal("Sprint Planning", list[0].NoteTitle);
        Assert.Equal(new DateOnly(2023, 3, 1), list[0].FullDate);
        Assert.Equal(new TimeOnly(9, 15), list[0].Time);
        Assert.Equal(new DateTime(2023, 3, 1, 9, 15, 0), list[0].Timestamp);
    }

    [Fact]
    public void TryRead_WithMemoryStream_ReturnsRecords()
    {
        byte[] bytes = Encoding.UTF8.GetBytes(VALID_CSV);
        using MemoryStream stream = new(bytes);
        using StreamReader streamReader = new(stream);

        IEnumerable<DaylioCSVDataModel>? records = DaylioFileAccess.TryRead(streamReader);

        Assert.NotNull(records);
        Assert.Single(records);
    }

    [Fact]
    public void TryRead_WithCorruptedHeader_ReturnsNullWithoutThrowing()
    {
        string corruptCsv = "some_random_column,another_column\n1,2\n";
        using StringReader reader = new(corruptCsv);

        IEnumerable<DaylioCSVDataModel>? records = DaylioFileAccess.TryRead(reader);

        Assert.Null(records);
    }

    [Fact]
    public void TryReadFile_WithNonExistentPath_ReturnsNullWithoutThrowing()
    {
        DaylioFileAccess fileAccess = new("non_existent_file_path_12345.csv");
        IEnumerable<DaylioCSVDataModel>? records = fileAccess.TryReadFile();

        Assert.Null(records);
    }
}
