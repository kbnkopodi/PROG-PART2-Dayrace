namespace RaceDay.Models
{
    public class CategoryDto
    {
        public string CategoryName { get; set; } = string.Empty; // e.g. Under 20, Senior, 10km, 21km
        public double Distance { get; set; }
        public decimal EntryFee { get; set; }

    }
}
