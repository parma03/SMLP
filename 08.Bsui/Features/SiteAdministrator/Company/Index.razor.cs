using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Web;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Masters.Queries;
using Syncfusion.Blazor.Inputs;

namespace Smlp.Bsui.Features.SiteAdministrator.Company;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    protected ErrorResponse? _error;
    protected bool _isLoadingDialog = false;
    protected bool _isLoading = false;
    protected bool _isLoadingData = false;

    protected SfUploader? _uploadObj;
    protected long _maximumFileSizeInBytes = long.MaxValue;
    public FileUploadResponse FileUploadIeltsDoc { get; set; } = new();

    protected List<GetCompanyQueryResponse> _companies = new();

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        await SetDataOnLoadAsync();
    }
    private async Task SetDataOnLoadAsync()
    {
        var response = await _companyService.GetCompaniesAsync();
        if (response.Error is null)
        {
            _companies = response.Result.Items.ToList();
        }

    }
    private async Task OnClickAdd(MouseEventArgs e)
    {
    }

    private async Task ValueChangeUploaderIeltsDoc(UploadChangeEventArgs args)
    {
        FileUploadIeltsDoc.FileName = args.Files.First().File.Name;
        FileUploadIeltsDoc.FileSize = args.Files.First().File.Size;
        FileUploadIeltsDoc.FileContentType = args.Files.First().File.ContentType;
        FileUploadIeltsDoc.File = args.Files.First().File;

        await Task.CompletedTask;
    }

    protected void OnRemoveUploaderIeltsDoc(RemovingEventArgs args)
    {
        FileUploadIeltsDoc = new();
        args.PostRawFile = false;
    }

    private async Task OnClickUploadAsync()
    {
    }

    private async Task OnClickDownloadAsync()
    {
    }
}
