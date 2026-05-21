namespace Smlp.Domain.Interfaces
{
    public interface IDeletable
    {
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}
