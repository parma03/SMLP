using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using MudBlazor;
using Psmp.Base.Scholarships.Enums;
using Psmp.Base.Selections.Enums;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Bsui.Features.Scholarships.ScholarshipPlan.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Documents.Queries.GetDocumentFile;
using Psmp.Shared.Masters.AdministrativeCriterias.Queries;
using Psmp.Shared.Masters.Queries;
using Psmp.Shared.Scholarships.Constants;
using Psmp.Shared.Scholarships.ScholarshipPlan.Commands.Create;
using Psmp.Shared.Scholarships.ScholarshipPlan.Commands.Update;
using Psmp.Shared.Services.Authorization.Constants;
using Syncfusion.Blazor.DropDowns;

namespace Psmp.Bsui.Features.Scholarships.ScholarshipPlan;

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
    public Guid IdScholarshipPlan { get; set; }
    public CreateScholarshipPlanRequest DataScholarshipPlan { get; set; } = new();
    protected List<GetCompanyGroupQueryResponse> _companyGroups = new();
    protected List<GetCompanyQueryResponse> _companies = new();
    protected List<GetDirectorateQueryResponse> _directorates = new();
    protected List<GetDivisionQueryResponse> _divisions = new();
    protected List<GetFungsiResponse> _fungsis = new();
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
    protected List<DropdownItemResponse> _locations = new();
    protected List<DropdownItemResponse> _scopes = new();
    protected List<DropdownItemResponse> _selectionTypes = new();
    protected List<DropdownItemResponse> _indiciplinaries = new();
    protected List<DropdownItemResponse> _mcu = new();
    public bool ValidToSave { get; set; }
    public string? Username { get; set; }
    public bool IsAllowEdit { get; set; } = false;

    protected string? _documentPathFinal = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        SetupBreadcrumb();
        if (_user.Identity!.IsAuthenticated)
        {
            Username = _user.GetUsername();
            _isLoading = true;
            await GetDropdownCompanyGroup();
            await GetDropdownCompany();
            await GetDropdownDegree();
            _locations = EnumExtentions.ToDropdownItem<ScholarshipLocation>();
            _scopes = EnumExtentions.ToDropdownItem<ScholarshipScope>();
            _selectionTypes = EnumExtentions.ToDropdownItem<SelectionType>();
            _indiciplinaries = EnumExtentions.ToDropdownItem<IndiciplinaryStatus>();
            _mcu = EnumExtentions.ToDropdownItem<McuStatus>();
            await GetDataAdministrativeCriteria();
            await Task.Delay(300);
            _isLoading = false;
            IdScholarshipPlan = RegistrationId;
            if (_user.Claims.Where(x => x.Type.ToLower().Trim() == "permission" && x.Value.ToLower().Trim().Contains(Permissions.Psmp_ScholarshipPlan_Crud.ToLower().ToString())).ToList().Count > 0)
            {
                IsAllowEdit = true;
                DataScholarshipPlan.PostingYear = DateTime.Now.Year;
                DataScholarshipPlan.McuStatus = McuStatus.P5.GetIntValue();
                DataScholarshipPlan.PerformanceRating = 5.5f;
            }

            if (!IsCreated) //Edit > bind data
            {
                var scholarshipPlan = await _scholarshipPlanService.GetScholarshipPlanByIdAsync(IdScholarshipPlan);
                if (scholarshipPlan.Error is not null)
                {
                    _snackbar.AddError($"Data Scholarship Plan with id {IdScholarshipPlan} not found");
                }
                else
                {
                    if (scholarshipPlan.Result is not null)
                    {
                        DataScholarshipPlan.Code = scholarshipPlan.Result.Code;
                        DataScholarshipPlan.CompanyGroupId = scholarshipPlan.Result.CompanyGroupId;
                        _companyGroupId = DataScholarshipPlan.CompanyGroupId;
                        await GetDropdownCompany();
                        DataScholarshipPlan.CompanyCode = scholarshipPlan.Result.CompanyCode;
                        _companyCode = DataScholarshipPlan.CompanyCode;
                        await GetDropdownDirectorate();
                        DataScholarshipPlan.DirectorateId = scholarshipPlan.Result.DirectorateId;
                        _directorateId = DataScholarshipPlan.DirectorateId;
                        await GetDropdownFungsi();
                        DataScholarshipPlan.FungsiId = scholarshipPlan.Result.FungsiId;
                        _fungsiId = DataScholarshipPlan.FungsiId;
                        await GetDropdownDivision();
                        DataScholarshipPlan.DivisionId = scholarshipPlan.Result.DivisionId;
                        DataScholarshipPlan.BusinessIssue = scholarshipPlan.Result.BusinessIssue;
                        DataScholarshipPlan.BusinessIssueReferences = scholarshipPlan.Result.BusinessIssueReferences;
                        DataScholarshipPlan.PerformanceIssue = scholarshipPlan.Result.PerformanceIssue;
                        DataScholarshipPlan.CompetencyIssue = scholarshipPlan.Result.CompetencyIssue;
                        DataScholarshipPlan.DegreeId = scholarshipPlan.Result.DegreeId;
                        DataScholarshipPlan.Faculty = scholarshipPlan.Result.Faculty;
                        DataScholarshipPlan.Major = scholarshipPlan.Result.Major;
                        DataScholarshipPlan.UniversityReference = scholarshipPlan.Result.UniversityReference;
                        DataScholarshipPlan.UniversityLocation = (int)scholarshipPlan.Result.UniversityLocation;
                        DataScholarshipPlan.LatestEducation = scholarshipPlan.Result.LatestEducation;
                        DataScholarshipPlan.WorkingExperience = scholarshipPlan.Result.WorkingExperience;
                        DataScholarshipPlan.Scope = (int)scholarshipPlan.Result.ScholarshipScope;
                        DataScholarshipPlan.AwardeeQuota = scholarshipPlan.Result.AwardeeQuota;
                        DataScholarshipPlan.Batch = scholarshipPlan.Result.Batch;
                        DataScholarshipPlan.SelectionType = (int)scholarshipPlan.Result.SelectionType;
                        DataScholarshipPlan.PostingYear = scholarshipPlan.Result.PostingYear;
                        DataScholarshipPlan.MinimumPrl = scholarshipPlan.Result.Prl;
                        DataScholarshipPlan.MaximumAge = scholarshipPlan.Result.Age;
                        DataScholarshipPlan.MinimumWorkPeriod = scholarshipPlan.Result.WorkPeriod;
                        DataScholarshipPlan.PerformanceRating = scholarshipPlan.Result.PerformanceRating;
                        DataScholarshipPlan.IndiciplinaryStatus = (int)scholarshipPlan.Result.Indiciplinary;
                        DataScholarshipPlan.McuStatus = (int)scholarshipPlan.Result.Mcu;
                        DataScholarshipPlan.ProjectInovation = scholarshipPlan.Result.ProjectInovation;
                        DataScholarshipPlan.TOEFLScore = scholarshipPlan.Result.TOEFLScore;

                        if (scholarshipPlan.Result.DocumentApprovedId is not null)
                        {
                            var docFinal = await _documentService.GetDocumentFileByDocumentTypeAsync(new GetDocumentFileRequest { DocumentId = (Guid)scholarshipPlan.Result.DocumentApprovedId, DocumentType = DocumentType.FinalScholarshipPlan });
                            if (docFinal.Error is null)
                            {
                                var dataDoc = docFinal.Result;
                                _documentPathFinal = "data:application/pdf;base64," + Convert.ToBase64String(dataDoc!.Content);
                            }
                        }

                        // status approved can not edit data
                        DataScholarshipPlan.Status = scholarshipPlan.Result.Status;
                        var status = scholarshipPlan.Result.Status;
                        if (status is ScholarshipPlanStatus.Approved or ScholarshipPlanStatus.WaitingForApproval)
                        {
                            IsAllowEdit = false;
                        }
                    }
                }
            }
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
            Degree = _degrees.Where(x => x.Id == DataScholarshipPlan.DegreeId).Select(x => x.Name).FirstOrDefault(),
            Location = DataScholarshipPlan.UniversityLocation == 1 ? "Abroad" : "Domestic"
        };
        var data = await _administrativeCriteriaService.GetAdministrativeCriteriaAsync(request);
        if (data.Error is null)
        {
            _administrativeCriterias = data.Result!.Items.ToList();
            if (_administrativeCriterias.FirstOrDefault(x => x.Name == "MinimumPrl") != null)
            {
                DataScholarshipPlan.MinimumPrl = Convert.ToInt32(_administrativeCriterias.FirstOrDefault(x => x.Name == "MinimumPrl")!.Value);
            }

            if (_administrativeCriterias.FirstOrDefault(x => x.Name == "MaximumAge") != null)
            {
                DataScholarshipPlan.MaximumAge = Convert.ToInt32(_administrativeCriterias.FirstOrDefault(x => x.Name == "MaximumAge")!.Value);
            }

            if (_administrativeCriterias.FirstOrDefault(x => x.Name == "MinimumWorkPeriod") != null)
            {
                DataScholarshipPlan.MinimumWorkPeriod = Convert.ToInt32(_administrativeCriterias.FirstOrDefault(x => x.Name == "MinimumWorkPeriod")!.Value);
            }

            if (_administrativeCriterias.FirstOrDefault(x => x.Name == "AveragePerformanceRating") != null)
            {
                var pr = _administrativeCriterias.FirstOrDefault(x => x.Name == "AveragePerformanceRating");
                if (pr != null)
                {
                    DataScholarshipPlan.PerformanceRating = Convert.ToSingle(pr.Value.ToString().Trim());
                }
                else
                {
                    DataScholarshipPlan.PerformanceRating = 0;
                }
            }

            if (_administrativeCriterias.FirstOrDefault(x => x.Name == "IndisciplinaryRecord") != null)
            {
                DataScholarshipPlan.IndiciplinaryStatus = Convert.ToInt32(_administrativeCriterias.FirstOrDefault(x => x.Name == "IndisciplinaryRecord")!.Value);
            }

            if (_administrativeCriterias.FirstOrDefault(x => x.Name == "McuStatus") != null)
            {
                DataScholarshipPlan.McuStatus = Convert.ToInt32(_administrativeCriterias!.FirstOrDefault(x => x.Name == "McuStatus")!.Value);
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
        _directorates = new();
        DataScholarshipPlan.DirectorateId = 0;
        _divisions = new();
        DataScholarshipPlan.DivisionId = 0;
        StateHasChanged();
    }

    public async void OnChangeCompanyAsync(ChangeEventArgs<string, GetCompanyQueryResponse> args)
    {
        _companyCode = args.ItemData.Code;
        await GetDropdownDirectorate();
        _divisions = new();
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
    public async void OnChangeDegree(ChangeEventArgs<int, GetDegreeQueryResponse> args)
    {
        if (args.ItemData != null)
        {
            await GetDataAdministrativeCriteria();
            StateHasChanged();
        }
    }
    public async void OnChangeUniversityLocation(ChangeEventArgs<int, DropdownItemResponse> args)
    {
        if (args.ItemData != null)
        {
            await GetDataAdministrativeCriteria();
            StateHasChanged();
        }
    }
    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active(DisplayTextFor.Scholarship),
            CommonBreadcrumbFor.Active(DisplayTextFor.ScholarshipPlan),
            CommonBreadcrumbFor.Active(DisplayTextFor.Registration)
        };
    }

    private void OnClickCancelAsync(MouseEventArgs e)
    {
        if (IsAllowEdit)
        {
            VisiblePopup = true;
        }
        else
        {
            _navigationManager.NavigateTo(RouteFor.Scholarships, true);
        }
    }

    private async Task OnClickSave(MouseEventArgs e)
    {
        if (!ValidateOnSave())
        {
            return;
        }

        if (IsCreated)
        {
            //set request save data
            CreateScholarshipPlanRequest request = new()
            {
                Code = "", //generate in Backend
                CompanyGroupId = DataScholarshipPlan.CompanyGroupId,
                CompanyCode = DataScholarshipPlan.CompanyCode,
                DirectorateId = DataScholarshipPlan.DirectorateId,
                DivisionId = DataScholarshipPlan.DivisionId,
                DegreeId = DataScholarshipPlan.DegreeId,
                BusinessIssue = DataScholarshipPlan.BusinessIssue,
                BusinessIssueReferences = DataScholarshipPlan.BusinessIssueReferences,
                PerformanceIssue = DataScholarshipPlan.PerformanceIssue,
                CompetencyIssue = DataScholarshipPlan.CompetencyIssue,
                Faculty = DataScholarshipPlan.Faculty,
                Major = DataScholarshipPlan.Major,
                FungsiId = DataScholarshipPlan.FungsiId,
                UniversityReference = DataScholarshipPlan.UniversityReference,
                LatestEducation = DataScholarshipPlan.LatestEducation,
                WorkingExperience = DataScholarshipPlan.WorkingExperience,
                AwardeeQuota = DataScholarshipPlan.AwardeeQuota,
                Batch = DataScholarshipPlan.Batch,
                PostingYear = DataScholarshipPlan.PostingYear,
                MinimumPrl = DataScholarshipPlan.MinimumPrl,
                MaximumAge = DataScholarshipPlan.MaximumAge,
                MinimumWorkPeriod = DataScholarshipPlan.MinimumWorkPeriod,
                PerformanceRating = DataScholarshipPlan.PerformanceRating,
                Scope = DataScholarshipPlan.Scope,
                McuStatus = DataScholarshipPlan.McuStatus,
                SelectionType = DataScholarshipPlan.SelectionType,
                IndiciplinaryStatus = DataScholarshipPlan.IndiciplinaryStatus,
                UniversityLocation = DataScholarshipPlan.UniversityLocation,
                Username = Username ?? string.Empty,
                Notes = "",
                Status = ScholarshipPlanStatus.Requested,
                ProjectInovation = DataScholarshipPlan.ProjectInovation,
                TOEFLScore = DataScholarshipPlan.TOEFLScore
            };

            var response = await _scholarshipPlanService.CreateScholarshipPlanAsync(request);
            if (response.Error is not null)
            {
                _snackbar.AddError(response.Error.Detail);
            }
            else
            {
                _snackbar.AddSuccess($"Success created Scholarship Plan, Code : {response.Result!.Code} with ID {response.Result!.Id}");
                VisibleSave = true;
                await Task.Delay(1500);
                _navigationManager.NavigateTo(RouteFor.Scholarships, true);
            }
        }
        else
        {
            UpdateScholarshipPlanRequest request = new()
            {
                Id = RegistrationId,
                Code = DataScholarshipPlan.Code.Trim(),
                CompanyGroupId = DataScholarshipPlan.CompanyGroupId,
                CompanyCode = DataScholarshipPlan.CompanyCode,
                DirectorateId = DataScholarshipPlan.DirectorateId,
                FungsiId = DataScholarshipPlan.FungsiId,
                DivisionId = DataScholarshipPlan.DivisionId,
                DegreeId = DataScholarshipPlan.DegreeId,
                BusinessIssue = DataScholarshipPlan.BusinessIssue,
                BusinessIssueReferences = DataScholarshipPlan.BusinessIssueReferences,
                PerformanceIssue = DataScholarshipPlan.PerformanceIssue,
                CompetencyIssue = DataScholarshipPlan.CompetencyIssue,
                Faculty = DataScholarshipPlan.Faculty,
                Major = DataScholarshipPlan.Major,
                UniversityReference = DataScholarshipPlan.UniversityReference,
                LatestEducation = DataScholarshipPlan.LatestEducation,
                WorkingExperience = DataScholarshipPlan.WorkingExperience,
                AwardeeQuota = DataScholarshipPlan.AwardeeQuota,
                Batch = DataScholarshipPlan.Batch,
                PostingYear = DataScholarshipPlan.PostingYear,
                MinimumPrl = DataScholarshipPlan.MinimumPrl,
                MaximumAge = DataScholarshipPlan.MaximumAge,
                MinimumWorkPeriod = DataScholarshipPlan.MinimumWorkPeriod,
                PerformanceRating = DataScholarshipPlan.PerformanceRating.ToString("n2"),
                Scope = DataScholarshipPlan.Scope,
                McuStatus = DataScholarshipPlan.McuStatus,
                SelectionType = DataScholarshipPlan.SelectionType,
                IndiciplinaryStatus = DataScholarshipPlan.IndiciplinaryStatus,
                UniversityLocation = DataScholarshipPlan.UniversityLocation,
                Username = Username ?? string.Empty,
                Notes = "",
                Status = ScholarshipPlanStatus.Requested,
                ProjectInovation = DataScholarshipPlan.ProjectInovation,
                TOEFLScore = DataScholarshipPlan.TOEFLScore
            };

            var response = await _scholarshipPlanService.UpdateScholarshipPlanAsync(request);
            if (response.Error is not null)
            {
                _snackbar.AddError(response.Error.Detail);
            }
            else
            {
                _snackbar.AddSuccess($"Success updated Scholarship Plan, Code : {response.Result!.Code} with ID {response.Result!.Id}");
                VisibleSave = true;
                await Task.Delay(1500);
                _navigationManager.NavigateTo(RouteFor.Scholarships, true);
            }
        }
    }

    private bool ValidateOnSave()
    {
        if (DataScholarshipPlan.CompanyCode == null)
        {
            _snackbar.AddError("Company cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.DirectorateId == 0)
        {
            _snackbar.AddError("Directorate cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.BusinessIssue == null || DataScholarshipPlan.BusinessIssue.Trim() == "")
        {
            _snackbar.AddError("Business Issues cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.BusinessIssueReferences == null || DataScholarshipPlan.BusinessIssueReferences.Trim() == "")
        {
            _snackbar.AddError("Reference of Business Issues cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.PerformanceIssue == null || DataScholarshipPlan.PerformanceIssue.Trim() == "")
        {
            _snackbar.AddError("Performance Issues cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.CompetencyIssue == null || DataScholarshipPlan.CompetencyIssue.Trim() == "")
        {
            _snackbar.AddError("Competency Issues cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.DegreeId == 0)
        {
            _snackbar.AddError("Degree cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.Faculty == null || DataScholarshipPlan.Faculty.Trim() == "")
        {
            _snackbar.AddError("Faculty cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.Major == null || DataScholarshipPlan.Major.Trim() == "")
        {
            _snackbar.AddError("Major cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.UniversityReference == null || DataScholarshipPlan.UniversityReference.Trim() == "")
        {
            _snackbar.AddError("University Reference cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.UniversityReference.Length > 150)
        {
            _snackbar.AddError("University Reference cannot exceed 150 characters");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.UniversityLocation == 0)
        {
            _snackbar.AddError("University Location cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.LatestEducation == null || DataScholarshipPlan.LatestEducation.Trim() == "")
        {
            _snackbar.AddError("Latest Education Background cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.LatestEducation.Length > 150)
        {
            _snackbar.AddError("Latest Education Background cannot exceed 150 characters");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.WorkingExperience == null || DataScholarshipPlan.WorkingExperience.Trim() == "")
        {
            _snackbar.AddError("Minimum Working Experience cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.WorkingExperience.Length > 150)
        {
            _snackbar.AddError("Minimum Working Experience cannot exceed 150 characters");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.Scope == 0)
        {
            _snackbar.AddError("Scope cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.AwardeeQuota == 0)
        {
            _snackbar.AddError("Awardee Quota cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.Batch == 0)
        {
            _snackbar.AddError("Batch cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.TOEFLScore == 0)
        {
            _snackbar.AddError("TOEFL Score cannot be empty");
            ValidToSave = false;
        }
        //else if (DataScholarshipPlan.SelectionType == 0)
        //{
        //    _snackbar.AddError("Selection Category cannot be empty");
        //    ValidToSave = false;
        //}
        else if (DataScholarshipPlan.PostingYear == 0)
        {
            _snackbar.AddError("Posting Year cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.MinimumPrl == 0)
        {
            _snackbar.AddError("Minimum PRL cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.MaximumAge == 0)
        {
            _snackbar.AddError("Maximum Age cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.MinimumWorkPeriod == 0)
        {
            _snackbar.AddError("Minimum Work Period cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.PerformanceRating == 0)
        {
            _snackbar.AddError("Average Performance Rating cannot be empty");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.PerformanceRating is < 3 or > (float)5.5)
        {
            _snackbar.AddError("Average Performance Minimum 3 and Maximum 5.5!");
            ValidToSave = false;
        }
        else if (DataScholarshipPlan.ProjectInovation == null || DataScholarshipPlan.ProjectInovation.Trim() == "")
        {
            _snackbar.AddError("Project Innovation cannot be empty");
            ValidToSave = false;
        }
        //else if (DataScholarshipPlan.IndiciplinaryStatus == 0)
        //{
        //    _snackbar.AddError("Indiciplinary Record Last 3 Years cannot be empty");
        //    ValidToSave = false;
        //}
        //else if (DataScholarshipPlan.McuStatus == 0)
        //{
        //    _snackbar.AddError("MCU Status cannot be empty");
        //    ValidToSave = false;
        //}
        else
        {
            ValidToSave = true;
        }

        return ValidToSave;
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
}
