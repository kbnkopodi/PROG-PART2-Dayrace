namespace RaceDay.Models
{
    public class Result
    {
        public string ResultId { get; set; } = Guid.NewGuid().ToString();
        public string EntryId { get; set; } = string.Empty;
        public string FinishTime { get; set; } = string.Empty;
        public int OverallPosition { get; set; }
        public string Status { get; set; } = "Finished"; // Finished / DNF

        public Entry? Entry { get; set; }

    }
}
