using System.Data;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using MudBlazor;
using Psmp.Bsui.Common.Constants;
using Psmp.Bsui.Common.Extensions;
using Psmp.Bsui.Common.Helper;
using Psmp.Bsui.Features.Payments.Constants;
using Psmp.Shared.Common.Constants;
using Psmp.Shared.Common.Extensions;
using Psmp.Shared.Payments.Commands;
using Psmp.Shared.Payments.Queries;
using Psmp.Shared.Payments.Queries.PaymentOverviewDetails;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Inputs;
using Syncfusion.XlsIO;

namespace Psmp.Bsui.Features.Payments.Overview;

public partial class Detail
{
    #region Parameters
    [Parameter]
    public string? RequestId { get; set; }
    #endregion

    #region Data
    protected List<GetPaymentOverviewDetailPeriodResponse> _paymentOverviewDetails = new();
    protected List<GetPaymentOverviewResponse> _paymentOverview = new();
    protected DataTable _table = new();
    protected List<PaymentOverviewStatusRequest> _workbookApplicants = new();
    protected (DateOnly? from, DateOnly? to) _approvalWindow;
    protected bool IsRequestIdDisabled { get; set; } = false;
    #endregion

    #region UI Component
    private SfGrid<GetPaymentOverviewDetailPeriodResponse> Grid { get; set; } = new();
    protected SfUploader? _uploadObj;
    private List<BreadcrumbItem> _breadcrumbItems = new();
    #endregion

    #region UI State
    protected bool _isLoading = false;
    private bool _isSimulating = false;
    private bool VisiblePopup { get; set; }
    private bool _visibleConfirmationDialog = false;
    private bool _showNoButton = true;
    private bool _isProgressButton = false;
    //edit dialog
    private bool _visibleEditDialog = false;
    private bool _isSavingStatusToSap = false;
    private GetPaymentOverviewDetailPeriodResponse? _selectedItemForEdit;
    #endregion

    #region Business State
    private bool _isRequestIdGenerated = false;
    private bool _isSimulationSuccess = false;
    private bool _isCompletedToSap = false;
    private int _currentStage = 0;
    private PaymentOverviewStatusRequest _requestResponseSap = new();
    #endregion

    #region Display Data
    protected string _displayName = default!;
    protected string _requestId = default!;
    protected string? _university = string.Empty;
    protected long _maximumFileSizeInBytes = long.MaxValue;
    private string TitlePopup { get; set; } = "Request Overview";
    private string _monthName = string.Empty;
    private string _dialogTitle = string.Empty;
    private string _dialogContent = string.Empty;
    #endregion

    #region Computed Properties
    // private bool CanSimulate => _isRequestIdGenerated;
    private bool CanSimulate
    {
        get
        {
            if (!_isRequestIdGenerated)
            {
                return false;
            }

            if (_paymentOverviewDetails == null || !_paymentOverviewDetails.Any())
            {
                return false;
            }

            var allSuccess = _paymentOverviewDetails.All(x =>
                !string.IsNullOrEmpty(x.TypeResponse) &&
                x.TypeResponse.Equals("s", StringComparison.OrdinalIgnoreCase));

            return !allSuccess;
        }
    }
    // private bool CanRepost => _isCompletedToSap;
    private bool CanRepost
    {
        get
        {
            // if (!_isCompletedToSap)
            // {
            //     return false;
            // }

            if (_paymentOverviewDetails == null || !_paymentOverviewDetails.Any())
            {
                return false;
            }

            var hasNonSuccessItems = _paymentOverviewDetails.Any(x =>
            string.IsNullOrEmpty(x.TypeResponse) ||
            !x.TypeResponse.Equals("s", StringComparison.OrdinalIgnoreCase));

            var allIsSimulate = _paymentOverviewDetails.All(x => x.IsSimulate);

            return hasNonSuccessItems && allIsSimulate;
        }
    }
    private bool CanPostToSap => _isSimulationSuccess && !_isCompletedToSap;
    private bool CanComplete
    {
        get
        {
            if (_paymentOverviewDetails == null || !_paymentOverviewDetails.Any())
            {
                return false;
            }

            var allStatusIsCompleted = _paymentOverviewDetails.All(x => x.PaymentStatus == Base.Payments.Enums.PaymentStatus.Completed);
            if (allStatusIsCompleted)
            {
                return false;
            }

            var allTypeResponseSuccess = _paymentOverviewDetails.All(x =>
            !string.IsNullOrEmpty(x.TypeResponse) &&
            x.TypeResponse.Equals("s", StringComparison.OrdinalIgnoreCase));

            return allTypeResponseSuccess;
        }
    }
    private bool IsRequestIdInputDisabled
    {
        get
        {
            if (IsRequestIdDisabled)
            {
                return true;
            }

            if (_paymentOverviewDetails == null || !_paymentOverviewDetails.Any())
            {
                return false;
            }

            var allStatusIsCompleted = _paymentOverviewDetails.All(x =>
                x.PaymentStatus == Base.Payments.Enums.PaymentStatus.Completed);

            return allStatusIsCompleted;
        }
    }
    #endregion

