using MediBook.Domain.Common;

namespace MediBook.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Token { get; set; } = default!;
        public DateTimeOffset ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
