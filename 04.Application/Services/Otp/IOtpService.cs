using Smlp.Application.Services.Otp.Models.CreateOtp;

namespace Smlp.Application.Services.Otp;

public interface IOtpService
{
    CreateOtpResponse CreateOtp(string username);

    string GetCode(string secret, bool isTotp = true);

    byte[] GetGraphic(string otpUrl);

    bool VerifyCode(string secret, string code);
}
