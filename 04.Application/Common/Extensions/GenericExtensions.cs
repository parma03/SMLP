using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Smlp.Application.Common.Extensions;

public static class GenericExtensions
{
    public static string ToPrettyJson<TRequest>(this TRequest request)
    {
        return JToken.Parse(JsonConvert.SerializeObject(request)).ToString(Formatting.Indented);
    }
}
