using SMLP.Base.User.Enums;
using SMLP.Domain.Abstracts;
using SMLP.Domain.Events;

namespace SMLP.Domain.Entities
{
    public class Document : FileEntity, IHasDomainEvent
    {
        public DocumentType DocumentType { get; set; }
        public IList<DomainEvent> DomainEvents { get; set; } = new List<DomainEvent>();
    }
}
