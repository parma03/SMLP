using Smlp.Base.Selections.Enums;

namespace Smlp.Bsui.Features.Applicants.Registration.Models;

public class ApplicantValidationStatusModel
{
    public ApplicantValidationStatus ApplicantStatus { get; set; } = ApplicantValidationStatus.Exist;
    public bool IsCanApply { get; set; } = true;
    public string Code { get; set; } = default!;
    public SelectionStage? SelectionStage { get; set; }
}