    #region Lifecycle Method
    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        await SetDataOnLoadAsync();
        await GetDataStatusConfiguration();
        _isLoading = false;
        SetupBreadcrumb();
    }
    #endregion

    #region Initialization Methods
    private void SetupBreadcrumb()
    {
        try
        {
            var uri = new Uri(_navigationManager.Uri);
            var query = QueryHelpers.ParseQuery(uri.Query);
            if (query.TryGetValue("month", out var monthValues))
            {
                _monthName = monthValues.FirstOrDefault() ?? string.Empty;
            }
        }
        catch
        {
            _monthName = string.Empty;
        }

        var title = string.IsNullOrWhiteSpace(_monthName) ? "Request" : $"Request {_monthName}";
        _breadcrumbItems = new()
        {
            CommonBreadcrumbFor.Active(title)
        };
    }

    private async Task SetDataOnLoadAsync()
    {
        _isLoading = true;
        await Task.Delay(2000);
        var req = new GetPaymentOverviewDetailPeriodRequest()
        {
            RequestId = RequestId
        };
        var response = await _paymentService.GetPaymentOverviewDetailPeriodAsync(req);
        if (response.Error is null)
        {
            _paymentOverviewDetails = response.Result.Items.ToList();
            _requestId = _paymentOverviewDetails!.FirstOrDefault()!.RequestId!;

            if (!string.IsNullOrEmpty(_requestId) && _requestId != "-")
            {
                _isRequestIdGenerated = true;
            }
        }
        else
        {
            _isLoading = false;
        }

        _isLoading = false;
    }

    private async Task GetDataStatusConfiguration()
    {
        var response = await _setDataStatusService.GetSetDataStatusAsync();
        if (response.Error is null && response.Result is not null)
        {
            var config = response.Result;
            IsRequestIdDisabled = SetDataStatusHelper.IsRequestIdDisabled(config.DisabledFrom, config.DisabledTo);
            _approvalWindow.from = config.DisabledFrom;
            _approvalWindow.to = config.DisabledTo;
        }
    }
    #endregion

    public async Task OnCommandClicked(CommandClickEventArgs<GetPaymentOverviewDetailPeriodResponse> args)
    {
        if (args.CommandColumn.Type == CommandButtonType.None)
        {
            _navigationManager.NavigateTo(CommonRouteFor.Payment + "/" + args.RowData.ApplicantId, true);
        }
    }
    private async Task ValueChangeUploaderApplicants(UploadChangeEventArgs args)
    {
        foreach (var file in args.Files)
        {
            var excelEngine = new ExcelEngine();
            var application = excelEngine.Excel;
            application.DefaultVersion = ExcelVersion.Excel2016;

            var memoryStream = new MemoryStream();
            await args.Files[0].File.OpenReadStream(long.MaxValue).CopyToAsync(memoryStream);
            memoryStream.Position = 0; // Reset the stream position

            var workbook = application.Workbooks.Open(memoryStream);

            //get the first worksheet in the workbook into IWorksheet
            var worksheet = workbook.Worksheets[0];

            _table = worksheet.ExportDataTable(worksheet.UsedRange, ExcelExportDataTableOptions.ColumnNames);

            //convert to list
            try
            {
                GenerateListFromTable(_table);
            }
            catch (Exception ex)
            {
                _snackbar.AddError(ex.Message);
            }
        }
    }

    protected void GenerateListFromTable(DataTable table)
    {
        if (table is not null)
        {
            foreach (DataRow row in table.Rows)
            {
                _workbookApplicants.Add(
                    new PaymentOverviewStatusRequest
                    {
                        EmployeeNumber = (string?)row["Nopek"],
                        PaymentDate = DateOnly.FromDateTime(
                        DateTime.ParseExact((string?)row["Date Of Origin"],
                                         "dd/MM/yyyy",
                                         CultureInfo.InvariantCulture)),

                        Currancy = GetStatusForUpload((string?)row["Currency"]),
                        RequestId = (string?)row["Request ID"],
                        PaymentStatus = (string?)row["IsComplited"] == "TRUE" ? 13 : 12,
                    });
            }
        }
    }
    private int GetStatusForUpload(string status)
    {
        //var result = 0;
        //if (status == "IDR")
        //{
        //    result = 1;
        //}
        //else if (status == "USD")
        //{
        //    result = 2;
        //}

        //return result;
        return status switch
        {
            "IDR" => 1,
            "USD" => 2,
            _ => 0
        };
    }
    //protected void OnRemoveUploaderApplicants(RemovingEventArgs args)
    //{
    //}

    //generate request id
    private async Task OnClickUploadAsync(MouseEventArgs e)
    {
        //foreach (var item in _workbookApplicants)
        //{
        //    item.Stage = 1;
        //    var response = await _paymentService.UpdateOverviewStatusAsync(item);
        //    if (response.Error is not null)
        //    {
        //        _snackbar.AddError(response.Error.Detail);
        //        VisiblePopup = false;
        //        return;
        //    }
        //}

        if (_requestId == "-")
        {
            _snackbar.Add("Request id is required", Severity.Error);
            return;
        }

        var item = new PaymentOverviewStatusRequest
        {
            RequestId = _requestId
        };
        var response = await _paymentService.UpdateOverviewStatusAsync(item);
        if (response.Error is not null)
        {
            _snackbar.AddError(response.Error.Detail);
            VisiblePopup = false;
            return;
        }

        _navigationManager.NavigateTo(RouteFor.PaymentOverviewDetail(_requestId));
        _snackbar.AddSuccess($"Success Update Payment Status");
        _isRequestIdGenerated = true;
        VisiblePopup = false;
        StateHasChanged();
    }

    private async Task OnClickPostingSap(int stage)
    {
        _currentStage = stage;

        if (stage == 1)
        {
            _dialogTitle = "Simulate Data";
            _dialogContent = "Are you sure you want to simulate this data?";
            _showNoButton = true;
            _isProgressButton = true;
            _visibleConfirmationDialog = true;
        }
        else if (stage == 2)
        {
            _dialogTitle = "Submit To SAP";
            _dialogContent = "Are you sure you want to Submit to SAP?";
            _showNoButton = true;
            _isProgressButton = true;
            _visibleConfirmationDialog = true;
        }
        else if (stage == 3)
        {
            _dialogTitle = "Reposting To SAP";
            _dialogContent = "Are you sure you want to Reposting to SAP?";
            _showNoButton = true;
            _isProgressButton = true;
            _visibleConfirmationDialog = true;
            //await ExecutePostingSap(stage);
        }
        else if (stage == 5)
        {
            _dialogTitle = "Complited To SAP";
            _dialogContent = "Are you sure you want to Complited to SAP?";
            _showNoButton = true;
            _isProgressButton = true;
            _visibleConfirmationDialog = true;
            //await ExecutePostingSap(stage);
        }

        StateHasChanged();

        //var item = new PaymentOverviewStatusRequest
        //{
        //    RequestId = _requestId,
        //    Stage = stage,
        //};
        //var response = await _paymentService.UpdateOverviewStatusAsync(item);
        //if (response.Error is not null)
        //{
        //    _snackbar.AddError(response.Error.Detail);
        //    VisiblePopup = false;
        //    return;
        //}
    }

    private async Task ExecutePostingSap(int stage)
    {
        var item = new PaymentOverviewStatusRequest
        {
            RequestId = _requestId,
            Stage = stage,
        };
        var response = await _paymentService.UpdateOverviewStatusAsync(item);

        if (response.Error is not null)
        {
            _isSimulating = false;
            _dialogTitle = stage == 1 ? "Simulate Data Error !" : "Error";
            _dialogContent = stage == 1
                ? "Simulated data failed. Please fix the data marked as failed."
                : response.Error.Detail;
            _showNoButton = false;
            _isProgressButton = false;
            _visibleConfirmationDialog = true;

            StateHasChanged();
            return;
        }

        await SetDataOnLoadAsync();

        if (stage == 1)
        {
            var hasFailedData = _paymentOverviewDetails.Any(x =>
                !string.IsNullOrEmpty(x.TypeResponse) &&
                x.TypeResponse.Equals("e", StringComparison.OrdinalIgnoreCase));

            if (hasFailedData)
            {
                _isSimulating = false;
                _isSimulationSuccess = true;

                _dialogTitle = "Simulate Data Error !";
                _dialogContent = "Simulated data failed. Please fix the data marked as failed.";
                _showNoButton = false;
                _isProgressButton = false;
                _visibleConfirmationDialog = true;

                StateHasChanged();
                return;
            }

            _isSimulationSuccess = true;
        }
        else if (stage == 2)
        {
            _isCompletedToSap = true;
        }
        else if (stage == 3)
        {
            await SetDataOnLoadAsync();

            var allSuccess = _paymentOverviewDetails.All(x =>
                !string.IsNullOrEmpty(x.TypeResponse) &&
                x.TypeResponse.Equals("s", StringComparison.OrdinalIgnoreCase));

            if (!allSuccess)
            {
                _dialogTitle = "RePosting Completed";
                _dialogContent = "RePosting completed. Please check the failed items.";
            }
        }

        var message = stage switch
        {
            1 => "Simulation completed successfully",
            2 => "Data submitted to SAP successfully",
            3 => "RePosting completed successfully",
            _ => "Operation completed successfully"
        };

        _dialogTitle = stage == 1 ? "Simulate Data Success !" : "Success";
        _dialogContent = message;
        _showNoButton = false;
        _isProgressButton = false;
        _visibleConfirmationDialog = true;

        await SetDataOnLoadAsync();

        _isSimulating = false;
        StateHasChanged();
    }

    private async Task OnClickDownloadForBMCAsync(MouseEventArgs e)
    {
        using var excelEngine = new ExcelEngine();
        var application = excelEngine.Excel;
        application.DefaultVersion = ExcelVersion.Xlsx;

        //Create a workbook
        var workbook = application.Workbooks.Create(1);
        var worksheet = workbook.Worksheets[0];
        var headingStyle = workbook.Styles.Add("HeadingStyle");
        headingStyle.Font.Bold = true;
        worksheet.Range["A1"].ColumnWidth = 30;
        worksheet.Range["B1"].ColumnWidth = 30;
        worksheet.Range["C1"].ColumnWidth = 20;
        worksheet.Range["D1"].ColumnWidth = 40;
        worksheet.Range["D1"].WrapText = true;
        worksheet.Range["E1"].ColumnWidth = 20;
        worksheet.Range["F1"].ColumnWidth = 20;
        worksheet.Range["G1"].ColumnWidth = 20;
        worksheet.Range["H1"].ColumnWidth = 20;
        worksheet.Range["H1"].WrapText = true;
        worksheet.Range["I1"].ColumnWidth = 20;
        worksheet.Range["J1"].ColumnWidth = 30;
        worksheet.Range["J1"].WrapText = true;

        worksheet.Range["A1"].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        worksheet.Range["A1"].Text = "IT 0015 Additional Payment";
        worksheet.Range["A1"].CellStyle.Font.Bold = true;
        worksheet.Range["A1"].CellStyle.Font.Size = 14;
        worksheet.Range["A1:J1"].Merge();

        worksheet.Range["A2"].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        worksheet.Range["A2"].Text = "Personal Data";
        worksheet.Range["A2"].CellStyle.Font.Bold = true;
        worksheet.Range["A2:B2"].Merge();

        worksheet.Range["C2"].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        worksheet.Range["C2"].Text = "Create Additional Payment";
        worksheet.Range["C2"].CellStyle.Font.Bold = true;
        worksheet.Range["C2:J2"].Merge();

        worksheet.Range["D3"].HorizontalAlignment = ExcelHAlign.HAlignCenter;
        worksheet.Range["D3"].Text = "Wage Type";
        worksheet.Range["D3"].CellStyle.Font.Bold = true;
        worksheet.Range["D3:E3"].Merge();

        worksheet.Range["A3:A4"].Merge();
        worksheet.Range["B3:B4"].Merge();
        worksheet.Range["C3:C4"].Merge();
        worksheet.Range["F3:F4"].Merge();
        worksheet.Range["G3:G4"].Merge();
        worksheet.Range["H3:H4"].Merge();
        worksheet.Range["I3:I4"].Merge();
        worksheet.Range["J3:J4"].Merge();

        worksheet.Range["A3"].Text = "Nopek";
        worksheet.Range["A3"].CellStyle.Font.Bold = true;
        worksheet.Range["A3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
        worksheet.Range["A3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
        worksheet.Range["B3"].Text = "Nama Pekerja";
        worksheet.Range["B3"].CellStyle.Font.Bold = true;
        worksheet.Range["B3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
        worksheet.Range["B3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
        worksheet.Range["C3"].Text = "Date Of Origin";
        worksheet.Range["C3"].CellStyle.Font.Bold = true;
        worksheet.Range["C3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
        worksheet.Range["C3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
        worksheet.Range["D4"].Text = "Net";
        worksheet.Range["D4"].CellStyle.Font.Bold = true;
        worksheet.Range["E4"].Text = "Gross";
        worksheet.Range["E4"].CellStyle.Font.Bold = true;
        worksheet.Range["F3"].Text = "Currency";
        worksheet.Range["F3"].CellStyle.Font.Bold = true;
        worksheet.Range["F3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
        worksheet.Range["F3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
        worksheet.Range["G3"].Text = "Amount";
        worksheet.Range["G3"].CellStyle.Font.Bold = true;
        worksheet.Range["G3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
        worksheet.Range["G3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
        worksheet.Range["H3"].Text = "Assignment Number";
        worksheet.Range["H3"].CellStyle.Font.Bold = true;
        worksheet.Range["H3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
        worksheet.Range["H3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
        worksheet.Range["I3"].Text = "Cost Center";
        worksheet.Range["I3"].CellStyle.Font.Bold = true;
        worksheet.Range["I3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
        worksheet.Range["I3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;
        worksheet.Range["J3"].Text = "Keterangan";
        worksheet.Range["J3"].CellStyle.Font.Bold = true;
        worksheet.Range["J3"].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignCenter;
        worksheet.Range["J3"].CellStyle.VerticalAlignment = ExcelVAlign.VAlignCenter;

        //var req = new GetPaymentOverviewRequest()
        //{

        //};
        //var response = await _paymentService.GetPaymentOverviewAsync(req);
        //if (response.Error is null)
        //{
        //    _paymentOverview = response.Result.Items.ToList();
        //}
        //else
        //{
        //    _isLoading = false;
        //}

        var table = DataTableExport();

        //import data from datatable
        worksheet.ImportDataTable(table, true, 4, 1, true);

        //save document as a stream and retrun the stream.
        var downloadStream = new MemoryStream();
        workbook.SaveAs(downloadStream, ExcelSaveType.SaveAsXLS);
        workbook.Close();

        var content = downloadStream.ToArray();
        //download document
        await _jsRuntime.InvokeVoidAsync
            (JavaScriptIdentifierFor.DownloadFile,
            $"PaymentFormRequesttoBmc_{DateTime.Now:yyyyMMdd}.xlsx",
            $"{ContentTypes.ApplicationOpenXmlExcel}",
            content);

        _snackbar.AddSuccess($"Success Update Payment Status");
        VisiblePopup = false;
        StateHasChanged();
    }

    private DataTable DataTableExport()
    {
        var table = new DataTable();

        table.Columns.Add("Nopek", typeof(string));
        table.Columns.Add("Nama Pekerja", typeof(string));
        table.Columns.Add("Date Of Origin", typeof(string));
        table.Columns.Add("Net", typeof(string));
        table.Columns.Add("Gross", typeof(string));
        table.Columns.Add("Currency", typeof(string));
        table.Columns.Add("Amount", typeof(string));
        table.Columns.Add("Assignment Number", typeof(string));
        table.Columns.Add("Cost Center", typeof(string));
        table.Columns.Add("Keterangan", typeof(string));

        if (_paymentOverviewDetails is not null)
        {
            var dataPaymentOverview = _paymentOverviewDetails.Where(x => x.PaymentStatus == Base.Payments.Enums.PaymentStatus.RequestPreparation).ToList();

            foreach (var item in dataPaymentOverview)
            {
                table.Rows.Add(
                    item.ApplicantNumber,
                    item.ApplicantName,
                    item.PaymentDate?.ToString("dd/MM/yyyy"),
                    item.WageType,
                    "",
                    item.Currency.GetDescription(),
                    item.Ammount,
                    item.AssigmentNumber,
                    item.CostCenter,
                    item.Notes);
            }
        }

        return table;
    }

    public async Task OnClickBack()
    {
        _navigationManager.NavigateTo(Psmp.Bsui.Features.Payments.Constants.RouteFor.Payment, true);
        //await Task.CompletedTask;
    }

    private async Task OnConfirmationYes()
    {
        if (_showNoButton)
        {
            //action
            _visibleConfirmationDialog = false;

            StateHasChanged();

            await Task.Delay(100);

            _isSimulating = true;
            StateHasChanged();
            await Task.Delay(100);

            await ExecutePostingSap(_currentStage);
        }
        else
        {
            //(success/error)
            _visibleConfirmationDialog = false;
            StateHasChanged();
        }
    }

    private void OnConfirmationClose()
    {
        _visibleConfirmationDialog = false;
        StateHasChanged();
    }

    private void OnClickEditStatusToSap(GetPaymentOverviewDetailPeriodResponse item)
    {
        _selectedItemForEdit = item;
        _visibleEditDialog = true;
        // _requestResponseSap.OverviewId = item.Id;
        // _requestResponseSap.Stage = 4;
        _requestResponseSap = new PaymentOverviewStatusRequest
        {
            OverviewId = item.Id,
            Stage = 4,
            MessageResponse = string.Empty
        };
        StateHasChanged();
    }
    private void OnCloseEditDialog()
    {
        _visibleEditDialog = false;
        _selectedItemForEdit = null;
        _requestResponseSap = new PaymentOverviewStatusRequest();
        StateHasChanged();
    }
    private async Task OnSaveStatusToSap()
    {
        _isSavingStatusToSap = true;
        StateHasChanged();

        // TODO: logikanya save disini harusnya
        await Task.Delay(1000);

        var response = await _paymentService.UpdateOverviewStatusAsync(_requestResponseSap);

        _isSavingStatusToSap = false;
        _visibleEditDialog = false;

        _snackbar.Add("Status SAP updated successfully", Severity.Success);
        await SetDataOnLoadAsync();
        StateHasChanged();
    }

    private void OnClickView(GetPaymentOverviewDetailPeriodResponse item)
    {
        if (item != null)
        {
            _navigationManager.NavigateTo(CommonRouteFor.Payment + "/" + item.ApplicantId, true);
        }
    }
}
