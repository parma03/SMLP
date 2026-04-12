namespace SMLP.Domain.Events
{
    public abstract class DomainEvent
    {
        protected DomainEvent()
        {
        }

        public bool IsPublished { get; set; }
    }

    public interface IHasDomainEvent
    {
        public IList<DomainEvent> DomainEvents { get; }
    }
}
