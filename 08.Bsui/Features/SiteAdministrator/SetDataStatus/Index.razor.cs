using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Smlp.Bsui.Common.Extensions;
using Smlp.Bsui.Common.Helper;
using Smlp.Client.Services.BackEnd.Masters;
using Smlp.Shared.Common.Responses;
using Smlp.Shared.Masters.Commands.SetDataStatus.Update;
using Syncfusion.Blazor.Calendars;

namespace Smlp.Bsui.Features.SiteAdministrator.SetDataStatus;

public partial class Index
{
    [CascadingParameter]
    protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;
    protected ClaimsPrincipal _user = default!;
    [Inject]
    protected SetDataStatusService SetDataStatusService { get; set; } = default!;
    protected ErrorResponse? _error;
    protected bool _isLoadingDialog = false;
    protected bool _isLoading = false;
    private UpdateSetDataStatusRequest UpdateStatusRequest { get; set; } = new();
    private bool IsApprovalDisabled { get; set; }
    private Guid IdDataStatusConfig { get; set; }
    private DateOnly? MinDisabledTo { get; set; }
    private DateTime? MinDisabledToDateTime => MinDisabledTo?.ToDateTime(TimeOnly.MinValue);
    private const int MinimumDaysDifference = 0;

    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        try
        {
            await GetDataStatusAsync();
            //IsApprovalDisabled = SetDataStatusHelper.CheckApproval(UpdateStatusRequest.DisabledFrom, UpdateStatusRequest.DisabledTo);
            IsApprovalDisabled = SetDataStatusHelper.IsApprovalDisabled(UpdateStatusRequest.DisabledFrom, UpdateStatusRequest.DisabledTo);
        }
        catch (Exception ex)
        {
            //_error = new ErrorResponse { Messages = new[] { ex.Message } };
        }
        finally
        {
            _isLoading = false;
        }
    }

    // private void ResetDisableDate()
    // {
    //     UpdateStatusRequest.DisabledTo = null;
    //     UpdateStatusRequest.DisabledFrom = null;
    //     IsApprovalDisabled = true;
    //     StateHasChanged();
    // }

    private async Task GetDataStatusAsync()
    {
        var response = await SetDataStatusService.GetSetDataStatusAsync();

        if (response.Error is null && response.Result is not null)
        {
            var data = response.Result;

            IdDataStatusConfig = data.Id;
            UpdateStatusRequest = new UpdateSetDataStatusRequest
            {
                //MaxDate = data.MaxDate,
                DisabledFrom = data.DisabledFrom,
                DisabledTo = data.DisabledTo,
                // IsRunJob = data.IsRunJob
            };

            if (UpdateStatusRequest.DisabledFrom.HasValue)
            {
                MinDisabledTo = UpdateStatusRequest.DisabledFrom.Value.AddDays(MinimumDaysDifference);
            }
        }
        else
        {
            _error = response.Error;
        }
    }

    private void OnDisabledFromChange(ChangedEventArgs<DateOnly?> args)
    {
        var newDate = args.Value;
        var previousDisabledTo = UpdateStatusRequest.DisabledTo;

        UpdateStatusRequest.DisabledFrom = newDate;

        if (newDate.HasValue)
        {
            MinDisabledTo = newDate.Value.AddDays(MinimumDaysDifference);

            if (previousDisabledTo.HasValue && newDate.Value > previousDisabledTo.Value)
            {
                UpdateStatusRequest.DisabledTo = null;
                _snackbar.AddWarning($"Disabled To has been cleared because Disabled From is later than the current Disabled To date.");
            }
        }
        else
        {
            MinDisabledTo = null;
            UpdateStatusRequest.DisabledTo = null;
        }

        StateHasChanged();
    }

    private async Task OnValidSubmit()
    {
        _isLoading = true;
        _error = null;
        var today = DateTime.Now;
        // Frontend validation
        if (!ValidateFormSubmit())
        {
            _isLoading = false;
            return;
        }

        try
        {

            UpdateStatusRequest.DisabledFrom = DateOnly.FromDateTime(new DateTime(today.Year, today.Month, UpdateStatusRequest.DisabledFrom.Value.Day));
            UpdateStatusRequest.DisabledTo = DateOnly.FromDateTime(new DateTime(today.Year, today.Month, UpdateStatusRequest.DisabledTo.Value.Day));
            var response = await SetDataStatusService.UpdateSetDataStatusAsync(UpdateStatusRequest, IdDataStatusConfig);

            if (response.Error != null)
            {
                _error = response.Error;
                _snackbar.AddError($"Error: {_error}");
            }
            else
            {
                // if (UpdateStatusRequest.IsRunJob)
                // {
                //     _snackbar.AddSuccess("Success Run Job");
                // }
                // else
                // {
                //     _snackbar.AddSuccess("Success Deactive Job");
                // }
                _snackbar.AddSuccess("Success update data status");
            }
        }
        catch (Exception ex)
        {
            //_error = new ErrorResponse { Messages = new[] { ex.Message } };
        }
        finally
        {
            //IsApprovalDisabled = SetDataStatusHelper.CheckApproval(UpdateStatusRequest.DisabledFrom, UpdateStatusRequest.DisabledTo);
            IsApprovalDisabled = SetDataStatusHelper.IsApprovalDisabled(UpdateStatusRequest.DisabledFrom, UpdateStatusRequest.DisabledTo);
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Frontend validation that disable the date range before submitting the form 
    /// for updating SetDataStatus which ensures both dates are filled together and the range is valid.
    /// </summary>
    /// 

    private bool ValidateFormSubmit()
    {
        if (UpdateStatusRequest.DisabledFrom is null &&
        UpdateStatusRequest.DisabledTo is not null)
        {
            _snackbar.AddError("Disabled From must be filled when Disabled To is filled.");
            return false;
        }

        if (UpdateStatusRequest.DisabledFrom is not null &&
        UpdateStatusRequest.DisabledTo is null)
        {
            _snackbar.AddError("Disabled To must be filled when Disabled From is filled.");
            return false;
        }

        // if (UpdateStatusRequest.DisabledFrom > UpdateStatusRequest.DisabledTo)
        // {
        //     _snackbar.AddError("Disabled From cannot be later than Disabled To.");
        //     return false;
        // }
        //if (UpdateStatusRequest.DisabledFrom.HasValue && UpdateStatusRequest.DisabledTo.HasValue)
        //{
        //// Check if DisabledFrom is later than DisabledTo
        //if (UpdateStatusRequest.DisabledFrom > UpdateStatusRequest.DisabledTo)
        //{
        //    _snackbar.AddError("Disabled From cannot be later than Disabled To.");
        //    return false;
        //}

        // Check minimum days difference
        //var daysDifference = UpdateStatusRequest.DisabledTo.Value.DayNumber - UpdateStatusRequest.DisabledFrom.Value.DayNumber;
        //if (daysDifference < MinimumDaysDifference)
        //{
        //    _snackbar.AddError($"The date range must be at least {MinimumDaysDifference} days. Current difference: {daysDifference} day(s).");
        //    return false;
        //}
        //}

        return true;
    }
}
