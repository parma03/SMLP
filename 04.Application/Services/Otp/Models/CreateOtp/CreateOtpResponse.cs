namespace Smlp.Application.Services.Otp.Models.CreateOtp;

public class CreateOtpResponse
{
    public string Label { get; set; } = default!;
    public string Secret { get; set; } = default!;
    public string Url { get; set; } = default!;
}
