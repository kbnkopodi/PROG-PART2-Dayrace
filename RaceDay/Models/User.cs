using Microsoft.Extensions.Logging;

namespace RaceDay.Models
{
    public class User
    {
        public string UserId { get; set; } = Guid.NewGuid().ToString();
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Organiser" or "Participant"

        public ICollection<Event>? Events { get; set; }
        public ICollection<Entry>? Entries { get; set; }


    }
}
