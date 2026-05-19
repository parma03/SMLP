/*
*/

namespace Smlp.Infrastructure.Persistence.Common.Constants;

public static class CommonColumnTypes
{
    public static readonly string VarbinaryMax = "varbinary(max)";

    public static string Nvarchar(int length)
    {
        return $"nvarchar({length})";
    }

    public static readonly string SmallInt = "smallint";

    public static readonly string Ntext = "ntext";
}
