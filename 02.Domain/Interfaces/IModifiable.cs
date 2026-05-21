namespace Smlp.Domain.Interfaces
{
    public interface IModifiable
    {
        public DateTimeOffset? ModifiedAt { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
