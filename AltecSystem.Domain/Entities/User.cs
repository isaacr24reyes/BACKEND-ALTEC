namespace AltecSystem.Domain.Entities
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public decimal? Altec_Points { get; set; }
        public string? Telefono { get; set; } 


    }
}