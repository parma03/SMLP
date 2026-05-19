using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components;
using Psmp.Bsui.Common.Extensions;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.DocumentEditor;
using Psmp.Base.Selections.Enums;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Commands.CreateDocument;
using Psmp.Shared.Selections.CreateSK.Commands.Create;
using Psmp.Shared.Selections.CreateSK.Commands.Update;
using Psmp.Shared.Selections.CreateSK.Queries;
using Psmp.Base.Scholarships.Enums;
using Psmp.Shared.Selections.SelectionTalentReviews.Commands.Update;
using Psmp.Shared.Selections.SelectionOverviews.Commands.Update;
using Psmp.Shared.Selections.BeritaAcara.Commands.Update;

namespace Psmp.Bsui.Features.Selections.CreateSK.Components;

public partial class DraftingSKFinal
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
    protected Guid _scholarshipPostingId;
    protected string _companyCode = "0000";
    protected string _companyCodeApplicantFilter = "0000";
    protected int _companyGroupId = 0;
    protected int _companyGroupIdApplicantFilter = 0;
    protected int _directorateId = 0;
    protected int _divisionId = 0;
    protected Guid _codeId;
    protected int _batch = 0;
    protected int _year = 0;
    protected ScholarshipLocation _category = 0;
    protected int _selectionType = 1;

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
                    DecreeFinalDocumentId = lastPosting.DecreeDocumentId ?? Guid.Empty;
                }
            }
        }

        if (DecreeFinalDocumentId != Guid.Empty)
        {
            var document = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = DecreeFinalDocumentId, DocumentType = DocumentType.ScholarshipDecree });
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
        if (DecreeFinalDocumentId != Guid.Empty)
        {
            // load document
            var doc = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = DecreeFinalDocumentId, DocumentType = DocumentType.ScholarshipDecree });
            if (doc.Error is null)
            {
                var dataDoc = doc.Result;
                _documentPath = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
            }
        }
    }

    protected void ValueChangeUploaderFinalSuratKeputusan(UploadChangeEventArgs args)
    {
        FileUploads.FileName = args.Files.First().File.Name;
        FileUploads.FileSize = args.Files.First().File.Size;
        FileUploads.FileContentType = args.Files.First().File.ContentType;
        FileUploads.File = args.Files.First().File;
    }
    protected void OnRemoveUploaderFinalSuratKeputusan(RemovingEventArgs args)
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

        await UploadFinalSuratKeputusan();
        if (DecreeFinalDocumentId == Guid.Empty)
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
                    Type = "Decree",
                    ScholarshipPostingId = item,
                    DecreeDocumentId = DecreeFinalDocumentId
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
                    var requestStatus = new UpdateSelectionDecreeStatusRequest
                    {
                        ApplicantId = item.ApplicantId,
                        Status = ApplicantStatus.SKCreated
                    };

                    var updateStatus = await _selectionDecreesService.UpdateSelectionDecreeStatusAsync(requestStatus);
                    if (updateStatus.Error is not null)
                    {
                        _snackbar.AddError("Error : " + updateStatus.Error.ToDetail());
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    var requestDraftStatus = new UpdateSelectionDecreeDraftStatusRequest
                    {
                        ApplicantId = item.ApplicantId,
                        Status = DraftDecreeStatus.Final
                    };

                    var updateDraftStatus = await _selectionDecreesService.UpdateSelectionDecreeDraftStatusAsync(requestDraftStatus);
                    if (updateDraftStatus.Error is not null)
                    {
                        _snackbar.AddError("Draft : " + updateDraftStatus.Error.ToDetail());
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    //Update talent review
                    var requestTalentReview = new UpdateSelectionTalentReviewRequest
                    {
                        Id = item.TalentReviewId,
                        ApplicantId = item.ApplicantId,
                        Status = ApplicantStatus.SKCreated,
                    };
                    var uodateTalentReview = await _selectionTalentReviewService.UpdateSelectionTalentReviewAsync(requestTalentReview);
                    if (uodateTalentReview.Error is not null)
                    {
                        _snackbar.AddError("Talent Review - Error : " + uodateTalentReview.Error.ToDetail());
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    //Update selection overview
                    var updateOverviewRequest = new UpdateSelectionOverviewByApplicantIdRequest
                    {
                        ApplicantId = item.ApplicantId,
                        Stage = SelectionStage.Decree,
                        Status = ApplicantStatus.SKCreated,
                    };

                    var updateOverview = await _selectionOverviewService.UpdateSelectionOverviewByApplicantIdAsync(updateOverviewRequest);
                    if (updateOverview.Error is not null)
                    {
                        _snackbar.AddError("Overview - Error : " + updateOverview.Error.ToDetail());
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }

                    var requestStatusBA = new UpdateSelectionBeritaAcaraApplicantRequest
                    {
                        ScholarshipPostingApplicantId = item.ApplicantId,
                        Status = ApplicantStatus.SKCreated
                    };
                    var sttausBA = await _selectionBeritaAcaraService.UpdateSelectionApplicantBeritAcaraApplicantAsync(requestStatusBA);
                    if (sttausBA.Error is not null)
                    {
                        _snackbar.AddError("Berita Acara Error : " + sttausBA.Error.ToDetail());
                        _isLoadingDialog = false;
                        StateHasChanged();
                        return;
                    }
                }
            }
        }

        _isLoadingDialog = false;
        StateHasChanged();
        _snackbar.AddSuccess("Success upload data");
        await Task.Delay(500);
        _navigationManager.NavigateTo(_navigationManager.Uri, forceLoad: true);
    }
    private async Task<MemoryStream> UploadFinalSuratKeputusan()
    {
        var extention = Path.GetExtension(FileUploads.File.Name);
        var timeStamp = DateTime.Now.ToString("ddMMyyyyHHmmss");
        var filename = $"Final Surat Keputusan_{timeStamp}.{extention}";
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
            DocumentType = DocumentType.ScholarshipDecree,
            File = formFile,
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is null)
        {
            DecreeFinalDocumentId = resultUpload.Result!.DocumentId;
        }

        return memoryStream;
    }
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
                DocumentId = DecreeFinalDocumentId,
                DocumentType = DocumentType.ScholarshipDecree
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
