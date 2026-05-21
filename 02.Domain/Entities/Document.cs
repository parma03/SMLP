using Smlp.Base.Document.Enums;
using Smlp.Domain.Abstracts;
using Smlp.Domain.Events;

namespace Smlp.Domain.Entities
{
    public class Document : FileEntity, IHasDomainEvent
    {
        public DocumentType DocumentType { get; set; }
        public IList<DomainEvent> DomainEvents { get; set; } = new List<DomainEvent>();
    }
}
