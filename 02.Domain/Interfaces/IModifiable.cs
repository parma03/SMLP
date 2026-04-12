namespace Smlp.Domain.Interfaces
{
    public interface IModifiable
    {
        public DateTimeOffset? Modified { get; set; }
        public string? ModifiedBy { get; set; }
    }
}
