namespace Smlp.Application.Services.Email.Models;

public class EmailSettings
{
    public string Provider { get; set; } = "None"; // None, Gmail, Smtp
    public SmtpEmailSettings? Smtp { get; set; }
    public SmtpEmailSettings? Gmail { get; set; }

    public SmtpEmailSettings? GetActiveSettings() => Provider switch
    {
        "Gmail" => Gmail,
        "Smtp" => Smtp,
        _ => null
    };
}

public class SmtpEmailSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = "SMLP Coffee Loyalty";
    public bool UseSsl { get; set; } = false;
    public bool UseStartTls { get; set; } = true;
}
