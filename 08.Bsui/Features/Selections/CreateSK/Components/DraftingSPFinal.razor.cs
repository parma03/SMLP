using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Psmp.Bsui.Common.Extensions;
using Syncfusion.Blazor.Inputs;
using Psmp.Base.Selections.Enums;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Syncfusion.Blazor.DocumentEditor;
using Psmp.Shared.Documents.Commands.CreateDocument;
using Psmp.Shared.Selections.CreateSK.Commands.Create;
using Psmp.Shared.Selections.CreateSK.Queries;
using Psmp.Shared.Selections.CreateSK.Commands.Update;

namespace Psmp.Bsui.Features.Selections.CreateSK.Components;

public partial class DraftingSPFinal
{
    [Parameter]
    public Guid Id { get; set; }

    [Parameter]
    public int Batch { get; set; }

    [Parameter]
    public int Year { get; set; }

    [Parameter]
    public string Type { get; set; } = string.Empty;

    protected long _maximumFileSizeInBytes = long.MaxValue;
    protected SfUploader? _uploadFinalBeritaAcara;
    public SfDocumentEditorContainer _containerFinal = new();
    protected List<Guid> PostingIds { get; set; } = new();
    protected Guid DecreeFinalDocumentId { get; set; } = Guid.Empty;
    protected Guid WarrantFinalDocumentId { get; set; } = Guid.Empty;
    public FileUploadResponse FileUploads { get; set; } = new();
    protected bool _isLoadingDialog = false;
    protected string? _documentPath;
    protected bool _isPDF = false;
    protected DocumentEditorSettingsModel _settingDocument = new() { ShowRuler = true };

