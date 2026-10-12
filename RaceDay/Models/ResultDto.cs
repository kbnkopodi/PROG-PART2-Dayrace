namespace RaceDay.Models
{
    public class ResultDto
    {
        public string EntryId { get; set; } = string.Empty;
        public string FinishTime { get; set; } = string.Empty; // e.g. "01:45:30"
        public int OverallPosition { get; set; }
        public string Status { get; set; } = "Finished"; // Finished / DNF
    }
}
