using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor;
using Smlp.Bsui.Common.Extensions;
using Smlp.Bsui.Services.Authentication;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Services.Authorization.Models.GetPositions;

namespace Smlp.Bsui.Services.Authorization.Components;

public partial class DialogSwitchPosition
{
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    [CascadingParameter]
    private Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

    private ErrorResponse? _error;
    private bool _isLoading;
    private List<GetPositionsPosition> _positions = default!;
    private GetPositionsPosition _position = default!;

    protected override async Task OnInitializedAsync()
    {
        var user = (await AuthenticationStateTask).User;
        var username = user.GetUsername();

        if (!string.IsNullOrWhiteSpace(username))
        {
            try
            {
                _isLoading = true;

                var response = await _authorizationService.GetPositionsAsync(username, _userInfo.AccessToken);

                _isLoading = false;

                _positions = response.Positions.ToList();
            }
            catch (Exception exception)
            {
                _isLoading = false;

                _error = new CommonErrorResponse
                {
                    Exception = exception,
                    Detail = exception.Message
                };

                return;
            }

            if (_positions.Count == 1)
            {
                _position = _positions.Single();
            }
            else if (_positions.Count > 1)
            {
                var positionId = user.GetPositionId();

                if (!string.IsNullOrEmpty(positionId))
                {
                    var position = _positions.Where(x => x.Id == positionId).SingleOrDefault();

                    if (position is not null)
                    {
                        _position = position;
                    }
                }

                if (_position is null)
                {
                    _position = _positions.First();
                }
            }
        }
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private async Task Confirm()
    {
        await ((AuthorizedAuthenticationStateProvider)_authenticationStateProvider).LoadAuthorizationAsync(_position.Id, _position.Name);

        MudDialog.Close(DialogResult.Ok(true));
    }
}
