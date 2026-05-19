using OtpNet;
using Smlp.Application.Services.Otp;
using Smlp.Application.Services.Otp.Models.CreateOtp;
using Smlp.Shared.Common.Constants;
using QRCoder;
using static QRCoder.PayloadGenerator;

namespace Smlp.InfrastructureOtp;

public class OtpService : IOtpService
{
    private const string Issuer = "SMLP";

    public CreateOtpResponse CreateOtp(string username)
    {
        var key = KeyGeneration.GenerateRandomKey(64);
        var encodedKey = Base32Encoding.ToString(key);
        var secret = encodedKey.Replace("=", string.Empty);

        var otpModel = new CreateOtpResponse
        {
            Label = username,
            Secret = secret
        };

        var otp = new OneTimePassword()
        {
            Secret = otpModel.Secret,
            Issuer = Issuer,
            Label = otpModel.Label
        };

        if (CommonValueFor.EnvironmentName != EnvironmentNames.Production)
        {
            otp.Issuer = CommonValueFor.EnvironmentName == EnvironmentNames.Development
                ? $"[DEV]-{Issuer}"
                : $"[{CommonValueFor.EnvironmentName}]-{Issuer}";
        }

        otpModel.Url = otp.ToString();

        return otpModel;
    }

    public string GetCode(string secret, bool isTotp = true)
    {
        if (isTotp)
        {
            var totp = new Totp(Base32Encoding.ToBytes(secret), step: 180, OtpHashMode.Sha256);

            return totp.ComputeTotp();
        }
        else
        {
            var hotp = new Hotp(Base32Encoding.ToBytes(secret));

            return hotp.ComputeHOTP(1);
        }
    }

    public byte[] GetGraphic(string otpUrl)
    {
        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(otpUrl, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrCodeData);

        return qrCode.GetGraphic(5);
    }

    public bool VerifyCode(string secret, string code)
    {
        var hotp = new Hotp(Base32Encoding.ToBytes(secret));
        var isHotpValid = hotp.VerifyHotp(code, 1);

        var totp = new Totp(Base32Encoding.ToBytes(secret));
        var isTotpValid = totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);

        var totp2 = new Totp(Base32Encoding.ToBytes(secret), step: 180, OtpHashMode.Sha256);
        var isTotpValid2 = totp2.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);

        return isTotpValid || isHotpValid || isTotpValid2;
    }
}