    protected override async Task OnInitializedAsync()
    {
        _maximumFileSizeInBytes = _documentOptions.Value.MaximumFileSizeInBytes;

        var param = new GetSelectionDecreeByBatchFilterRequest
        {
            SelectionBatch = Convert.ToInt32(Batch),
            Year = Year
        };

        var getPosting = await _selectionDecreesService.GetSelectionDecreeDocumentByBatchAsync(param);
        if (getPosting.Result is not null)
        {
            if (getPosting.Result.Items.Count > 0)
            {
                var itemPosting = getPosting.Result.Items;
                var lastPosting = itemPosting.LastOrDefault();
                if (lastPosting != null)
                {
                    WarrantFinalDocumentId = lastPosting.WarrantDocumentId ?? Guid.Empty;
                }
            }
        }

        if (WarrantFinalDocumentId != Guid.Empty)
        {
            var document = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = WarrantFinalDocumentId, DocumentType = DocumentType.ScholarshipWarrant });
            if (document.Error is null)
            {
                var extention = Path.GetExtension(document.Result!.FileName);
                if (extention.ToLower() == ".pdf")
                {
                    _isPDF = true;
                    await OnLoadDocument();
                }
            }
        }

        // auto get from applicant because can generate again
        var getApplicant = await _selectionDecreesService.GetSelectionDecreeByBatchAsync(param);
        if (getApplicant.Result is not null && getApplicant.Result.Items.Count() > 0)
        {
            var data = getApplicant.Result.Items;
            foreach (var item in data)
            {
                PostingIds.Add(item.PostingId);
            }

            PostingIds = PostingIds.Distinct().ToList();
        }
    }
    public async Task OnLoadDocument()
    {
        if (WarrantFinalDocumentId != Guid.Empty)
        {
            // load document
            var doc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = WarrantFinalDocumentId, DocumentType = DocumentType.ScholarshipWarrant });
            if (doc.Error is null)
            {
                var dataDoc = doc.Result;
                _documentPath = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
            }
        }
    }

    protected void ValueChangeUploaderFinalSuartPerintah(UploadChangeEventArgs args)
    {
        FileUploads.FileName = args.Files.First().File.Name;
        FileUploads.FileSize = args.Files.First().File.Size;
        FileUploads.FileContentType = args.Files.First().File.ContentType;
        FileUploads.File = args.Files.First().File;
    }
    protected void OnRemoveUploaderFinalSuratPerintah(RemovingEventArgs args)
    {
        FileUploads = new();
        args.PostRawFile = false;
    }
    private async Task OnClickUploadAsync(MouseEventArgs e)
    {
        var x = e;
        _isLoadingDialog = true;
        if (FileUploads.File is null)
        {
            _snackbar.AddWarning("Please browse file");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        await UploadFinalSuratPerintah();
        if (WarrantFinalDocumentId == Guid.Empty)
        {
            _snackbar.AddWarning("Error upload file");
            _isLoadingDialog = false;
            StateHasChanged();
            return;
        }

        if (PostingIds.Count() > 0)
        {
            foreach (var item in PostingIds)
            {
                var request = new CreateSelectionDecreeFinalRequest
                {
                    Type = "Warrant",
                    ScholarshipPostingId = item,
                    WarrantDocumentId = WarrantFinalDocumentId,
                };

                var createDecreeFinal = await _selectionDecreesService.CreateSelectionDecreeFinalAsync(request);
                if (createDecreeFinal.Error is not null)
                {
                    _snackbar.AddError("Create Decree Error : " + createDecreeFinal.Error.ToDetail());
                    _isLoadingDialog = false;
                    StateHasChanged();
                    return;
                }
            }

            var filterRequest = new GetSelectionDecreeByBatchFilterRequest
            {
                SelectionBatch = Convert.ToInt32(Batch),
                Year = Year
            };

            var getApplicant = await _selectionDecreesService.GetSelectionDecreeByBatchAsync(filterRequest);
            if (getApplicant.Result!.Items.Count() > 0)
            {
                var applicantPass = getApplicant.Result.Items.Where(x => x.Status == ApplicantStatus.Passed).ToList();
                foreach (var item in applicantPass)
                {
                    var requestDraftStatus = new UpdateSelectionWarrantDraftStatusRequest
                    {
                        ApplicantId = item.ApplicantId,
                        Status = DraftWarrantStatus.Final
                    };

                    var updateDraftStatus = await _selectionDecreesService.UpdateSelectionWarrantDraftStatusAsync(requestDraftStatus);
                }
            }
        }

        _isLoadingDialog = false;
        StateHasChanged();
        _snackbar.AddSuccess("Success upload data");
        await Task.Delay(500);
        _navigationManager.NavigateTo(_navigationManager.Uri, forceLoad: true);
    }
    private async Task<MemoryStream> UploadFinalSuratPerintah()
    {
        var extention = Path.GetExtension(FileUploads.File.Name);
        var timeStamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
        var filename = $"Final Surat Perintah_{timeStamp}.{extention}";
        var memoryStream = new MemoryStream();
        await FileUploads.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset the stream position

        var formFile = new FormFile(memoryStream, 0, FileUploads.File.Size, filename, filename)
        {
            Headers = new HeaderDictionary(),
            ContentType = FileUploads.File.ContentType
        };

        //var file = IBrowserFileExtensions.ToFormFile(FileUploads.File, memoryStream, filename);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.ScholarshipWarrant,
            File = formFile,
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is null)
        {
            WarrantFinalDocumentId = resultUpload.Result!.DocumentId;
        }

        return memoryStream;
    }
    //private async Task OnClickDownloadAsync(MouseEventArgs e)
    //{
    //    var x = e;
    //    _snackbar.AddWarning("On Progress Development");
    //    await Task.CompletedTask;
    //}

    public void OnExport(object args)
    {
        var x = args;
        var timeStamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
        var filename = $"Final Surat Perjanjian_{timeStamp}.docx";
        var documentEditor = _containerFinal.DocumentEditor;
        documentEditor.SaveAsync(filename, FormatType.Docx);
    }
    public void Print(object args)
    {
        var x = args;
        var documentEditor = _containerFinal.DocumentEditor;
        documentEditor.PrintAsync();
    }

    public async void OnCreated(object args)
    {
        try
        {
            var x = args;
            var request = new GetDocumentFileRequest
            {
                DocumentId = WarrantFinalDocumentId,
                DocumentType = DocumentType.ScholarshipWarrant
            };

            var getDraftDocument = await _documentService.GetDocumentFileByDocumentTypeAsync(request);
            if (getDraftDocument.Error is not null)
            {
                var editor = _containerFinal.DocumentEditor;
                await editor.OpenBlankAsync();
                StateHasChanged();
            }
            else
            {
                if (!_isPDF)
                {
                    Stream stream = new MemoryStream(getDraftDocument.Result!.Content);
                    var editor = _containerFinal.DocumentEditor;
                    await editor.OpenAsync(stream, ImportFormatType.Docx);
                    StateHasChanged();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
