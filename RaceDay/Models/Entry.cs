namespace RaceDay.Models
{
    public class Entry
    {
        public string EntryId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty; // Participant
        public string CategoryId { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Registered";

        public User? Participant { get; set; }
        public Category? Category { get; set; }
        public Result? Result { get; set; }

    }
}
