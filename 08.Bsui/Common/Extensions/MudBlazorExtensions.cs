using MudBlazor;
using Psmp.Shared.Common.Enums;
using Psmp.Shared.Common.Requests;

namespace Psmp.Bsui.Common.Extensions;

public static class MudBlazorExtensions
{
    public static void AddSuccess(this ISnackbar snackbar, string message)
    {
        snackbar.Add(message, Severity.Success);
    }

    public static void AddInfo(this ISnackbar snackbar, string message)
    {
        snackbar.Add(message, Severity.Info);
    }

    public static void AddWarning(this ISnackbar snackbar, string message)
    {
        snackbar.Add(message, Severity.Warning);
    }

    public static void AddWarnings(this ISnackbar snackbar, IEnumerable<string> messages)
    {
        foreach (var message in messages)
        {
            snackbar.Add(message, Severity.Warning);
        }
    }

    public static void AddError(this ISnackbar snackbar, string message)
    {
        snackbar.Add(message, Severity.Error);
    }

    public static void AddErrors(this ISnackbar snackbar, IEnumerable<string> messages)
    {
        foreach (var message in messages)
        {
            snackbar.Add(message, Severity.Error);
        }
    }

    public static PaginatedListRequest ToPaginatedListRequest(this TableState state, string? searchKeyword)
    {
        return new PaginatedListRequest
        {
            Page = state.Page + 1,
            PageSize = state.PageSize,
            SearchText = searchKeyword,
            SortField = state.SortLabel,
            SortOrder = (SortOrder)state.SortDirection
        };
    }

    public static T ToPaginatedListRequest<T>(this TableState state, string? searchKeyword) where T : PaginatedListRequest, new()
    {
        return new()
        {
            Page = state.Page + 1,
            PageSize = state.PageSize,
            SearchText = searchKeyword,
            SortField = state.SortLabel,
            SortOrder = (SortOrder)state.SortDirection
        };
    }
}
