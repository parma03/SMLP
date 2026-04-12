using Smlp.Shared.Common.Extensions;

namespace Smlp.Shared.Audits.Constants;

public static class DisplayTextFor
{
    public const string Audits = nameof(Audits);
    public const string Audit = nameof(Audit);

    public static readonly string TableName = nameof(TableName).SplitWords();
    public static readonly string EntityName = nameof(EntityName).SplitWords();
    public const string EntityId = "Entity ID";
    public static readonly string ActionType = nameof(ActionType).SplitWords();
    public static readonly string ActionName = nameof(ActionName).SplitWords();
    public const string ClientApplicationId = "Client Application ID";
    public const string FromIpAddress = "From IP Address";
    public static readonly string FromGeolocation = nameof(FromGeolocation).SplitWords();
    public static readonly string OldValues = nameof(OldValues).SplitWords();
    public static readonly string NewValues = nameof(NewValues).SplitWords();

    public static readonly string FromDate = nameof(FromDate).SplitWords();
    public static readonly string FromTime = nameof(FromTime).SplitWords();
    public static readonly string ToDate = nameof(ToDate).SplitWords();
    public static readonly string ToTime = nameof(ToTime).SplitWords();
}
