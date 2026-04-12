namespace Smlp.Shared.Common.Responses;
public class DropdownEnumResponse<T>
{
    public T Id { get; set; } = default!;
    public string Description { get; set; } = default!;
}
