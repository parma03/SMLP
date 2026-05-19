using Microsoft.AspNetCore.Components;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Commands.CreateDocument;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.Selections.BeritaAcara.Commands.Update;
using Psmp.Shared.Selections.BeritaAcara.Queries;
using Syncfusion.Blazor.Inputs;

namespace Psmp.Bsui.Features.Selections.BeritaAcara.Components;

public partial class FinalBeritaAcara
{
    [Parameter]
    public string? Type { get; set; } = string.Empty;

    [Parameter]
    public int Batch { get; set; }
    [Parameter]
    public int Year { get; set; }

    protected long _maximumFileSizeInBytes = long.MaxValue;
    protected ErrorResponse? _error;
    protected List<GetSelectionBeritaAcaraQueryResponse> _selectionBeritaAcara = new();
    public FileUploadResponse FileUploads { get; set; } = new();
    protected SfUploader _sfUploaderFinal = new();
    protected List<Guid> PostingIds { get; set; } = new();
    protected Guid FinalDocumentId { get; set; } = Guid.Empty;
    protected Guid DraftDocumentId { get; set; } = Guid.Empty;
    protected bool _isLoadingDialog = false;
    protected string _title = "";
    protected string? _documentPath;

    protected override async Task OnInitializedAsync()
    {
        _maximumFileSizeInBytes = _documentOptions.Value.MaximumFileSizeInBytes;
        await OnLoadData();
    }

    public async Task OnLoadData()
    {
        var param = new GetSelectionBeritaAcaraFilterByBatchRequest
        {
            SelectionBatch = Batch,
            Year = Year
        };

        // get data all scholarship berita acara on batch
        var getBeritaAcara = await _selectionBeritaAcaraService.GetSelectionBeritaAcaraByBatchAsync(param);
        _error = getBeritaAcara.Error;
        if (getBeritaAcara.Result is not null)
        {
            var data = getBeritaAcara.Result.Items;

            if (data.Count() > 0)
            {
                foreach (var item in data)
                {
                    PostingIds!.Add(item.ScholarshipPostingId);
                }

                var itemBeritaAcara = data.LastOrDefault();
                FinalDocumentId = itemBeritaAcara!.FinalDocumentId ?? Guid.Empty;
                DraftDocumentId = itemBeritaAcara!.DraftDocumentId ?? Guid.Empty;
                _title = itemBeritaAcara.Title;

                if (FinalDocumentId != Guid.Empty)
                {
                    var document = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = FinalDocumentId, DocumentType = DocumentType.BeritaAcara });
                    _error = document.Error;
                    if (document.Error is null)
                    {
                        var extention = Path.GetExtension(document.Result!.FileName);
                        if (extention.ToLower() == ".pdf")
                        {
                            await OnLoadDocument();
                        }
                    }
                }
            }
        }
    }

    protected void ValueChangeUploaderFinalBeritaAcara(UploadChangeEventArgs args)
    {
        FileUploads.FileName = args.Files.First().File.Name;
        FileUploads.FileSize = args.Files.First().File.Size;
        FileUploads.FileContentType = args.Files.First().File.ContentType;
        FileUploads.File = args.Files.First().File;
    }
    protected void OnRemoveUploaderFinalBeritaAcara(RemovingEventArgs args)
    {
        FileUploads = new();
        args.PostRawFile = false;
    }
    private async Task OnClickUploadAsync()
    {
        _isLoadingDialog = true;
        if (FileUploads.File is null)
        {
            _snackbar.AddWarning("Please browse file");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        await UploadFinalBeritaAcara();
        if (FinalDocumentId == Guid.Empty)
        {
            _snackbar.AddWarning("Error upload file");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        var request = new UpdateSelectionBeritaAcaraByBatchRequest
        {
            Type = "Final",
            ScholarshipPostingIds = PostingIds,
            DraftDocumentId = DraftDocumentId,
            FinalDocumentId = FinalDocumentId,
            Title = _title,
        };
        var updateBeritaCara = await _selectionBeritaAcaraService.UpdateSelectionBeritaAcaraByBatchAsync(request);
        if (updateBeritaCara.Error is not null)
        {
            var result = string.Empty;
            for (var i = 0; i < updateBeritaCara.Error.Details.Count; i++)
            {
                result = result + updateBeritaCara.Error.Details[i] + ",";
            }

            _snackbar.AddError("Berita Acara Error : " + result);
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        var requestStatus = new UpdateSelectionBeritaAcaraStatusByBatchRequest
        {
            ScholarshipPostingIds = PostingIds,
            Status = BeritaAcaraStatus.Completed
        };
        var updateStatus = await _selectionBeritaAcaraService.UpdateSelectionBeritaAcaraStatusByBatchAsync(requestStatus);
        if (updateStatus.Error is not null)
        {
            var result = string.Empty;
            for (var i = 0; i < updateStatus.Error.Details.Count; i++)
            {
                result = result + updateStatus.Error.Details[i] + ",";
            }

            _snackbar.AddError("Error : " + result);
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        await OnLoadData();
        _isLoadingDialog = false;
        FileUploads = new();
        await _sfUploaderFinal.RemoveAsync();
        StateHasChanged();
        _snackbar.AddSuccess("Success upload data");
    }

    private async Task<MemoryStream> UploadFinalBeritaAcara()
    {
        var extention = Path.GetExtension(FileUploads.File.Name);
        var timeStamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
        var filename = $"Final_Berita Acara_{timeStamp}{extention}";
        var memoryStream = new MemoryStream();
        await FileUploads.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset the stream position

        var formFile = new FormFile(memoryStream, 0, FileUploads.File.Size, filename, filename)
        {
            Headers = new HeaderDictionary(),
            ContentType = FileUploads.File.ContentType
        };

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.BeritaAcara,
            File = formFile,
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is null)
        {
            FinalDocumentId = resultUpload.Result!.DocumentId;
        }

        return memoryStream;
    }

    public async Task OnLoadDocument()
    {
        if (FinalDocumentId != Guid.Empty)
        {
            // load document
            var doc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = FinalDocumentId, DocumentType = DocumentType.BeritaAcara });
            if (doc.Error is null)
            {
                var dataDoc = doc.Result;
                _documentPath = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
            }
        }
    }
}
