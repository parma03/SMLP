using Microsoft.AspNetCore.Components.Forms;

namespace Smlp.Bsui.Common.Extensions;

public static class IBrowserFileExtensions
{
    public static FormFile ToFormFile(this IBrowserFile browserFile, MemoryStream stream, string parameterName)
    {
        return new FormFile(stream, 0, stream.Length, parameterName, browserFile.Name)
        {
            Headers = new HeaderDictionary(),
            ContentType = browserFile.ContentType
        };
    }
}
