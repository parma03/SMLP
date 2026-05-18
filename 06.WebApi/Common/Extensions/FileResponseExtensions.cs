using Microsoft.AspNetCore.Mvc;
using Psmp.Shared.Common.Responses;

namespace Psmp.WebApi.Common.Extensions;

public static class FileResponseExtensions
{
    public static FileContentResult AsFile(this FileResponse fileResponse)
    {
        return new FileContentResult(fileResponse.Content, fileResponse.ContentType)
        {
            FileDownloadName = fileResponse.FileName
        };
    }
}
