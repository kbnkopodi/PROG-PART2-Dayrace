namespace RaceDay.Models
{
    public class RaceRoute
    {
        public string RouteId { get; set; } = Guid.NewGuid().ToString();
        public string EventId { get; set; } = string.Empty;
        public string RouteName { get; set; } = string.Empty;
        public string StartPoint { get; set; } = string.Empty;
        public string EndPoint { get; set; } = string.Empty;

        public Event? Event { get; set; }

    }
}
