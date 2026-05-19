using Microsoft.AspNetCore.Components;
using Psmp.Shared.Masters.Queries;
using Syncfusion.Blazor.DropDowns;

namespace Psmp.Bsui.Common.Components;

public partial class DropdownCompany
{
    [Parameter]
    public EventCallback<ChangeEventArgs<string, GetCompanyQueryResponse>> OnChangeAsync { get; set; }

    protected string? CompanyValue { get; set; }
    protected List<GetCompanyQueryResponse> _companies = new();

    protected override async Task OnInitializedAsync()
    {
        var companies = await _companyService.GetCompaniesAsync();
        if (companies.Error is null)
        {
            _companies = companies.Result!.Items.ToList();
            _companies.Add(new GetCompanyQueryResponse { Code = "0000", Name = "All Company" });

            _companies = _companies.OrderByDescending(x => x.Code == "0000").ToList();
            CompanyValue = _companies.FirstOrDefault(x => x.Code == "0000")!.Code;
        }
    }
}
