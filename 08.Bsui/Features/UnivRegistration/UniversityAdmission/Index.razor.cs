//namespace Smlp.Bsui.Features.Selections.Overview;
using Microsoft.AspNetCore.Components.Web;
using Smlp.Shared.Common.Responses;
using Syncfusion.Blazor.Inputs;

namespace Smlp.Bsui.Features.UnivRegistration.UniversityAdmission;

public partial class Index
{
    public List<DropdownItemResponse> _dropdownCategories = new();
    public List<DropdownItemResponse> _dropdownHps = new();
    public List<DropdownItemResponse> _dropdownCompany = new();
    public List<DropdownItemResponse> _dropdownDirectorate = new();
    public List<DropdownItemResponse> _dropdownScholarshipCode = new();
    public List<DropdownItemResponse> _dropdownBatch = new();
    protected override async Task OnInitializedAsync()
    {
        var data = new List<DropdownItemResponse>()
        {
            new() {Id = 1, Description = "All Categories"},
            new() {Id = 2, Description = "Abroad" },
            new() {Id = 3, Description = "Domestic" }
        };

        _dropdownCategories = data.ToList();

        data = new List<DropdownItemResponse>()
        {
            new() {Id = 1, Description = "PT. Pertamina (Persero)"},
            new() {Id = 2, Description = "PT. Pertamina Putra Niaga" },
            new() {Id = 3, Description = "PT. Pertamina Power Indonesia" }
        };

        _dropdownHps = data.ToList();

        data = new List<DropdownItemResponse>()
        {
            new() {Id = 1, Description = "PT. Pertamina (Persero)"},
            new() {Id = 2, Description = "PT. Pertamina Putra Niaga" },
            new() {Id = 3, Description = "PT. Pertamina Power Indonesia" }
        };

        _dropdownCompany = data.ToList();

        data = new List<DropdownItemResponse>()
        {
            new() {Id = 1, Description = "Directorate 1"},
            new() {Id = 2, Description = "Directorate 2"}
        };

        _dropdownDirectorate = data.ToList();

        data = new List<DropdownItemResponse>()
        {
            new() {Id = 1, Description = "PT001"},
            new() {Id = 2, Description = "PT002"}
        };

        _dropdownScholarshipCode = data.ToList();

        data = new List<DropdownItemResponse>()
        {
            new() {Id = 1, Description = "Batch1"},
            new() {Id = 2, Description = "Batch2"}
        };

        _dropdownBatch = data.ToList();
    }

    private readonly int _count1 = 50;
    private readonly int _count2 = 20;
    private readonly decimal _count3 = 50;
    private readonly string _judul1 = "Total Awardee";
    private readonly string _judul2 = "Complete";
    private readonly string _judul3 = "Submitted";
    private readonly string _judul4 = "Waiting For Approval";
    protected long _maximumFileSizeInBytes = long.MaxValue;
    protected SfUploader? _uploadObj;

    public List<DataStudyPreparation> _gridData = new()
    {
         new DataStudyPreparation(){Index = 1, Name = "John", Company = "Company",EmployeeNumber = "123456", Email="John@gmail.com", University="Uiniversitas Indonesia", Status="Complete", Aksi="" },
         new DataStudyPreparation(){Index = 2, Name = "Budi", Company = "Company",EmployeeNumber = "1234567", Email="Budi@gmail.com", University="Universitas Of Oxford", Status="Approved", Aksi="" },
    };

    private int _leftValue = 20;
    private int _rightValue = 80;

    private void OnLeftValueChanged()
    {
        _leftValue = Math.Min(_leftValue, _rightValue - 1);
    }

    private void OnRightValueChanged()
    {
        _rightValue = Math.Max(_rightValue, _leftValue + 1);
    }
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
    public string? Name { get; set; }
    public string? Company { get; set; }
    public string? EmployeeNumber { get; set; }
    public string? Email { get; set; }
    public string? University { get; set; }
    public string Status { get; set; } = default!;
    public string Aksi { get; set; } = default!;
}
