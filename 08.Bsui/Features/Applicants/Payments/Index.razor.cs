using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Smlp.Bsui.Common.Constants;
using System.Security.Claims;
using MudBlazor;
using Smlp.Shared.Payments.Contants;
using Smlp.Bsui.Common.Extensions;

namespace Smlp.Bsui.Features.Applicants.Payments;

public partial class Index
{
    private List<BreadcrumbItem> _breadcrumbItems = new();
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    [Parameter]
    public string? Action { get; set; }
    [Parameter]
    public string? EmployeeNumber { get; set; }
    //private List<DropdownItemResponse> _dropdownItems = new();
    protected string _employeeId = string.Empty;
    protected string _username = string.Empty;
    protected bool _isLoading = false;
    protected string _displayName = default!;
    protected string _accountNumber = default!;
    protected bool _bankAccountExist = false;

    protected override async Task OnInitializedAsync()
    {
        SetupBreadcrumb();
        _user = (await AuthenticationStateTask).User;
        _displayName = _user.GetDisplayName() ?? string.Empty;
        _employeeId = _user.GetEmployeeId() ?? string.Empty;
        //var response = await _paymentService.GetBankListAsync();
        //_dropdownItems = response.Result!.Items.ToList();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active(DisplayTextFor.Payment),
            CommonBreadcrumbFor.Active(DisplayTextFor.Reimbursement)
        };
    }
}
