/*
*/

using Smlp.Base.Scholarships.Enums;
using Smlp.Base.Selections.Enums;
using Smlp.Bsui.Features.Applicants.Registration.Models;
using Smlp.Client.Services.BackEnd.Applicants;
using Smlp.Client.Services.Hris;
using Smlp.Shared.Common.Extensions;
using Smlp.Shared.Scholarships.ScholarshipPosting.Queries;

namespace Smlp.Bsui.Features.Applicants.Registration.Actions;

public class RegistrationValidation
{
    private readonly Registration _registration;
    private readonly GetScholarshipPostingResponse _posting;
    private readonly HrisService _hris;
    private readonly ApplicantService _applicantService;

    public RegistrationValidation(Registration registration, GetScholarshipPostingResponse posting, HrisService hris, ApplicantService applicantService)
    {
        _registration = registration;
        _posting = posting;
        _hris = hris;
        _applicantService = applicantService;
    }

    public async Task<ApplicantValidationStatusModel> IsExistApplicantAsync(Guid scholarshipId, string employeeId)
    {
        var response = new ApplicantValidationStatusModel();
        var applicantExist = await _applicantService.GetApplicantByScholarshipEmployeeIdAsync(scholarshipId, employeeId);
        if (applicantExist.Error == null)
        {
            if (applicantExist.Result!.EmployeeNumber == null)
            {
                //checking applicant exist in other scholarship
                var aplicantRegistered = await _applicantService.GetApplicantByEmployeeIdAsync(employeeId);
                if (aplicantRegistered.Error is null)
                {
                    if (aplicantRegistered.Result!.Items.Count > 0)
                    {
                        foreach (var item in aplicantRegistered.Result!.Items)
                        {
                            if (item.ApplicantStatus is Base.Selections.Enums.ApplicantStatus.Failed
                                || item.ApplicantStatus is Base.Selections.Enums.ApplicantStatus.Canceled)
                            {
                                response.IsCanApply = true;
                            }
                            else
                            {
                                response.ApplicantStatus = ApplicantValidationStatus.ExistInOther;
                                response.IsCanApply = false;
                                response.Code = item.Code;
                                response.SelectionStage = item.SelectionStage;
                                break;
                            }
                        }
                    }
                    else
                    {
                        response.IsCanApply = true;
                    }
                }
                else
                {
                    response.IsCanApply = false;
                    response.ApplicantStatus = ApplicantValidationStatus.Error;
                    _registration.Error = aplicantRegistered.Error;
                }
            }
            else
            {
                if (applicantExist.Result.ScholarshipPostingId != scholarshipId &&
                    (applicantExist.Result.ApplicantStatus is Base.Selections.Enums.ApplicantStatus.Canceled
                    || applicantExist.Result.ApplicantStatus is Base.Selections.Enums.ApplicantStatus.Failed))
                {
                    response.IsCanApply = true;
                }
                else if (applicantExist.Result.ScholarshipPostingId == scholarshipId && applicantExist.Result.ApplicantStatus is Base.Selections.Enums.ApplicantStatus.Canceled)
                {
                    response.IsCanApply = false;
                    // if employee apply -> cancel -> apply again in same scholarship
                    response.ApplicantStatus = ApplicantValidationStatus.Canceled;
                }
                else if (applicantExist.Result.ScholarshipPostingId == scholarshipId)
                {
                    response.IsCanApply = false;
                    response.ApplicantStatus = ApplicantValidationStatus.Exist;
                }
                else
                {
                    //checking applicant exist in other scholarship
                    var aplicantRegistered = await _applicantService.GetApplicantByEmployeeIdAsync(employeeId);
                    if (aplicantRegistered.Error is null)
                    {
                        if (aplicantRegistered.Result!.Items.Count > 0)
                        {
                            foreach (var item in aplicantRegistered.Result!.Items)
                            {
                                if (item.ApplicantStatus is Base.Selections.Enums.ApplicantStatus.Failed
                                    || item.ApplicantStatus is Base.Selections.Enums.ApplicantStatus.Canceled)
                                {
                                    response.IsCanApply = true;
                                }
                                else
                                {
                                    response.IsCanApply = false;
                                    response.ApplicantStatus = ApplicantValidationStatus.ExistInOther;
                                    response.Code = item.Code;
                                    response.SelectionStage = item.SelectionStage;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            response.IsCanApply = true;
                        }
                    }
                    else
                    {
                        response.IsCanApply = false;
                        response.ApplicantStatus = ApplicantValidationStatus.Error;
                        _registration.Error = applicantExist.Error;
                    }
                }
            }
        }
        else
        {
            response.IsCanApply = false;
            response.ApplicantStatus = ApplicantValidationStatus.Error;
            _registration.Error = applicantExist.Error;
        }

        return response;
    }

    public async Task<bool> ValidateApplicant(string employeeId)
    {
        var response = true;
        var profile = await _hris.GetEmployeeByIdAsync(employeeId ?? string.Empty);
        var birthDate = profile.BirthDate ?? DateTime.Now.ToString("MM/dd/yyyy");
        var hiringDate = profile.HiringDate ?? DateTime.Now.ToString("MM/dd/yyyy");
        var mcuStatus = 0;
        if (!string.IsNullOrEmpty(profile.McuResult))
        {
            mcuStatus = (int)Enum.Parse<McuStatus>(profile.McuResult);
        }

        _registration.ApplicantValidationResponse = new List<ApplicantValidationModel>();

        //validate minimum prl
        if (Convert.ToInt16(profile.Prl) < _posting.Prl)
        {
            response = false;
            _registration.ApplicantValidationResponse.Add(new Models.ApplicantValidationModel
            {
                Item = nameof(_posting.Prl),
                Value = $"Minimum PRL is {_posting.Prl}"
            });
        }

        //validate maximum age
        if (Int64Extensions.GetAge(DateTimeExtensions.ToDateTime(birthDate)) > _posting.Age)
        {
            response = false;
            _registration.ApplicantValidationResponse.Add(new Models.ApplicantValidationModel
            {
                Item = nameof(_posting.Age),
                Value = $"Maximum Age is {_posting.Age}"
            });
        }

        //validate minimum work periode
        if (Int64Extensions.GetAge(DateTimeExtensions.ToDateTime(hiringDate)) < _posting.WorkPeriod)
        {
            response = false;
            _registration.ApplicantValidationResponse.Add(new Models.ApplicantValidationModel
            {
                Item = nameof(_posting.WorkPeriod),
                Value = $"Minimum Work Period is {_posting.WorkPeriod}"
            });
        }

        //validate indiciplinary
        if (!await _hris.IsGrievanceByIdAsync(employeeId ?? string.Empty))
        {
            response = false;
            _registration.ApplicantValidationResponse.Add(new Models.ApplicantValidationModel
            {
                Item = nameof(_posting.Indiciplinary),
                Value = "Indiciplinary detected"
            });
        }

        //validate performance rete
        if (await _hris.GetPerformanceRate(employeeId ?? string.Empty) < _posting.PerformanceRating)
        {
            response = false;
            _registration.ApplicantValidationResponse.Add(new Models.ApplicantValidationModel
            {
                Item = nameof(_posting.PerformanceRating),
                Value = $"Minimum Performance Rating is {_posting.PerformanceRating}"
            });
        }

        //validate mcu
        //if (mcuStatus == 0 || mcuStatus > (int)Enum.Parse<McuStatus>(profile.McuResult))
        //{
        //    response = false;
        //    _registration.ApplicantValidationResponse.Add(new Models.ApplicantValidationModel
        //    {
        //        Item = nameof(_posting.Mcu),
        //        Value = $"MCU Status is {_posting.Mcu}"
        //    });
        //}

        return response;
    }
}
