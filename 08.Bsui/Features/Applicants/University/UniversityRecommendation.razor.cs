/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Psmp.Base.Universities.Enums;
using Psmp.Bsui.Common.Extensions;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Masters.Queries;
using Psmp.Shared.Universities.UniversityRecommendation.Queries;

namespace Psmp.Bsui.Features.Applicants.University;

public partial class UniversityRecommendation
{
    #region Parameter
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    #endregion

    #region Protected Properties
    public ErrorResponse? Error { get; set; }
    protected bool _isLoading = false;
    protected bool _isCanView = false;
    protected string _displayName = default!;
    protected string _employeeId = string.Empty;
    protected string _username = string.Empty;
    #endregion

    protected List<GetUniversityRecommendationQueryResponse> _dataGrid = new();

    #region Filter
    protected List<GetScholarshipSubjectQueryResponse> _subjects = new();
    #endregion

    #region Dropdown Properties
    protected UniversityCategory _category = 0;
    protected int _subject = 0;
    protected int _year = 0;
    protected string _universityName = string.Empty;
    protected string _country = string.Empty;
    public List<DropdownEnumResponse<UniversityCategory>> _dropdownEnumCategories = new();
    public List<DropdownItemResponse> _dropdownYear = new();
    public List<DropdownItemStringResponse> _dropdownUniversityName = new();
    public List<DropdownItemStringResponse> _dropdownCountry = new();
    #endregion

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;
        _employeeId = _user.GetEmployeeId() ?? string.Empty;
        _displayName = _user.GetDisplayName() ?? string.Empty;

        _isLoading = true;
        _isCanView = false;
        #region bypass user applicant

        var isProductionEnvironment = _webHostEnvironment.IsInEnvironment(EnvironmentNames.Production);
        if (!isProductionEnvironment && _hrisOptions.Value.ByPass)
        {
            var asResponse = await _applicantByPassService.GetApplicantByPassEmployeeIdAsync(_user.GetEmployeeId() ?? string.Empty);
            if (asResponse.Result != null && asResponse.Result.EmployeeIdAlias != null)
            {
                _employeeId = asResponse.Result.EmployeeIdAlias;
                _username = asResponse.Result.EmailAlias;
            }
        }

        if (string.IsNullOrEmpty(_employeeId))
        {
            _snackbar.AddError("Not Found Employee ID");
            _isLoading = false;
            return;
        }
        #endregion bypass user applicant

        // check if exist data scholarship
        var applicant = await _applicantService.GetApplicantByEmployeeIdAsync(_employeeId);
        if (applicant.Error != null)
        {
            Error = applicant.Error;
        }
        else
        {
            var lastApplicant = applicant.Result!.Items.OrderByDescending(x => x.Created).FirstOrDefault();
            if (lastApplicant is not null && lastApplicant!.Id != Guid.Empty)
            {
                _isCanView = true;
                await SetDataOnLoadAsync();
                await GetDropdownCategories();
                await GetDropdownSubject();
                await GetDropdownYear();
                await GetDropdownUniversityName();
                await GetDropdownCountry();
            }
        }

        _isLoading = false;
    }

    #region Data Grid
    private async Task SetDataOnLoadAsync()
    {
        var data = await _universityRecommendationService.GetUniversityRecommendationAsync();
        if (data.Error is null)
        {
            _dataGrid = data.Result!.Items.ToList();
            if (_category != 0)
            {
                _dataGrid = _dataGrid.Where(x => x.UniversityCategory == _category).ToList();
            }

            if (_subject != 0)
            {
                _dataGrid = _dataGrid.Where(x => x.ScholarshipSubject.Id == _subject).ToList();
            }

            if (_year != 0)
            {
                _dataGrid = _dataGrid.Where(x => x.Dateline.Year == _year).ToList();
            }

            if (!string.IsNullOrEmpty(_universityName))
            {
                _dataGrid = _dataGrid.Where(x => x.University == _universityName).ToList();
            }

            if (!string.IsNullOrEmpty(_country))
            {
                _dataGrid = _dataGrid.Where(x => x.Country == _country).ToList();
            }
        }
        else
        {
            Error = data.Error;
        }
    }
    #endregion

    #region Dropdown
    private async Task GetDropdownCategories()
    {
        _dropdownEnumCategories = EnumExtentions.ToDropdownEnum<UniversityCategory>();
        await Task.CompletedTask;
    }

    private async Task GetDropdownSubject()
    {
        var subjects = await _scholarshipSubjectService.GetScholarshipSubjectsAsync();
        if (subjects.Error is null)
        {
            _subjects = subjects.Result!.Items.ToList();
            _subjects = _subjects.OrderBy(x => x.Name).ToList();
        }

        await Task.CompletedTask;
    }

    private async Task GetDropdownYear()
    {
        _dropdownYear = _dataGrid.Select(x => new DropdownItemResponse { Id = x.Dateline.Year, Description = x.Dateline.Year.ToString() }).DistinctBy(x => x.Id).ToList();
        await Task.CompletedTask;
    }

    private async Task GetDropdownUniversityName()
    {
        _dropdownUniversityName = _dataGrid.Select(x => new DropdownItemStringResponse { Id = x.University, Description = x.University }).DistinctBy(x => x.Id).ToList();
        await Task.CompletedTask;
    }

    private async Task GetDropdownCountry()
    {
        _dropdownCountry = _dataGrid.Select(x => new DropdownItemStringResponse { Id = x.Country, Description = x.Country }).DistinctBy(x => x.Id).ToList();
        await Task.CompletedTask;
    }
    #endregion

    #region OnChange Event
    protected async Task OnChangeCategoryAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<UniversityCategory, DropdownEnumResponse<UniversityCategory>> args)
    {
        if (args.ItemData == null)
        {
            _category = 0;
        }
        else
        {
            _category = args.ItemData.Id;
        }

        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    protected async Task OnChangeSubjectAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, GetScholarshipSubjectQueryResponse> args)
    {
        _subject = args.Value;
        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    protected async Task OnChangeYearAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, DropdownItemResponse> args)
    {
        _year = args.Value;
        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    protected async Task OnChangeUniversityNameAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string, DropdownItemStringResponse> args)
    {
        _universityName = args.Value;
        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    protected async Task OnChangeCountryAsync(Syncfusion.Blazor.DropDowns.ChangeEventArgs<string, DropdownItemStringResponse> args)
    {
        _country = args.Value;
        await SetDataOnLoadAsync();
        StateHasChanged();
    }
    #endregion
}
