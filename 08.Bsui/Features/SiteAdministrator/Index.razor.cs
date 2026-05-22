using MudBlazor;
using Smlp.Base.User.Enums;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.Master.Users.Commands.Create;
using Smlp.Shared.Master.Users.Commands.Update;
using Smlp.Shared.Master.Users.Queries;

namespace Smlp.Bsui.Features.SiteAdministrator;

public partial class Index
{
    private List<BreadcrumbItem> _breadcrumbItems = new();
    private string _searchUser = string.Empty;
    private string _filterRole = string.Empty;
    private bool _loadingUsers = false;
    private string _shopName = "Kopi Nusantara";
    private string _shopAddress = string.Empty;
    private string _shopPhone = string.Empty;
    private string _shopEmail = string.Empty;
    private bool _isSavingSettings = false;

    private readonly List<RolePermissionRow> _rolePermissions = new()
    {
        new("Dashboard & Analytics", true, true, false),
        new("Kelola Pelanggan (CRUD)", true, true, true),
        new("Buat Transaksi", true, true, true),
        new("Batalkan Transaksi", true, true, false),
        new("Laporan & Export", true, true, false),
        new("Aturan Poin", true, true, false),
        new("Aturan Tier", true, true, false),
        new("Kelola Reward", true, true, false),
        new("Redeem Reward", true, true, true),
        new("Penyesuaian Poin", true, false, false),
        new("Customer Insights", true, true, false),
        new("Audit Log", true, false, false),
        new("Kelola Pengguna", true, false, false),
        new("Site Administrator", true, false, false),
    };

    private List<GetUserResponse> _users = new();

    // Dialog state
    private bool _showUserDialog = false;
    private bool _isEditMode = false;
    private bool _isSavingUser = false;
    private Guid? _editingUserId = null;

    // Form model
    private UserFormModel _userForm = new();

    protected override async Task OnInitializedAsync()
    {
        SetupBreadcrumb();
        await LoadUsersAsync();
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active("Administrator"),
        };
    }

    private async Task LoadUsersAsync()
    {
        _loadingUsers = true;
        try
        {
            var request = new GetUserRequest
            {
                Search = string.IsNullOrWhiteSpace(_searchUser) ? null : _searchUser,
                PageSize = 100
            };

            var result = await _usersService.GetUsersAsync(request);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

            var all = result.Result!.Items.ToList();

            if (!string.IsNullOrWhiteSpace(_filterRole) && Enum.TryParse<UserRole>(_filterRole, out var roleEnum))
                all = all.Where(u => u.Role == roleEnum).ToList();

            _users = all;
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal memuat data pengguna: {ex.Message}");
        }
        finally
        {
            _loadingUsers = false;
        }
    }

    private void OpenAddUserDialog()
    {
        _isEditMode = false;
        _editingUserId = null;
        _userForm = new UserFormModel { IsActive = true };
        _showUserDialog = true;
    }

    private async Task OpenEditUserDialog(GetUserResponse user)
    {
        _isEditMode = true;
        _editingUserId = user.Id;
        _userForm = new UserFormModel
        {
            Username = user.Username,
            DisplayName = user.DisplayName,
            Role = user.Role,
            PhoneNumber = user.PhoneNumber,
            EmailAddress = user.EmailAddress,
            IsActive = user.IsActive
        };
        _showUserDialog = true;
        await Task.CompletedTask;
    }

    private void CloseUserDialog()
    {
        _showUserDialog = false;
        _userForm = new();
    }

    private async Task SaveUserAsync()
    {
        if (string.IsNullOrWhiteSpace(_userForm.Username) || string.IsNullOrWhiteSpace(_userForm.DisplayName))
        {
            _snackbar.AddWarning("Username dan nama lengkap wajib diisi.");
            return;
        }

        _isSavingUser = true;
        try
        {
            if (_isEditMode && _editingUserId.HasValue)
            {
                var updateReq = new UpdateUserRequest
                {
                    Username = _userForm.Username,
                    DisplayName = _userForm.DisplayName,
                    Role = _userForm.Role,
                    PhoneNumber = _userForm.PhoneNumber ?? string.Empty,
                    EmailAddress = _userForm.EmailAddress ?? string.Empty,
                    IsActive = _userForm.IsActive
                };
                var result = await _usersService.UpdateUserAsync(_editingUserId.Value, updateReq);
                if (result.Error is not null)
                {
                    _snackbar.AddError(result.Error.ToDetail());
                    return;
                }

                _snackbar.AddSuccess($"Pengguna {_userForm.DisplayName} berhasil diperbarui.");
            }
            else
            {
                var createReq = new CreateUserRequest
                {
                    Username = _userForm.Username,
                    DisplayName = _userForm.DisplayName,
                    Role = _userForm.Role,
                    PhoneNumber = _userForm.PhoneNumber ?? string.Empty,
                    EmailAddress = _userForm.EmailAddress ?? string.Empty,
                    IsActive = _userForm.IsActive
                };
                var result = await _usersService.CreateUserAsync(createReq);
                if (result.Error is not null)
                {
                    _snackbar.AddError(result.Error.ToDetail());
                    return;
                }

                _snackbar.AddSuccess($"Pengguna {_userForm.DisplayName} berhasil ditambahkan.");
            }

            CloseUserDialog();
            await LoadUsersAsync();
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal menyimpan: {ex.Message}");
        }
        finally
        {
            _isSavingUser = false;
        }
    }

    private async Task DeleteUserAsync(GetUserResponse user)
    {
        var result = await _usersService.DeleteUserAsync(user.Id);
        if (result.Error is not null)
        {
            _snackbar.AddError(result.Error.ToDetail());
            return;
        }

        _snackbar.AddSuccess($"Pengguna {user.DisplayName} berhasil dihapus.");
        await LoadUsersAsync();
    }

    private async Task OnSearchChanged(string value)
    {
        _searchUser = value;
        await LoadUsersAsync();
    }

    private async Task OnFilterRoleChanged(string value)
    {
        _filterRole = value;
        await LoadUsersAsync();
    }

    private string GetRoleLabel(UserRole role) => role switch
    {
        UserRole.Admin => "Admin",
        UserRole.Owner => "Owner",
        UserRole.Kasir => "Kasir",
        UserRole.Pelanggan => "Pelanggan",
        _ => role.ToString()
    };

    private Color GetRoleColor(UserRole role) => role switch
    {
        UserRole.Admin => Color.Error,
        UserRole.Owner => Color.Warning,
        UserRole.Kasir => Color.Info,
        _ => Color.Default
    };

    private async Task SaveAppSettingsAsync()
    {
        _isSavingSettings = true;
        try
        {
            // TODO: Save to database via API when AppSettings endpoint is available
            await Task.Delay(500); // Simulate save
            _snackbar.AddSuccess("Pengaturan aplikasi berhasil disimpan.");
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal menyimpan pengaturan: {ex.Message}");
        }
        finally
        {
            _isSavingSettings = false;
        }
    }

    public record RolePermissionRow(string Feature, bool Admin, bool Owner, bool Kasir);

    public class UserFormModel
    {
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.Kasir;
        public string? PhoneNumber { get; set; }
        public string? EmailAddress { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
