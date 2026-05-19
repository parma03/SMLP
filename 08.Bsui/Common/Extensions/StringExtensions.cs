using Newtonsoft.Json;
using Psmp.Shared.Common.Constants;

namespace Psmp.Bsui.Common.Extensions;

public static class StringExtensions
{
    public static string PrettifyJson(this string json)
    {
        if (!string.IsNullOrWhiteSpace(json))
        {
            using var stringReader = new StringReader(json);
            using var stringWriter = new StringWriter();
            var jsonReader = new JsonTextReader(stringReader);
            var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
            jsonWriter.WriteToken(jsonReader);
            return stringWriter.ToString();
        }
        else
        {
            return DefaultTextFor.NA;
        }
    }
}
