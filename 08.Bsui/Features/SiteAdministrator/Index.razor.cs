using MudBlazor;
using Smlp.Base.User.Enums;
using Smlp.Bsui.Common.Components;
using Smlp.Bsui.Common.Constants;
using Smlp.Bsui.Common.Extensions;
using Smlp.Shared.AppSettings.Commands.UpsertAppSettings;
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
    private bool _loadingSettings = false;

    // Shop info
    private string _shopName = "SMLP Coffee Shop";
    private string _shopAddress = string.Empty;
    private string _shopPhone = string.Empty;
    private string _shopEmail = string.Empty;
    private string _shopLogoUrl = string.Empty;

    // Loyalty settings
    private int _loyaltyPointExpireDays = 365;
    private int _loyaltyMinRedeemPoints = 100;
    private int _loyaltyWelcomeBonus = 0;

    // Receipt
    private string _receiptFooterMessage = "Terima kasih telah berbelanja!";

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
        await Task.WhenAll(LoadUsersAsync(), LoadAppSettingsAsync());
    }

    private void SetupBreadcrumb()
    {
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active("Administrator"),
        };
    }

    // ─── App Settings ────────────────────────────────────────────────────────

    private async Task LoadAppSettingsAsync()
    {
        _loadingSettings = true;
        try
        {
            var result = await _appSettingService.GetAppSettingsAsync();
            if (result.Error is not null)
            {
                _snackbar.AddWarning("Gagal memuat pengaturan aplikasi.");
                return;
            }

            var s = result.Result!;
            _shopName = s.ShopName;
            _shopAddress = s.ShopAddress;
            _shopPhone = s.ShopPhone;
            _shopEmail = s.ShopEmail;
            _shopLogoUrl = s.ShopLogoUrl ?? string.Empty;
            _loyaltyPointExpireDays = s.LoyaltyPointExpireDays;
            _loyaltyMinRedeemPoints = s.LoyaltyMinRedeemPoints;
            _loyaltyWelcomeBonus = s.LoyaltyWelcomeBonus;
            _receiptFooterMessage = s.ReceiptFooterMessage;
        }
        catch (Exception ex)
        {
            _snackbar.AddError($"Gagal memuat pengaturan: {ex.Message}");
        }
        finally
        {
            _loadingSettings = false;
        }
    }

    private async Task SaveAppSettingsAsync()
    {
        _isSavingSettings = true;
        try
        {
            var payload = new UpsertAppSettingsRequest
            {
                ShopName = _shopName,
                ShopAddress = _shopAddress,
                ShopPhone = _shopPhone,
                ShopEmail = _shopEmail,
                ShopLogoUrl = string.IsNullOrWhiteSpace(_shopLogoUrl) ? null : _shopLogoUrl,
                LoyaltyPointExpireDays = _loyaltyPointExpireDays,
                LoyaltyMinRedeemPoints = _loyaltyMinRedeemPoints,
                LoyaltyWelcomeBonus = _loyaltyWelcomeBonus,
                ReceiptFooterMessage = _receiptFooterMessage,
            };

            var result = await _appSettingService.UpsertAppSettingsAsync(payload);
            if (result.Error is not null)
            {
                _snackbar.AddError(result.Error.ToDetail());
                return;
            }

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

    // ─── Users ───────────────────────────────────────────────────────────────

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
        var parameters = new DialogParameters
        {
            [nameof(ConfirmDialog.Title)] = "Hapus Pengguna",
            [nameof(ConfirmDialog.Message)] = $"Hapus pengguna \"{user.DisplayName}\"? Tindakan ini tidak bisa dibatalkan.",
            [nameof(ConfirmDialog.Icon)] = Icons.Material.Filled.Delete,
            [nameof(ConfirmDialog.ConfirmText)] = "Ya, Hapus",
        };
        var dialog = await _dialogService.ShowAsync<ConfirmDialog>("Hapus Pengguna", parameters);
        var dialogResult = await dialog.Result;
        if (dialogResult is null || dialogResult.Canceled)
            return;

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
