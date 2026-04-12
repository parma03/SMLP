namespace Smlp.Shared.Common.Responses;

public class PaginatedListResponse<T> : Response
{
    public IList<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
}
