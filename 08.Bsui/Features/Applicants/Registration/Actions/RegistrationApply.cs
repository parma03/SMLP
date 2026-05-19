/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using System.Security.Claims;
using Psmp.Base.Scholarships.Enums;
using Psmp.Bsui.Common.Extensions;
using Psmp.Client.Services.BackEnd.Applicants;
using Psmp.Client.Services.Hris;
using Psmp.Shared.Applicants.Commands.Create;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Common.Responses;
using Psmp.Shared.Scholarships.ScholarshipPosting.Queries;

namespace Psmp.Bsui.Features.Applicants.Registration.Actions;

public class RegistrationApply
{
    private readonly Registration _registration;
    private readonly RegistrationService _registrationService;
    private readonly HrisService _hris;
    private readonly ClaimsPrincipal _user;

    public RegistrationApply(Registration registration, RegistrationService registrationService, HrisService hris, ClaimsPrincipal authenticationState)
    {
        _registration = registration;
        _registrationService = registrationService;
        _hris = hris;
        _user = authenticationState;
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreateApplicantAsync(GetScholarshipPostingResponse posting, string employeeId)
    {
        if (posting is not null)
        {
            var profile = await _hris.GetEmployeeByIdAsync(employeeId);
            var profileMasterData = await _hris.GetEmployeeMasterDataByIdAsync(employeeId);

            var applicant = new CreateApplicantRequest
            {
                ScholarshipPostingId = posting.Id,
                Name = profile.EmployeeName ?? _user.GetDisplayName() ?? string.Empty,
                Email = profile.Email ?? _user.GetUsername() ?? string.Empty,
                EmployeeNumber = profile.EmployeeId ?? _user.GetEmployeeId() ?? string.Empty,
                Position = profileMasterData.PositionName ?? _user.GetPositionName() ?? string.Empty,
                CompanyCode = profile.CompanyCode ?? _user.GetCompanyCode() ?? string.Empty,
                DirectorateId = Int64Extensions.StringToInt(profile.DirectoratId),
                DivisionId = Int64Extensions.StringToInt(profile.DivisionId),
                Gender = Int64Extensions.StringToInt(profile.GenderCode),
            };
            var responseApplicant = await _registrationService.CreateApplicantAsync(applicant);

            if (responseApplicant.Error is null)
            {
                return responseApplicant;
            }
            else
            {
                _registration.Error = responseApplicant.Error;
                return responseApplicant;
            }
        }
        else
        {
            return new();
        }
    }

    public async Task<ResponseResult<CommonCreateResponse>> CreateApplicantAdministrativeCriteria(Guid applicantId, string employeeId)
    {
        var profile = await _hris.GetEmployeeByIdAsync(employeeId);
        var profileMasterData = await _hris.GetEmployeeMasterDataByIdAsync(employeeId);

        var birthDate = profile.BirthDate ?? DateTime.Now.ToString("MM/dd/yyyy");
        var hiringDate = profileMasterData.HiringDate ?? DateTime.Now.ToString("MM/dd/yyyy");
        var mcuStatus = 0;
        if (!string.IsNullOrEmpty(profile.McuResult))
        {
            mcuStatus = (int)Enum.Parse<McuStatus>(profile.McuResult);
        }

        var criteria = new CreateApplicantAdministrativeCriteriaRequest
        {
            ApplicantId = applicantId,
            Prl = Convert.ToInt16(profile.Prl),
            Age = Convert.ToInt16(Int64Extensions.GetAge(DateTimeExtensions.ToDateTime(birthDate))),
            WorkPeriod = Convert.ToInt16(Int64Extensions.GetAge(DateTimeExtensions.ToDateTime(hiringDate))),
            PerformanceRating = await _hris.GetPerformanceRate(employeeId),
            IndiciplinaryStatus = await _hris.IsGrievanceByIdAsync(employeeId) ? 0 : 1,
            McuStatus = mcuStatus
        };

        var responseCriteria = await _registrationService.CreateApplicantAdministrativeCriteriaAsync(criteria);

        if (responseCriteria.Error is null)
        {
            return responseCriteria;
        }
        else
        {
            _registration.Error = responseCriteria.Error;
            return new();
        }
    }
}
