using Smlp.Domain.Abstracts;

namespace Smlp.Domain.Entities.Master
{
    /// <summary>
    /// Stores configurable application settings (key-value pairs) persisted in database.
    /// </summary>
    public class AppSetting : AuditableEntity
    {
        /// <summary>Setting key (unique identifier)</summary>
        public string Key { get; set; } = default!;

        /// <summary>Setting value (stored as string, cast as needed)</summary>
        public string? Value { get; set; }

        /// <summary>Human-readable label for UI display</summary>
        public string? Label { get; set; }

        /// <summary>Setting group/category (e.g. "Shop", "Loyalty", "System")</summary>
        public string? Group { get; set; }

        /// <summary>Description of what this setting controls</summary>
        public string? Description { get; set; }
    }
}
