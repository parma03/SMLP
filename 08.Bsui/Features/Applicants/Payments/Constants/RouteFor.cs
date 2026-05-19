namespace Smlp.Bsui.Features.Applicants.Payments.Constants;

public static class RouteFor
{
    public static readonly string ApplicantPayment = $"{nameof(ApplicantPayment)}";
    public static readonly string DetailAcceptedPayment = $"{nameof(DetailAcceptedPayment)}";

    public static string PaymentDetailAccepted(Guid id, Guid applicantId)
    {
        return $"{ApplicantPayment}/{DetailAcceptedPayment}/{id}/{applicantId}";
    }
}
