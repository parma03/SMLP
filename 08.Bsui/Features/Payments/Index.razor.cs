using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Shared.Common.Enums;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Payments.Contants;
namespace Smlp.Bsui.Features.Payments;

public partial class Index
{
    public class PaymentFilterState
    {
        public int? CompanyGroupId { get; set; }
        public string? CompanyCode { get; set; }
        public int? DirectorateId { get; set; }

        public event Func<PaymentFilterState, PaymentFilterType?, Task>? OnFilterChanged;
        public List<DropdownItemResponse> DropdownHps { get; set; } = new();
        public List<DropdownItemStringResponse> DropdownCompany { get; set; } = new();
        public List<DropdownItemResponse> DropdownDirectorate { get; set; } = new();

        public async Task NotifyChangedAsync(PaymentFilterType? filter)
        {
            if (OnFilterChanged != null)
            {
                await OnFilterChanged.Invoke(this, filter);
            }
        }

        public void ResetCompanyGroup()
        {
            CompanyGroupId = null;
            CompanyCode = string.Empty;
            DirectorateId = null;
        }

        public void ResetCompany()
        {
            CompanyCode = string.Empty;
            DirectorateId = null;
        }

        public void ResetDirectorate()
        {
            DirectorateId = null;
        }
    }

    [Parameter]
    public Guid? ApplicantId { get; set; }
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    [CascadingParameter]
    protected PaymentFilterState PaymentFilters { get; set; } = new();
    protected ClaimsPrincipal _user = default!;
    private List<BreadcrumbItem> _breadcrumbItems = new();

    protected override async Task OnInitializedAsync()
    {
        var applicantId = ApplicantId;
        SetupBreadcrumb();
        _user = (await AuthenticationStateTask).User;

        //filters

        PaymentFilters.OnFilterChanged += async (state, field) =>
        {
            switch (field)
            {
                case PaymentFilterType.CompanyGroup:
                    await GetDropdownCompany();      // update company dropdown
                    PaymentFilters.ResetCompany();
                    break;
                case PaymentFilterType.Company:
                    await GetDropdownDirectorate(); // update directorate dropdown
                    PaymentFilters.ResetDirectorate();
                    break;
                case PaymentFilterType.Directorate:
                    break;
                default:
                    break;
            }
        };

        await GetAllDropdowns();
        //_displayName = _user.GetDisplayName() ?? string.Empty;
        //var response = await _paymentService.GetBankListAsync();
        //_dropdownItems = response.Result!.Items.ToList();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active(DisplayTextFor.Payment),
            CommonBreadcrumbFor.Active(DisplayTextFor.Overview)
        };
    }

    #region FIltering Dropdown
    private async Task GetAllDropdowns()
    {
        await GetDropdownCompanyGroup();
        await GetDropdownCompany();
        await GetDropdownDirectorate();
    }

    private async Task GetDropdownCompanyGroup()
    {
        var data = await _companyGroupService.GetCompanyGroupsAsync();
        if (data.Error is null)
        {
            //PaymentFilters.DropdownHps = data.Result!.Items
            //.Where(x => _companyCode == "1010" ? true : x.CompanyCode == _companyCode)
            //.Select(x => new DropdownItemResponse { Id = x.Id, Description = x.Name }).ToList();

            PaymentFilters.DropdownHps = data.Result!.Items
                .Select(x => new DropdownItemResponse { Id = x.Id, Description = x.Name }).ToList();
        }
    }

    private async Task GetDropdownCompany()
    {
        if (PaymentFilters.CompanyGroupId == null)
        {
            PaymentFilters.DropdownCompany = new List<DropdownItemStringResponse>();
            PaymentFilters.CompanyCode = string.Empty;
            return;
        }

        var data = await _companyService.GetCompanyByCompanyGroupIdAsync(PaymentFilters.CompanyGroupId.Value);
        if (data.Error is null)
        {
            PaymentFilters.DropdownCompany = data.Result!.Items
                .Select(x => new DropdownItemStringResponse { Id = x.Code, Description = x.Name }).OrderBy(x => x.Id).ToList();
        }
    }
    private async Task GetDropdownDirectorate()
    {
        if (string.IsNullOrEmpty(PaymentFilters.CompanyCode))
        {
            PaymentFilters.DropdownDirectorate = new List<DropdownItemResponse>();
            PaymentFilters.DirectorateId = null;
            return;
        }

        var data = await _directorateService.GetDirectotaeByCompanyCodeAsync(PaymentFilters.CompanyCode);
        if (data.Error is null)
        {
            PaymentFilters.DropdownDirectorate = data.Result!.Items
                .Select(x => new DropdownItemResponse { Id = x.Id, Description = x.Name }).OrderBy(x => x.Id).ToList();
        }
    }
    #endregion
}
