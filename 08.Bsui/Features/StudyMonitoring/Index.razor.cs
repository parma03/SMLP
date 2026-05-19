using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Psmp.Bsui.Common.Constants;
using Psmp.Shared.StudyMonitorings.Constants;

namespace Psmp.Bsui.Features.StudyMonitoring;

public partial class Index
{
    #region Parameter
    [Parameter]
    public int TabIndex { get; set; }
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    #endregion

    private readonly List<BreadcrumbItem> _breadcrumbItems = new();
    private int _activeTabIndex = 0;
    public bool IsShowStudyPeriode { get; set; } = true;
    public bool IsShowGraduate { get; set; } = true;
    public bool IsShowAfterProgram { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        _activeTabIndex = TabIndex;
        SetupBreadcrumb();
        _user = (await AuthenticationStateTask).User;
    }

    private void SetupBreadcrumb()
    {
        // Tambahkan breadcrumb sesuai tab yang aktif
        switch (_activeTabIndex)
        {
            case 0:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyMonitoring));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyPeriod));
                break;
            case 1:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyMonitoring));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.Graduate));
                break;
            case 2:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyMonitoring));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.AfterProgram));
                break;
        }

        StateHasChanged();
    }
    private void OnTabChanged(int index)
    {
        _activeTabIndex = index;
        SetupBreadcrumb(); // Perbarui breadcrumb setiap kali tab berubah
    }
}
