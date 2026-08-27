using AMYL.Api.Shared.Domain;

namespace AMYL.Api.Shared.Domain.Entities
{
    public class Users : BaseEntity
    {
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public Roles Role { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
        public DateTime DateCreated { get; set; }
        public ICollection<Memory> Memories { get; set; } = new List<Memory>();
        public Users() { }

        public static Users Create(
        string userName,
        string FullName,
        string password,
        string email,
        DateTime createdAt)
        {
            return new Users
            {
                UserName = userName,
                FullName = FullName,
                PasswordHash = password,
                Email = email,
                Role = Roles.Owner,
                DateCreated = createdAt
            };
        }

    }

    public enum Roles
    {
        Owner,
        Viwer
    }

}
