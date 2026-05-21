using Smlp.Application.Services.Ecm.Models.UploadContent;

namespace Smlp.Application.Services.Ecm;

public interface IEcmService
{
    Task<UploadContentResponse> UploadContentAsync(UploadContentRequest request, CancellationToken cancellationToken);
}
