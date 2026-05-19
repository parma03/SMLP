using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Psmp.Base.Scholarships.Enums;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Bsui.Features.Scholarships.SholarshipPosting.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Masters.AdministrativeCriterias.Queries;
using Psmp.Shared.Masters.Queries;
using Psmp.Shared.Scholarships.ScholarshipPlan.Queries;
using Psmp.Shared.Scholarships.ScholarshipPosting.Commands.Create;
using Psmp.Shared.Scholarships.ScholarshipPosting.Commands.Update;
using Psmp.Shared.Scholarships.ScholarshipPosting.Constants;
using Syncfusion.Blazor.Calendars;
using Syncfusion.Blazor.DropDowns;

namespace Psmp.Bsui.Features.Scholarships.SholarshipPosting;

public partial class Registration
{
    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    private ClaimsPrincipal _user = default!;
    private List<BreadcrumbItem> _breadcrumbItems = new();

    [Parameter]
    public Guid RegistrationId { get; set; }

    [Parameter]
    public bool IsCreated { get; set; }

    protected bool _isLoading = false;
    protected bool _isLoadingData = false;
    public Guid IdScholarshipPosting { get; set; }

    private SfDatePicker<DateTimeOffset?> DatePickerObj { get; set; }
    public CreateScholarshipPostingRequest DataScholarshipPosting { get; set; } = new();
    public UpdateScholarshipPostingRequest UpdateDataScholarshipPosting { get; set; } = new();
    protected List<GetCompanyGroupQueryResponse> _companyGroups = new();
    protected List<GetCompanyQueryResponse> _companies = new();
    protected List<GetDirectorateQueryResponse> _directorates = new();
    protected List<GetFungsiResponse> _fungsis = new();
    protected List<GetDivisionQueryResponse> _divisions = new();
    protected List<GetDegreeQueryResponse> _degrees = new();
    protected List<GetFacultyQueryResponse> _faculties = new();
    protected List<GetScholarshipSubjectQueryResponse> _subjects = new();
    protected List<GetAdministrativeCriteriaQueryResponse> _administrativeCriterias = new();
    protected ErrorResponse? _error;
    private bool VisiblePopup { get; set; }
    private bool VisibleSave { get; set; }
    protected int _companyGroupId = 0;
    protected string _companyCode = "0000";
    protected int _directorateId = 0;
    protected int _fungsiId = 0;
    protected int _maxQuota = 0;
    protected List<DropdownItemResponse> _locations = new();
    protected List<DropdownItemResponse> _scopes = new();
    protected List<DropdownItemResponse> _selectionTypes = new();
    protected List<DropdownItemResponse> _indiciplinaries = new();
    protected List<DropdownItemResponse> _mcu = new();
    protected List<GetScholarshipPlanResponse> _scholarshipPlan = new();
    protected List<DropdownItemResponse> _selectionType = new();
    private UpdateScholarshipPostingStatusRequest ScholarshipPostingStatusRequest { get; set; } = new();
    public bool IsAllowEdit { get; set; } = false;
    public int _totalAplicant;
    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        SetupBreadcrumb();
        if (_user.Identity.IsAuthenticated)
        {
            _isLoading = true;
            await GetDropdownScholarshipPlan();
            _selectionType = EnumExtentions.ToDropdownItem<SelectionType>();
            await GetDropdownCompanyGroup();
            await GetDropdownCompany();
            await GetDropdownDegree();
            _locations = EnumExtentions.ToDropdownItem<ScholarshipLocation>();
            _scopes = EnumExtentions.ToDropdownItem<ScholarshipScope>();
            _selectionTypes = EnumExtentions.ToDropdownItem<SelectionType>();
            _indiciplinaries = EnumExtentions.ToDropdownItem<IndiciplinaryStatus>();
            _mcu = EnumExtentions.ToDropdownItem<McuStatus>();
            await Task.Delay(300);
            _isLoading = false;
            if (_user.Claims.Where(x => x.Type.ToLower().Trim() == "permission" && x.Value.ToLower().Trim().Contains("scholarship.posting.crud")).ToList().Count > 0)
            {
                IsAllowEdit = true;
            }

            var responseBatch = await _scholarshipPostingService.GetScholarshipPostingBatchAsync();
            var batch = responseBatch.Result.Items.OrderByDescending(x => x.Batch).FirstOrDefault();
            if (!IsCreated) //Edit > bind data
            {
                var response = await _scholarshipPostingService.GetScholarshipPostingByIdAsync(RegistrationId);
                if (response != null)
                {
                    var data = response.Result;
                    UpdateDataScholarshipPosting.Id = data.Id;
                    UpdateDataScholarshipPosting.Code = data.Code;
                    UpdateDataScholarshipPosting.CompanyGroupId = data.CompanyGroupId;
                    _companyGroupId = UpdateDataScholarshipPosting.CompanyGroupId;
                    await GetDropdownCompany();
                    UpdateDataScholarshipPosting.CompanyCode = data.CompanyCode;
                    _companyCode = UpdateDataScholarshipPosting.CompanyCode;
                    await GetDropdownDirectorate();
                    UpdateDataScholarshipPosting.DirectorateId = data.DirectorateId;
                    _directorateId = UpdateDataScholarshipPosting.DirectorateId;
                    await GetDropdownFungsi();
                    UpdateDataScholarshipPosting.FungsiId = data.FungsiId;
                    _fungsiId = UpdateDataScholarshipPosting.FungsiId;
                    await GetDropdownDivision();
                    UpdateDataScholarshipPosting.DivisionId = data.DivisionId;
                    UpdateDataScholarshipPosting.BusinessIssue = data.BusinessIssue;
                    UpdateDataScholarshipPosting.BusinessIssueReferences = data.BusinessIssueReferences;
                    UpdateDataScholarshipPosting.PerformanceIssue = data.PerformanceIssue;
                    UpdateDataScholarshipPosting.CompetencyIssue = data.CompetencyIssue;
                    UpdateDataScholarshipPosting.ProjectInovation = data.ProjectInovation ?? "-";
                    UpdateDataScholarshipPosting.DegreeId = data.DegreeId;
                    UpdateDataScholarshipPosting.Faculty = data.Faculty;
                    UpdateDataScholarshipPosting.Major = data.Major;
                    UpdateDataScholarshipPosting.DueDate = data.DueDate;
                    UpdateDataScholarshipPosting.PostingDate = data.PostingDate;
                    UpdateDataScholarshipPosting.UniversityReference = data.UniversityReference;
                    UpdateDataScholarshipPosting.UniversityLocation = (int)data.UniversityLocation;
                    UpdateDataScholarshipPosting.LatestEducation = data.LatestEducation;
                    UpdateDataScholarshipPosting.WorkingExperience = data.WorkingExperience;
                    UpdateDataScholarshipPosting.Scope = (int)data.ScholarshipScope;
                    UpdateDataScholarshipPosting.AwardeeQuota = data.AwardeeQuota - data.CurentAplicant;
                    UpdateDataScholarshipPosting.Batch = data.Status == ScholarshipPostingStatus.Published ? data.Batch : batch != null ? batch!.Batch + 1 : data.Batch;
                    UpdateDataScholarshipPosting.SelectionType = (int)data.SelectionType;
                    UpdateDataScholarshipPosting.PostingYear = data.PostingYear;
                    UpdateDataScholarshipPosting.MinimumPrl = data.Prl;
                    UpdateDataScholarshipPosting.MaximumAge = data.Age;
                    UpdateDataScholarshipPosting.MinimumWorkPeriod = data.WorkPeriod;
                    UpdateDataScholarshipPosting.PerformanceRating = data.PerformanceRating.ToString();
                    UpdateDataScholarshipPosting.IndiciplinaryStatus = (int)data.Indiciplinary;
                    UpdateDataScholarshipPosting.McuStatus = (int)data.Mcu;
                    UpdateDataScholarshipPosting.ScholarshipPlanId = data.ScholarshipPlanId;
                    UpdateDataScholarshipPosting.Status = data.Status;
                    _maxQuota = data.AwardeeQuota - data.CurentAplicant;
                    _totalAplicant = data.CurentAplicant;
                    UpdateDataScholarshipPosting.TOEFLScore = data.TOEFLScore;
                }
            }
            else
            {
                var response = await _scholarshipPostingService.GetScholarshipPostingByIdAsync(RegistrationId);
                if (response != null)
                {
                    var data = response.Result;
                    UpdateDataScholarshipPosting.Id = data.Id;
                    UpdateDataScholarshipPosting.Code = data.Code;
                    UpdateDataScholarshipPosting.CompanyGroupId = data.CompanyGroupId;
                    _companyGroupId = UpdateDataScholarshipPosting.CompanyGroupId;
                    await GetDropdownCompany();
                    UpdateDataScholarshipPosting.CompanyCode = data.CompanyCode;
                    _companyCode = UpdateDataScholarshipPosting.CompanyCode;
                    await GetDropdownDirectorate();
                    UpdateDataScholarshipPosting.DirectorateId = data.DirectorateId;
                    _directorateId = UpdateDataScholarshipPosting.DirectorateId;
                    await GetDropdownFungsi();
                    UpdateDataScholarshipPosting.FungsiId = data.FungsiId;
                    _fungsiId = UpdateDataScholarshipPosting.FungsiId;
                    await GetDropdownDivision();
                    UpdateDataScholarshipPosting.DivisionId = data.DivisionId;
                    UpdateDataScholarshipPosting.DegreeId = data.DegreeId;
                    UpdateDataScholarshipPosting.Faculty = data.Faculty;
                    UpdateDataScholarshipPosting.Major = data.Major;
                    UpdateDataScholarshipPosting.UniversityReference = data.UniversityReference;
                    UpdateDataScholarshipPosting.UniversityLocation = (int)data.UniversityLocation;
                    UpdateDataScholarshipPosting.LatestEducation = data.LatestEducation;
                    UpdateDataScholarshipPosting.WorkingExperience = data.WorkingExperience;
                    UpdateDataScholarshipPosting.Scope = (int)data.ScholarshipScope;
                    UpdateDataScholarshipPosting.AwardeeQuota = data.AwardeeQuota - data.CurentAplicant;
                    UpdateDataScholarshipPosting.Batch = batch!.Batch + 1;
                    UpdateDataScholarshipPosting.SelectionType = (int)data.SelectionType;
                    UpdateDataScholarshipPosting.PostingYear = data.PostingYear;
                    UpdateDataScholarshipPosting.MinimumPrl = data.Prl;
                    UpdateDataScholarshipPosting.MaximumAge = data.Age;
                    UpdateDataScholarshipPosting.MinimumWorkPeriod = data.WorkPeriod;
                    UpdateDataScholarshipPosting.PerformanceRating = data.PerformanceRating.ToString();
                    UpdateDataScholarshipPosting.IndiciplinaryStatus = (int)data.Indiciplinary;
                    UpdateDataScholarshipPosting.McuStatus = (int)data.Mcu;
                    UpdateDataScholarshipPosting.ScholarshipPlanId = data.ScholarshipPlanId;
                    _maxQuota = data.AwardeeQuota - data.CurentAplicant;
                    _totalAplicant = data.CurentAplicant;
                    UpdateDataScholarshipPosting.TOEFLScore = data.TOEFLScore;
                }
            }
        }
    }

    private async Task GetDropdownScholarshipPlan()
    {
        var param = new GetScholarshipPlanRequest();
        var scholarshipPlan = await _scholarshipPlanService.GetScholarshipPlanAllAsync(param);

        if (scholarshipPlan.Error is null)
        {
            _scholarshipPlan = scholarshipPlan.Result!.Items.ToList();
            _scholarshipPlan = _scholarshipPlan.Where(x => x.Status == ScholarshipPlanStatus.Approved).ToList();
        }
    }
    private async Task GetDropdownCompanyGroup()
    {
        var data = await _companyGroupService.GetCompanyGroupsAsync();
        if (data.Error is null)
        {
            _companyGroups = data.Result!.Items.ToList();
        }
    }
    private async Task GetDropdownCompany()
    {
        var data = await _companyService.GetCompanyByCompanyGroupIdAsync(_companyGroupId);
        if (data.Error is null)
        {
            _companies = data.Result!.Items.ToList();
            _companies = _companies.OrderBy(x => x.Code).ToList();
        }
    }

    private async Task GetDropdownDirectorate()
    {
        var data = await _directorateService.GetDirectotaeByCompanyCodeAsync(_companyCode);
        if (data.Error is null)
        {
            _directorates = data.Result!.Items.ToList();
            //_directorates = _directorates.OrderBy(x => x.Name).ToList();
        }
    }

    private async Task GetDropdownDivision()
    {
        var data = await _divisionService.GetDivisionByDirectorateIdAsync(_fungsiId);
        if (data.Error is null)
        {
            _divisions = data.Result!.Items.ToList();
        }
    }

    private async Task GetDropdownFungsi()
    {
        var data = await _fungsiService.GetFungsiByDirectorateIdAsync(_directorateId);
        if (data.Error is null)
        {
            _fungsis = data.Result!.Items.ToList();
        }
    }

    private async Task GetDropdownDegree()
    {
        var data = await _degreeService.GetDegreesAsync();
        if (data.Error is null)
        {
            _degrees = data.Result!.Items.ToList();
        }
    }

    private async Task GetDataAdministrativeCriteria()
    {
        var request = new GetAdministrativeCriteriaQueryRequest()
        {
            Degree = _degrees.Where(x => x.Id == UpdateDataScholarshipPosting.DegreeId).Select(x => x.Name).FirstOrDefault(),
            Location = UpdateDataScholarshipPosting.UniversityLocation == 1 ? "Abroad" : "Domestic"
        };
        var data = await _administrativeCriteriaService.GetAdministrativeCriteriaAsync(request);
        if (data.Error is null)
        {
            _administrativeCriterias = data.Result!.Items.ToList();
            if (_administrativeCriterias.FirstOrDefault(x => x.Name == "MinimumPrl") != null)
            {
                DataScholarshipPosting.MinimumPrl = Convert.ToInt32(_administrativeCriterias.FirstOrDefault(x => x.Name == "MinimumPrl").Value);
            }

            if (_administrativeCriterias.FirstOrDefault(x => x.Name == "MaximumAge") != null)
            {
                DataScholarshipPosting.MaximumAge = Convert.ToInt32(_administrativeCriterias.FirstOrDefault(x => x.Name == "MaximumAge").Value);
            }

            if (_administrativeCriterias.FirstOrDefault(x => x.Name == "MinimumWorkPeriod") != null)
            {
                DataScholarshipPosting.MinimumWorkPeriod = Convert.ToInt32(_administrativeCriterias.FirstOrDefault(x => x.Name == "MinimumWorkPeriod").Value);
            }

            if (_administrativeCriterias.FirstOrDefault(x => x.Name == "AveragePerformanceRating") != null)
            {
                DataScholarshipPosting.PerformanceRating = _administrativeCriterias.FirstOrDefault(x => x.Name == "AveragePerformanceRating").Value.ToString().Trim();
            }

            if (_administrativeCriterias.FirstOrDefault(x => x.Name == "IndisciplinaryRecord") != null)
            {
                DataScholarshipPosting.IndiciplinaryStatus = Convert.ToInt32(_administrativeCriterias.FirstOrDefault(x => x.Name == "IndisciplinaryRecord").Value);
            }

            if (_administrativeCriterias.FirstOrDefault(x => x.Name == "McuStatus") != null)
            {
                DataScholarshipPosting.McuStatus = Convert.ToInt32(_administrativeCriterias.FirstOrDefault(x => x.Name == "McuStatus").Value);
            }
        }
    }

    private async Task GetDropdownFaculty()
    {
        var faculties = await _facultyService.GetFacultiesAsync();
        if (faculties.Error is null)
        {
            _faculties = faculties.Result!.Items.ToList();
            _faculties = _faculties.OrderBy(x => x.Name).ToList();
        }
    }

    private async Task GetDropdownSubject()
    {
        var subjects = await _scholarshipSubjectService.GetScholarshipSubjectsAsync();
        if (subjects.Error is null)
        {
            _subjects = subjects.Result!.Items.ToList();
            _subjects = _subjects.OrderBy(x => x.Name).ToList();
        }
    }

    public async void OnChangeCompanyGroupAsync(ChangeEventArgs<int, GetCompanyGroupQueryResponse> args)
    {
        _companyGroupId = args.ItemData.Id;
        await GetDropdownCompany();
        StateHasChanged();
    }

    public async void OnChangeCompanyAsync(ChangeEventArgs<string, GetCompanyQueryResponse> args)
    {
        _companyCode = args.ItemData.Code;
        await GetDropdownDirectorate();
        StateHasChanged();
    }

    public async void OnChangeDirectorateAsync(ChangeEventArgs<int, GetDirectorateQueryResponse> args)
    {
        if (args.ItemData != null)
        {
            _directorateId = args.ItemData.Id;
            await GetDropdownFungsi();
            StateHasChanged();
        }
    }
    public async void OnChangeFungsiAsync(ChangeEventArgs<int, GetFungsiResponse> args)
    {
        if (args.ItemData != null)
        {
            _fungsiId = args.ItemData.Id;
            await GetDropdownDivision();
            StateHasChanged();
        }
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active(DisplayTextForScholarshipPosting.ScholarshipPosting)
        };
    }

    private void OnClickCancelAsync()
    {
        VisiblePopup = true;
    }
    private async Task OnClickSave()
    {
        if (IsCreated)
        {
            var response = await _scholarshipPostingService.RePublishScholarshipPostingAsync(UpdateDataScholarshipPosting);
            if (response.Error is not null)
            {
                _snackbar.AddError(response.Error.Detail);
            }
            else
            {
                _snackbar.AddSuccess($"Success Reposting Scholarship Posting");
                VisibleSave = true;
                await Task.Delay(1500);
                _navigationManager.NavigateTo(RouteFor.Scholarships, true);
            }
        }
        else
        {
            var response = await _scholarshipPostingService.UpdateScholarshipPostingAsync(UpdateDataScholarshipPosting);
            if (response.Error is not null)
            {
                _snackbar.AddError(response.Error.Detail);
            }
            else
            {
                _snackbar.AddSuccess($"Success Update Scholarship Posting");
                VisibleSave = true;
                await Task.Delay(1500);
                _navigationManager.NavigateTo(RouteFor.Scholarships, true);
            }
        }
    }
    private async Task OnClickSaveComplite()
    {
        UpdateDataScholarshipPosting.AwardeeQuota = _totalAplicant;
        var response = await _scholarshipPostingService.UpdateScholarshipPostingAsync(UpdateDataScholarshipPosting);
        if (response.Error is not null)
        {
            _snackbar.AddError(response.Error.Detail);
        }
        else
        {
            _snackbar.AddSuccess($"Success Update Scholarship Posting");
            VisibleSave = true;
            await Task.Delay(1500);
            _navigationManager.NavigateTo(RouteFor.Scholarships, true);
        }
    }
    private async Task OnClickPublish(Guid? id)
    {
        if (UpdateDataScholarshipPosting.Id is not null)
        {
            var guids = new List<Guid>
            {
                (Guid)id
            };

            ScholarshipPostingStatusRequest.Id = guids;
            if (ScholarshipPostingStatusRequest.Id.Count > 0)
            {
                var response = await _scholarshipPostingService.UpdateScholarshipPostingStatusAsync(ScholarshipPostingStatusRequest);
                if (response.Error != null)
                {
                    _snackbar.AddError(response.Error.Detail);
                }
                else
                {
                    _snackbar.AddSuccess($"Success Update Scholarship Posting");
                    VisibleSave = true;
                    await Task.Delay(1500);
                    _navigationManager.NavigateTo(RouteFor.Scholarships, true);
                }
            }
        }
    }

    #region Popup Event

    private void DialogOpen()
    {
        VisiblePopup = true;
    }

    private void DialogClose()
    {
        VisiblePopup = false;
    }

    private void ClickedNo()
    {
        VisiblePopup = false;
    }

    private void ClickedYes()
    {
        VisiblePopup = false;
        _navigationManager.NavigateTo(RouteFor.Scholarships, true);
    }

    #endregion Popup Event

    private void DisableDate(RenderDayCellEventArgs args)
    {
        var view = DatePickerObj.CurrentView();
        if (view == "Month" && args.Date < DateTimeOffset.Now.Date)
        {
            args.IsDisabled = true;
        }
    }
}
