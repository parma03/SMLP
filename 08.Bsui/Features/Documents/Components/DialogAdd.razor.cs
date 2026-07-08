/*
*/

using FluentValidation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using Smlp.Base.Selections.Enums;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Documents.Commands.CreateDocument;

namespace Smlp.Bsui.Features.Documents.Components;

public partial class DialogAdd
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    private bool _isLoading;
    private ErrorResponse? _error;
    private readonly CreateDocumentModel _model = new();
    private long _maximumFileSizeInBytes;

    protected override void OnInitialized()
    {
        _maximumFileSizeInBytes = _documentOptions.Value.MaximumFileSizeInBytes;
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private void OnDocumentFileSelected(InputFileChangeEventArgs eventArgs)
    {
        _model.File = eventArgs.File;
    }

    private async Task OnValidSubmit()
    {
        _error = null;

        if (_model.File is null)
        {
            _snackbar.AddError($"Please select a {CommonDisplayTextFor.File.ToLower()}.");

            return;
        }

        if (_model.File.Size > _maximumFileSizeInBytes)
        {
            _snackbar.AddError($"Selected file with size {_model.File.Size.ToReadableFileSize()} exceeds the maximum of {_maximumFileSizeInBytes.ToReadableFileSize()}");

            return;
        }

        _isLoading = true;

        await MudDialog.SetOptionsAsync(MudDialog.Options with { BackdropClick = false });

        await using var memoryStream = new MemoryStream();
        await _model.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);

        var request = new CreateDocumentRequest
        {
            DocumentType = _model.DocumentType,
            File = _model.File.ToFormFile(memoryStream, nameof(CreateDocumentRequest.File))
        };

        var response = await _documentService.CreateDocumentAsync(request);

        _isLoading = false;

        await MudDialog.SetOptionsAsync(MudDialog.Options with { BackdropClick = true });

        if (response.Error is not null)
        {
            _error = response.Error;

            return;
        }

        MudDialog.Close(DialogResult.Ok(true));
    }

    private void OnInvalidSubmit(EditContext editContext)
    {
        _snackbar.AddErrors(editContext.GetValidationMessages());
    }
}

public class CreateDocumentModel
{
    public DocumentType DocumentType { get; set; }
    public IBrowserFile? File { get; set; }
}

public class CreateDocumentModelValidator : AbstractValidator<CreateDocumentModel>
{
    public CreateDocumentModelValidator()
    {
        RuleFor(v => v.DocumentType)
            .NotEmpty();

        RuleFor(v => v.File)
            .NotNull();
    }
}
