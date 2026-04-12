namespace Smlp.Shared.Common.Responses;

public class DropdownItemResponse
{
    public int? Id { get; set; }
    public string Description { get; set; } = default!;
}

public class DropdownItemStringResponse
{
    public string? Id { get; set; }
    public string Description { get; set; } = default!;
}

public class DropdownItemGuidResponse
{
    public Guid? Id { get; set; }
    public string Description { get; set; } = default!;
    public string Code { get; set; } = default!;
}
