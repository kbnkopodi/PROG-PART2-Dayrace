namespace RaceDay.Models
{
    public class EventDto
    {
        public string EventName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty; // Run / Walk / Cycle
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public double Distance { get; set; }
    }
}
