using MudBlazor;
using Smlp.Bsui.Common.Constants;

namespace Smlp.Bsui.Features.SiteAdministrator;

public partial class Index
{
    private List<BreadcrumbItem> _breadcrumbItems = new();
    private string _searchUser = string.Empty;
    private string _filterRole = string.Empty;
    private bool _loadingUsers = false;
    private string _shopName = string.Empty;
    private string _shopAddress = string.Empty;
    private string _shopPhone = string.Empty;
    private string _shopEmail = string.Empty;

    private List<UserRow> _users = new();

    protected override void OnInitialized()
    {
        SetupBreadcrumb();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active("Administrator"),
        };
    }

    private void OpenAddUserDialog()
    {
        // TODO: Open dialog to add user
    }

    public record UserRow(string Username, string FullName, string Email, string Role, bool IsActive);
}
