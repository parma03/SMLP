using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Smlp.Bsui.Common.Constants;
using Smlp.Shared.Scholarships.Constants;

namespace Smlp.Bsui.Features.Dashboards;

public partial class Index
{
    private readonly List<BreadcrumbItem> _breadcrumbItems = new();
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    public bool IsShowTrackList { get; set; } = true;
    public bool IsShowOverView { get; set; } = true;
    public bool IsShowScholarshipPlans { get; set; } = true;
    public bool IsShowCompanySelections { get; set; } = true;
    public bool IsShowUnivRegistration { get; set; } = true;
    public bool IsShowStudyMonitoring { get; set; } = true;
    public bool IsShowPayment { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        SetupBreadcrumb();
        _user = (await AuthenticationStateTask).User;
        if (_user.Identity.IsAuthenticated)
        {
            //if (_user.Claims.Where(x => x.Type.ToLower().Trim() == "permission" && x.Value.ToLower().Trim().Contains("dashboards.overview.view")).ToList().Count > 0)
            //{
            //    IsShowDashboardsOverView = true;
            //}

            //if (_user.Claims.Where(x => x.Type.ToLower().Trim() == "permission" && x.Value.ToLower().Trim().Contains("dashboards.scholarshipplan.view")).ToList().Count > 0)
            //{
            //    IsShowDashboardsScholarshipPlan = true;
            //}
        }
    }

    //private void SetupBreadcrumb()
    //{
    //    _breadcrumbItems = new()
    //    {
    //        CommonBreadcrumbFor.Active(DisplayTextFor.Dashboard),
    //        CommonBreadcrumbFor.Active(DisplayTextFor.OverView)
    //    };
    //}

    private int _activeTabIndex = 0;
    private static readonly List<BreadcrumbItem> _breadcrumbItems1 = new();

    private void SetupBreadcrumb()
    {
        //Atur breadcrumb dasar

        //_breadcrumbItems = new List<BreadcrumbItem>
        //        {
        //            CommonBreadcrumbFor.Active(DisplayTextFor.Dashboard), // Breadcrumb utama
        //        };

        // Tambahkan breadcrumb sesuai tab yang aktif
        switch (_activeTabIndex)
        {
            case 0:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.Dashboard));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.TaskList));
                break;
            case 1:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.Dashboard));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.OverView));
                break;
            case 2:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.Dashboard));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.ScholarshipPlans));
                break;
            case 3:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.Dashboard));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.CompanySelections));
                break;
            case 4:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.Dashboard));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.UnivRegistration));
                break;
            case 5:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.Dashboard));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.StudyMonitoring));
                break;
            case 6:
                _breadcrumbItems.Clear();
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.Dashboard));
                _breadcrumbItems.Add(CommonBreadcrumbFor.Active(DisplayTextFor.Payment));
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
