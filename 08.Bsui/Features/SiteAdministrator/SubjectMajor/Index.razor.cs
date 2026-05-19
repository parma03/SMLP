using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using Smlp.Shared.Common.Responses;
//using Smlp.Shared.Masters.Queries;
using Syncfusion.Blazor.Inputs;
using Microsoft.AspNetCore.Components.Web;

namespace Smlp.Bsui.Features.SiteAdministrator.SubjectMajor;

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

    private List<SubjectMajor> _subjectmajor;

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        await SetDataOnLoadAsync();
    }

    private async Task<List<SubjectMajor>> GetSubjectMajorAsync()
    {
        return new List<SubjectMajor>
        {
            new() { Name = "Engineering & Technology", },
            new() { Name = "Arts & Humanities", },
            new() { Name = "Natural Sciences", },
            new() { Name = "Data Science", },
        };
    }

    private async Task SetDataOnLoadAsync()
    {
        _subjectmajor = await GetSubjectMajorAsync();
    }
    public class SubjectMajor
    {
        public string Name { get; set; }
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
