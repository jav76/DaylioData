using CsvHelper.Configuration.Attributes;

namespace DaylioParser.Models
{
    /// <summary>
    /// <see cref="DaylioCSVDataModel"/> is a model for deserialized Daylio CSV data.
    /// </summary>
    public class DaylioCSVDataModel
    {
        [Index(0)]
        public DateOnly FullDate { get; set; }

        [Index(1)]
        public DateOnly Date { get; set; }

        [Index(2)]
        public string? Weekday { get; set; }

        [Index(3)]
        public TimeOnly Time { get; set; }

        [Index(4)]
        public string? Mood { get; set; }

        [Index(5)]
        public string? Activities { get; set; }

        [Index(6)]
        public string? NoteTitle { get; set; }

        [Index(7)]
        public string? Note { get; set; }

        public override string ToString() => $"{FullDate.ToShortDateString()},{Date.DayNumber.ToString() + "-" + Date.Month.ToString()}," +
            $"{Weekday},{Time},{Mood},{Activities},{NoteTitle},{Note}"; 
            
    }
}
