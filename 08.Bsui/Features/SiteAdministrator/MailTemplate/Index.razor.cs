using Microsoft.AspNetCore.Components.Web;
using Smlp.Shared.Common.Responses;

namespace Smlp.Bsui.Features.SiteAdministrator.MailTemplate;

public partial class Index
{
    protected ErrorResponse? _error;
    protected bool _isLoadingDialog = false;
    protected bool _isLoading = false;

    protected List<DropdownItemResponse> _dropdownTemplate = new();
    protected int _templateid = 1;

    protected override async Task OnInitializedAsync()
    {
        await InitDropdownValidationCriteria();
    }

    public async Task InitDropdownValidationCriteria()
    {
        _dropdownTemplate = new()
        {
            new DropdownItemResponse {Id = 1, Description = "Pengumuman Seleksi LOLOS Administrasi >> TOEFL" },
            new DropdownItemResponse {Id = 2, Description = "Pengumuman Seleksi LOLOS Administrasi >> PSIKOTES" },
            new DropdownItemResponse {Id = 2, Description = "Pengumuman Seleksi LOLOS Administrasi >> TRM" },
            new DropdownItemResponse {Id = 2, Description = "Pengumuman Seleksi TIDAK LOLOS Administrasi" },
            new DropdownItemResponse {Id = 2, Description = "Pengumuman Seleksi >> LULUS TOEFL" },
            new DropdownItemResponse {Id = 2, Description = "Pengumuman Seleksi  >> TIDAK LULUS TOEFL" },
            new DropdownItemResponse {Id = 2, Description = "Pengumuman Seleksi >> LULUS PSIKOTES" },
            new DropdownItemResponse {Id = 2, Description = "Pengumuman Seleksi >> TIDAK LULUS PSIKOTES" },
            new DropdownItemResponse {Id = 2, Description = "Pengumuman Hasil >> LULUS TRM" },
            new DropdownItemResponse {Id = 2, Description = "Pengumuman Hasil >> TIDAK LULUS TRM" },
        };
    }

    public async Task OnChangeTemplate(Syncfusion.Blazor.DropDowns.ChangeEventArgs<int, DropdownItemResponse> args)
    {
        if (args is null)
        {
            throw new ArgumentNullException(nameof(args));
        }

        _templateid = (int)args.ItemData.Id;
        StateHasChanged();
    }

    private async Task OnClickSave(MouseEventArgs e)
    {
        if (!ValidateOnSave())
        {
            return;
        }
    }
    private bool ValidateOnSave()
    {
        return true;
    }
}
