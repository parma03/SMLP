namespace Smlp.Bsui.Features.Payments.Constants;

public static class RouteFor
{
    public static readonly string Payment = $"{nameof(Payment)}";
    public static readonly string ApplicantPayment = $"{nameof(ApplicantPayment)}";

    public static string ApplicantPaymentd(string employeeNumber, string action)
    {
        return $"{ApplicantPayment}/{employeeNumber}/{action}";
    }

    public static string PaymentOverviewDetail(string requestId)
    {
        return $"payment-overview/detail/{requestId}";
    }

}
