using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Scholarships.ScholarshipPlan.Commands.Update;
using Smlp.Shared.Scholarships.ScholarshipPlan.Queries;
using Smlp.Shared.Services.Authorization.Constants;
using Syncfusion.Blazor.Grids;
using System.Security.Claims;
using Smlp.Bsui.Features.Scholarships.ScholarshipPlan.Constants;
using Smlp.Base.Scholarships.Enums;
using Syncfusion.Blazor.Inputs;
using Microsoft.AspNetCore.Components.Web;
using Smlp.Base.Selections.Enums;
using Smlp.Shared.Documents.Commands.CreateDocument;

namespace Smlp.Bsui.Features.Scholarships.ScholarshipPlan.Draft.Components;

public partial class FinalScholarshipPlan
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    protected ErrorResponse? _error;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    protected bool _isUploadFinal = false;
    protected int _companyGroupId = 9999;
    private SfGrid<GetScholarshipPlanResponse> Grid { get; set; } = new();
    protected List<GetScholarshipPlanResponse> _scholarshipPlans = new();
    protected bool _validationDataCheck = false;
    private bool VisiblePopup { get; set; }
    public FileUploadResponse FileUploadFinal { get; set; } = new();
    protected Guid _documentApprovedId = new();
    protected SfUploader _sfUploader = new();
    protected long _maximumFileSizeInBytes = long.MaxValue;

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        _maximumFileSizeInBytes = _documentOptions.Value.MaximumFileSizeInBytes;
        if (_user.Identity!.IsAuthenticated)
        {
            var val = _user.Claims.Where(x => x.Type.ToLower().Trim() == "permission" && x.Value.ToLower().Trim().Contains(Permissions.Psmp_ScholarshipPlan_Crud)).ToList();
            if (val.Count() == 0)
            {
                _navigationManager.NavigateTo(RouteFor.Scholarships, true);
            }

            _isLoading = true;
            await SetDataOnLoadAsync();
            _isLoading = false;
        }
    }

    private async Task SetDataOnLoadAsync()
    {
        _isUploadFinal = false;
        _isLoadingData = true;
        var param = new GetScholarshipPlanRequest
        {
            CompanyCode = "",
            CompanyGroupId = _companyGroupId
        };
        var scholarshipPlan = await _scholarshipPlanService.GetScholarshipPlanAllAsync(param);

        if (scholarshipPlan.Error is null)
        {
            _scholarshipPlans = scholarshipPlan.Result!.Items.Where(x => x.Status is not ScholarshipPlanStatus.Approved and not ScholarshipPlanStatus.Rejected and not ScholarshipPlanStatus.Requested).ToList();
            if (_scholarshipPlans.Count() > 0)
            {
                _isUploadFinal = true;
            }

            StateHasChanged();
        }
        else
        {
            _error = scholarshipPlan.Error;
            _isLoadingData = false;
        }

        _isLoadingData = false;
    }

    #region On Upload Document
    private async Task ValueChangeUploaderFinal(UploadChangeEventArgs args)
    {
        FileUploadFinal.FileName = args.Files.First().File.Name;
        FileUploadFinal.FileSize = args.Files.First().File.Size;
        FileUploadFinal.FileContentType = args.Files.First().File.ContentType;
        FileUploadFinal.File = args.Files.First().File;
        await Task.CompletedTask;
    }
    protected void OnRemoveUploaderFinal(RemovingEventArgs args)
    {
        FileUploadFinal = new();
        args.PostRawFile = false;
    }
    private async Task OnClickUploadAsync(MouseEventArgs e)
    {
        if (FileUploadFinal.FileName is null)
        {
            _snackbar.AddWarning("Please browse agreement document!");
            StateHasChanged();
            return;
        }

        await OnUploadDocument();
        if (_documentApprovedId == Guid.Empty)
        {
            _snackbar.AddWarning("Error upload file");
            StateHasChanged();
            return;
        }

        // update all plan to approve
        foreach (var item in _scholarshipPlans)
        {
            var param = new UpdateScholarshipPlanStatusRequest
            {
                Id = item.Id,
                DocumentApprovedId = _documentApprovedId,
                Status = ScholarshipPlanStatus.Approved
            };

            await _scholarshipPlanService.UpdateScholarshipPlanStatusAsync(param);
        }

        FileUploadFinal = new();
        await _sfUploader.RemoveAsync();
        await SetDataOnLoadAsync();
        StateHasChanged();
        await Task.CompletedTask;
    }

    private async Task OnUploadDocument()
    {
        var memoryStream = new MemoryStream();
        await FileUploadFinal.File.OpenReadStream(_maximumFileSizeInBytes).CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset the stream position

        var file = IBrowserFileExtensions.ToFormFile(FileUploadFinal.File, memoryStream, FileUploadFinal.File.Name);

        var requestFile = new CreateDocumentRequest
        {
            DocumentType = DocumentType.FinalScholarshipPlan,
            File = file
        };

        var resultUpload = await _documentService.CreateDocumentAsync(requestFile);
        if (resultUpload.Error is not null)
        {
            var result = string.Join(", ", resultUpload.Error.Details.ToList());
            _snackbar.AddError("Error : " + result);
            return;
        }

        _documentApprovedId = resultUpload.Result!.DocumentId;
        StateHasChanged();
        await Task.CompletedTask;
    }
    #endregion
}
