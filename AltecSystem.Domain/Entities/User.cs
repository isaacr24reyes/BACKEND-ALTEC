namespace AltecSystem.Domain.Entities
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public int? Altec_Points { get; set; }

    }
}