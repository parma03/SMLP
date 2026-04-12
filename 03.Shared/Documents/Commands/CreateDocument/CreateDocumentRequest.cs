using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Smlp.Base.Selections.Enums;
using Smlp.Shared.Common.Attributes;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Extensions;
using Smlp.Shared.Documents.Constants;
using Smlp.Shared.Documents.Options;

namespace Smlp.Shared.Documents.Commands.CreateDocument;

public class CreateDocumentRequest
{
    [OpenApiContentType(ContentTypes.TextPlain)]
    public DocumentType DocumentType { get; set; } = default!;

    [OpenApiContentType(ContentTypesFor.DocumentFile.Value)]
    public IFormFile File { get; set; } = default!;

    [OpenApiContentType(ContentTypes.TextPlain)]
    public string? EmployeeNumber { get; set; }
}

public class CreateDocumentRequestValidator : AbstractValidator<CreateDocumentRequest>
{
    private readonly long _maximumFileSize;

    public CreateDocumentRequestValidator(IOptions<DocumentOptions> documentOptions)
    {
        _maximumFileSize = documentOptions.Value.MaximumFileSizeInBytes;

        RuleFor(v => v.DocumentType)
            .IsInEnum();

        RuleFor(v => v.File.Length)
            .ExclusiveBetween(0L, _maximumFileSize)
            .WithMessage($"{DisplayTextFor.Document} file size should be greater than 0 KB and less than {_maximumFileSize.ToReadableFileSize()}.");

        RuleFor(v => v.File.ContentType)
            .Must(HaveSupportedContentType)
            .WithMessage($"{DisplayTextFor.Document} file extension is not supported. Please {CommonDisplayTextFor.Upload.ToLower()} supported file.");
    }

    private bool HaveSupportedContentType(string contentType)
    {
        return ContentTypesFor.DocumentFile.List.Contains(contentType);
    }
}
