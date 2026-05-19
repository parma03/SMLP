namespace Smlp.Bsui.Features.Selections.BeritaAcara.Constants;

public static class RouteFor
{
    public static readonly string Selection = $"{nameof(Selection)}";
    public static readonly string BeritaAcara = $"{nameof(BeritaAcara)}";
    public static readonly string Detail = $"{Selection}/{nameof(BeritaAcara)}/{nameof(Details)}";
    public static readonly string Adds = $"{Selection}/{nameof(BeritaAcara)}";
    public static string Selections()
    {
        return $"{Selection}";
    }
    public static string Details(Guid id, int type)
    {
        return $"{Detail}/{id}/{type}";
    }
    public static string Add(string type, Guid id, int selectionType)
    {
        return $"{Adds}/{type}/{id}/{selectionType}";
    }
    public static string AddByBatch(string type, int batch, int year)
    {
        return $"{Adds}/{type}/{batch}/{year}";
    }

    public static string BeritaAcaraFinal(int batch, int year)
    {
        return $"{Selection}/{nameof(BeritaAcaraFinal)}/{batch}/{year}";
    }
    public static string SelectionsClose()
    {
        return $"{Selection}/Close";
    }
}
