namespace Smlp.Shared.Common.Responses;

public class ServiceUnavailableResponse : ErrorResponse
{
    public override IList<string> Details => new List<string> { Detail };
}
