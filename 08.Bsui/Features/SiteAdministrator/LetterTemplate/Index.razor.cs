using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using System.Security.Claims;
using Psmp.Shared.Common.Responses;
//using Psmp.Shared.Masters.Queries;
using Syncfusion.Blazor.Inputs;
using Microsoft.AspNetCore.Components.Web;

namespace Psmp.Bsui.Features.SiteAdministrator.LetterTemplate;

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

    private List<LetterTemplate> _lettertemplate;

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        await SetDataOnLoadAsync();
    }

    private async Task<List<LetterTemplate>> GetLetterTemplateAsync()
    {
        return new List<LetterTemplate>
        {
            new() { Lettertemplate = "Surat Perintah Direktur SDM", Hshap = "Administration Selection", Receiver = "University Registration" },
            new() { Lettertemplate = "Surat Perjanjian Tugas Belajar", Hshap = "TOEFL", Receiver = "Passed Applicants" },
            new() { Lettertemplate = "Sponsorship Letter", Hshap = "Study Preparation", Receiver = "Passed Applicants" },
            new() { Lettertemplate = "Surat Perintah Mentor", Hshap = "Talent Review Meeting", Receiver = "Mentor" },
            new() { Lettertemplate = "Visa Guarantee", Hshap = "Study Preparation", Receiver = "Univer" },
        };
    }

    private async Task SetDataOnLoadAsync()
    {
        _lettertemplate = await GetLetterTemplateAsync();
    }

    public class LetterTemplate
    {
        public string Lettertemplate { get; set; }
        public string Hshap { get; set; }
        public string Receiver { get; set; }
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
