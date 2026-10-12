namespace RaceDay.Models
{
    public class Event
    {
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty; // Organiser
        public string EventName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty; // Run / Walk / Cycle
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
        public double Distance { get; set; }

        public User? Organiser { get; set; }
        public ICollection<Category>? Categories { get; set; }
        public ICollection<RaceRoute>? Routes { get; set; }

    }
}
