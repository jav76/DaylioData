using CsvHelper.Configuration.Attributes;

namespace DaylioData.Models
{
    /// <summary>
    /// <see cref="DaylioCSVDataModel"/> is a model for deserialized Daylio CSV data.
    /// </summary>
    public class DaylioCSVDataModel
    {
        [Index(0)]
        public required DateOnly FullDate { get; set; }

        [Index(1)]
        public required DateOnly Date { get; set; }

        [Index(2)]
        public required string? Weekday { get; set; }

        [Index(3)]
        public required TimeOnly Time { get; set; }

        [Index(4)]
        public required string Mood { get; set; }

        [Index(5)]
        public string? Activities { get; set; }

        [Index(6)]
        public string? NoteTitle { get; set; }

        [Index(7)]
        public string? Note { get; set; }

        public override string ToString() => $"{FullDate.ToShortDateString()},{Date.DayNumber.ToString() + "-" + Date.Month.ToString()}," +
            $"{Weekday},{Time},{Mood},{Activities},{NoteTitle},{Note}"; 

        public IEnumerable<string> ActivitiesCollection => Activities?.Split(" | ")
            ?? Array.Empty<string>();
            
    }
}
