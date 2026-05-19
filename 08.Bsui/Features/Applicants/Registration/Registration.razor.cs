/*
*/

using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Smlp.Base.Scholarships.Enums;
using Smlp.Bsui.Common.Extensions;
using Smlp.Bsui.Features.Applicants.Registration.Actions;
using Smlp.Bsui.Features.Applicants.Registration.Components;
using Smlp.Bsui.Features.Applicants.Registration.Models;
using Smlp.Shared.Common.Constants;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Scholarships.ScholarshipPosting.Queries;

namespace Smlp.Bsui.Features.Applicants.Registration;

public partial class Registration
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    protected ClaimsPrincipal _user = default!;
    public bool IsLoadingGrid { get; set; } = false;
    public ErrorResponse? Error { get; set; }
    public GetScholarshipPostingResponse Posting { get; set; } = default!;
    private IList<GetScholarshipPostingResponse> Postings { get; set; } = default!;
    public IList<ApplicantValidationModel> ApplicantValidationResponse { get; set; } = default!;

    protected string _employeeId = string.Empty;
    protected string _username = string.Empty;
    protected string _companyCode = "0000";
    protected bool _isLoading = false;
    protected bool _isLoadingDialog = false;

    protected override async Task OnInitializedAsync()
    {
        _user = (await AuthenticationStateTask).User;

        _isLoading = true;

        _employeeId = _user.GetEmployeeId() ?? string.Empty;
        _username = _user.GetUsername() ?? string.Empty;
        _companyCode = _user.GetCompanyCode() ?? "0000";

        #region bypass user applicant

        var isProductionEnvironment = _webHostEnvironment.IsInEnvironment(EnvironmentNames.Production);
        if (!isProductionEnvironment && _hrisOptions.Value.ByPass)
        {
            var asResponse = await _applicantByPassService.GetApplicantByPassEmployeeIdAsync(_user.GetEmployeeId() ?? string.Empty);
            if (asResponse.Result != null && asResponse.Result.EmployeeIdAlias != null)
            {
                _employeeId = asResponse.Result.EmployeeIdAlias;
                _username = asResponse.Result.EmailAlias;
                _companyCode = "1010";
            }
        }

        #endregion bypass user applicant
        var validationRegistration = new ApplicantValidationRegistrationModel { CompanyCode = _companyCode };
        var registrationLoad = new RegistrationLoad(_postingService, this, validationRegistration);
        Postings = await registrationLoad.LoadScholarship();

        #region test data to HRIS
        var profile = await _hrisService.GetEmployeeByIdAsync(_employeeId);
        var birthDate = profile.BirthDate ?? DateTime.Now.ToString("MM/dd/yyyy");
        var hiringDate = profile.HiringDate ?? DateTime.Now.ToString("MM/dd/yyyy");
        var mcuStatus = 0;
        if (!string.IsNullOrEmpty(profile.McuResult))
        {
            mcuStatus = (int)Enum.Parse<McuStatus>(profile.McuResult);
        }
        #endregion

        _isLoading = false;
    }

    protected async Task OnCommandClick(GetScholarshipPostingResponse posting)
    {
        if (posting is not null)
        {
            if (posting.ScholarshipScope == ScholarshipScope.InternalEntity && posting.CompanyCode != _companyCode)
            {
                _snackbar.AddError("Can not apply this scholarship!!");
                return;
            }

            var options = new DialogOptions { CloseOnEscapeKey = false, DisableBackdropClick = true, MaxWidth = MaxWidth.Medium };

            var parameters = new DialogParameters<RegistrationDetail> { { x => x.Content, posting } };

            //show dialog apply
            var dialog = await _dialogService.ShowAsync<RegistrationDetail>("Registration Detail", parameters, options);
            var result = await dialog.Result;

            //on click apply
            if (!result.Canceled)
            {
                _isLoadingDialog = true;
                StateHasChanged();
                //checking applicant is exists
                var registrationValidation = new RegistrationValidation(this, posting, _hrisService, _applicantService);
                var applicantExist = await registrationValidation.IsExistApplicantAsync(posting.Id, _employeeId);

                if (Error != null)
                {
                    _isLoadingDialog = false;
                    return;
                }
                else if (!applicantExist.IsCanApply && applicantExist.ApplicantStatus is Base.Selections.Enums.ApplicantValidationStatus.Canceled)
                {
                    // if exist on same scholarship after canceled
                    _isLoadingDialog = false;
                    await _dialogService.ShowAsync<RegistrationDetailExistCanceled>("Information Registration Detail");
                }
                else if (!applicantExist.IsCanApply && applicantExist.ApplicantStatus is Base.Selections.Enums.ApplicantValidationStatus.ExistInOther)
                {
                    // if exist in other scholarship
                    _isLoadingDialog = false;
                    var param = new DialogParameters<RegistrationDetailExistInOther>
                    {
                        { x => x.Code, applicantExist.Code },
                        { x => x.SelectionStage, applicantExist.SelectionStage }
                    };
                    await _dialogService.ShowAsync<RegistrationDetailExistInOther>("Information Registration Detail", param);
                }
                else if (!applicantExist.IsCanApply && applicantExist.ApplicantStatus is Base.Selections.Enums.ApplicantValidationStatus.Exist)
                {
                    _isLoadingDialog = false;
                    await _dialogService.ShowAsync<RegistrationDetailExist>("Information Registration Detail");
                }
                else
                {
                    await RegisterApplicant(posting, registrationValidation);
                    _isLoadingDialog = false;
                }

                _isLoadingDialog = false;
                StateHasChanged();
            }
        }
    }

    private async Task RegisterApplicant(GetScholarshipPostingResponse applicantProfile, RegistrationValidation registrationValidation)
    {
        //create new applicant
        var registrationApply = new RegistrationApply(this, _registrationService, _hrisService, _user);
        var responseApplicant = await registrationApply.CreateApplicantAsync(applicantProfile, _employeeId);
        if (responseApplicant.Error is not null)
        {
            Error = responseApplicant.Error;
            return;
        }

        if (responseApplicant.Result!.Id != Guid.Empty)
        {
            //create new applicant detail
            var responseCriteria = await registrationApply.CreateApplicantAdministrativeCriteria(responseApplicant.Result.Id, _employeeId);
            var validateApplicant = await registrationValidation.ValidateApplicant(_employeeId);

            //create new selection
            await CreateSelection(responseApplicant, validateApplicant);
        }
        else
        {
            var parameters = new DialogParameters<RegistrationDetailFailed> { { x => x.ValidationResponse, ApplicantValidationResponse } };
            await _dialogService.ShowAsync<RegistrationDetailFailed>("Failed Registration Detail", parameters);
        }
    }

    private async Task CreateSelection(ResponseResult<CommonCreateResponse> responseApplicant, bool validateApplicant)
    {
        if (validateApplicant)
        {
            var selection = new SelectionCreate(this, _selectionOverviewService, _selectionAdministrationService, _selectionTOEFLService);
            var overview = await selection.CreateSelectionOverview(responseApplicant.Result!.Id, true);
            await selection.CreateSelectionAdministration(responseApplicant.Result.Id, true);
            await selection.CreateSelectionToefl(responseApplicant.Result.Id);

            await _dialogService.ShowAsync<RegistrationDetailSuccess>("Success Registration Detail");
        }
        else
        {
            var selection = new SelectionCreate(this, _selectionOverviewService, _selectionAdministrationService, _selectionTOEFLService);
            await selection.CreateSelectionOverview(responseApplicant.Result!.Id, false);
            await selection.CreateSelectionAdministration(responseApplicant.Result.Id, false);
            var parameters = new DialogParameters<RegistrationDetailFailed> { { x => x.ValidationResponse, ApplicantValidationResponse } };
            await _dialogService.ShowAsync<RegistrationDetailFailed>("Failed Registration Detail", parameters);
        }
    }
}
