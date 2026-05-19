//namespace Psmp.Bsui.Features.Selections.Overview;
using Microsoft.AspNetCore.Components.Web;
using Psmp.Shared.Common.Responses;
using Syncfusion.Blazor.Inputs;

namespace Psmp.Bsui.Features.UnivRegistration.UniversityRecommendation;

public partial class Index
{
    public List<DropdownItemResponse> _dropdownCategories = new();
    public List<DropdownItemResponse> _dropdownSubyek = new();
    public List<DropdownItemResponse> _dropdownYear = new();
    protected override async Task OnInitializedAsync()
    {
        var data = new List<DropdownItemResponse>()
        {
            new() {Id = 1, Description = "All Categories"},
            new() {Id = 2, Description = "World Rank University" },
            new() {Id = 3, Description = "Local Rank University" }
        };

        _dropdownCategories = data.ToList();

        data = new List<DropdownItemResponse>()
        {
            new() {Id = 1, Description = "Engineering & Technology"},
            new() {Id = 2, Description = "Data Analysis" },
            new() {Id = 3, Description = "Medic" }
        };

        _dropdownSubyek = data.ToList();

        data = new List<DropdownItemResponse>()
        {
            new() {Id = 1, Description = "2024"},
            new() {Id = 2, Description = "2023" },
            new() {Id = 3, Description = "2022" }
        };

        _dropdownYear = data.ToList();
    }

    public List<DataStudyPreparation> _gridData = new()
    {
         new DataStudyPreparation(){Index = 1, University = "Universitas Indonesi", Country = "Indonesia",OfficialWebsite = "Link", RegistrationLink="Link Reg", Deadline = DateTime.Now, Aksi="" },
         new DataStudyPreparation(){Index = 2, University = "University of Oxford", Country = "United Kingdom",OfficialWebsite = "Link", RegistrationLink="Link Reg", Deadline = DateTime.Now, Aksi="" },
    };

    protected long _maximumFileSizeInBytes = long.MaxValue;
    protected SfUploader? _uploadObj;
    private void OnClickAddAsync()
    {
    }
    private async Task ValueChangeUploaderApplicants(UploadChangeEventArgs args)
    {
    }
    protected void GenerateListFromTable()
    {
    }
    protected void OnRemoveUploaderApplicants(RemovingEventArgs args)
    {
    }
    private async Task OnClickUploadAsync(MouseEventArgs e)
    {
    }
    private async Task OnClickDownloadAsync(MouseEventArgs e)
    {
    }

}

public class DataStudyPreparation
{
    public int? Index { get; set; }
    public string? University { get; set; }
    public string? Country { get; set; }
    public string? OfficialWebsite { get; set; }
    public string? RegistrationLink { get; set; }
    public DateTime? Deadline { get; set; }
    public string Aksi { get; set; }
}
