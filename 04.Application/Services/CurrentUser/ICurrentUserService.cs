namespace Smlp.Application.Services.CurrentUser;

public interface ICurrentUserService
{
    public Guid? UserId { get; }
    public string Username { get; }
    public string ClientId { get; }
    public string? PositionId { get; }
    public string IpAddress { get; }
    public IList<string> Permissions { get; }
}
