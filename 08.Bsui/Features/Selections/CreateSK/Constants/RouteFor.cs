namespace Psmp.Bsui.Features.Selections.CreateSK.Constants;

public static class RouteFor
{
    public static readonly string Selection = $"{nameof(Selection)}";
    public static readonly string Decree = $"{nameof(TalentReview)}";
    public static readonly string Detail = $"{Selection}/{nameof(Decree)}/{nameof(Details)}";
    public static readonly string Adds = $"{Selection}/{nameof(CreateSK)}/Add";
    public static readonly string Edits = $"{Selection}/{nameof(CreateSK)}/Edit";
    public static readonly string Finals = $"{Selection}/{nameof(CreateSK)}/Final";
    public static string Selections()
    {
        return $"{Selection}";
    }
    public static string Details(Guid id)
    {
        return $"{Detail}/{id}";
    }
    public static string Add(Guid id)
    {
        return $"{Adds}/{id}";
    }
    public static string AddByBatch(int batch, int year)
    {
        return $"{Adds}/{batch}/{year}";
    }

    public static string Final(int batch, int year)
    {
        return $"{Finals}/{batch}/{year}";
    }

    public static string Edit(Guid id)
    {
        return $"{Edits}/{id}";
    }
    public static string EditByBatch(int batch, int year)
    {
        return $"{Edits}/{batch}/{year}";
    }
}
