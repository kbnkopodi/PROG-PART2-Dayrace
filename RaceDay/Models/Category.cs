namespace RaceDay.Models
{
    public class Category
    {
        public string CategoryId { get; set; } = Guid.NewGuid().ToString();
        public string EventId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public double Distance { get; set; }
        public decimal EntryFee { get; set; }

        public Event? Event { get; set; }
        public ICollection<Entry>? Entries { get; set; }
    }
}
