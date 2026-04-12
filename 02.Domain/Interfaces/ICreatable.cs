namespace SMLP.Domain.Interfaces
{
    public interface ICreatable
    {
        public DateTimeOffset Created { get; set; }
        public string CreatedBy { get; set; }
    }

}
