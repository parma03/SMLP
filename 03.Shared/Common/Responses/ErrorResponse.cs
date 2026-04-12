using System.Net;

namespace Smlp.Shared.Common.Responses;

public abstract class ErrorResponse
{
    public Exception Exception { get; set; } = default!;
    public string Type { get; set; } = default!;
    public string Title { get; set; } = default!;
    public HttpStatusCode Status { get; set; }
    public string Detail { get; set; } = default!;
    public virtual IList<string> Details { get; set; } = new List<string>();
}
