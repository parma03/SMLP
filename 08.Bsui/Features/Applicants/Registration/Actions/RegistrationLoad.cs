/*
* Pertamina Scholarship Management Portal
* Copyright (c) 2024-2025 PT. Pertamina Persero
*/

using Psmp.Base.Scholarships.Enums;
using Psmp.Client.Services.BackEnd.Scholarships;
using Psmp.Shared.Scholarships.ScholarshipPosting.Queries;
using Psmp.Bsui.Features.Applicants.Registration.Models;

namespace Psmp.Bsui.Features.Applicants.Registration.Actions;

public class RegistrationLoad
{
    private readonly ScholarshipPostingService _posting;
    private readonly Registration _registration;
    private readonly ApplicantValidationRegistrationModel _validation;

    public RegistrationLoad(ScholarshipPostingService posting, Registration registration, ApplicantValidationRegistrationModel validation)
    {
        _posting = posting;
        _registration = registration;
        _validation = validation;
    }

    public async Task<List<GetScholarshipPostingResponse>> LoadScholarship()
    {
        var response = new List<GetScholarshipPostingResponse>();
        var request = new GetScholarshipPostingRequest()
        {
            CompanyCode = _validation.CompanyCode
        };
        var data = await _posting.GetScholarshipPostingAsync(request);

        if (data.Error == null)
        {
            _registration.IsLoadingGrid = true;
            response = data.Result!.Items
                .Where(x => x.Status == ScholarshipPostingStatus.Published && x.DueDate!.Value.Date >= DateTimeOffset.Now.Date).ToList();
            _registration.IsLoadingGrid = false;

            return response;
        }
        else
        {
            _registration.Error = data.Error;
            return response;
        }
    }
}
