using Microsoft.AspNetCore.Components;

namespace Psmp.Bsui.Features.Payments.Component;

public partial class DialogConfirmation
{
    [Parameter]
    public bool Visibility { get; set; }

    [Parameter]
    public string Title { get; set; } = default!;

    [Parameter]
    public string Content { get; set; } = default!;

    [Parameter]
    public EventCallback DialogOpen { get; set; }

    [Parameter]
    public EventCallback DialogClose { get; set; }

    [Parameter]
    public EventCallback OnClickNo { get; set; }

    [Parameter]
    public EventCallback OnClickYes { get; set; }

    [Parameter]
    public bool IsProgressButton { get; set; } = false;

    [Parameter]
    public int Duration { get; set; } = 300;

    [Parameter]
    public string PositionX { get; set; } = "center";

    [Parameter]
    public string PositionY { get; set; } = "center";

    [Parameter]
    public bool ShowNoButton { get; set; } = true;

    private record DialogMeta(
        string IconCss,
        string IconCircleCss,
        string IconColor,
        string ButtonCss,
        string ButtonContent);

    private static readonly IReadOnlyDictionary<string, DialogMeta> _dialogMetaMap = new Dictionary<string, DialogMeta>
    {
        {
            "Simulate Data",
            new DialogMeta(
                IconCss: "e-icons e-refresh",
                IconCircleCss: "dialog-icon-circle simulate",
                IconColor: "#7E22CE",
                ButtonCss: "e-simulate",
                ButtonContent: "Simulate"
            )
        },
        {
            "Submit To SAP",
            new DialogMeta(
                IconCss: "e-icons e-check",
                IconCircleCss: "dialog-icon-circle submit-sap",
                IconColor: "#1E4EA2",
                ButtonCss: "e-submit-sap",
                ButtonContent: "Submit"
            )
        },
        {
            "Simulate Data Error !",
            new DialogMeta(
                IconCss: "e-icons e-warning",
                IconCircleCss: "dialog-icon-circle simulate-data-error",
                IconColor: "#CA8A04",
                ButtonCss: "e-submit-sap",
                ButtonContent: "Continue"
            )
        },
        {
            "Simulate Data Success !",
            new DialogMeta(
                IconCss: "e-icons e-check",
                IconCircleCss: "dialog-icon-circle simulate-data-success",
                IconColor: "#16A34A",
                ButtonCss: "e-submit-sap",
                ButtonContent: "Continue"
            )
        },
        {
            "Success",
            new DialogMeta(
                IconCss: "e-icons e-check",
                IconCircleCss: "dialog-icon-circle success",
                IconColor: "#16A34A",
                ButtonCss: "e-submit-sap",
                ButtonContent: "Continue"
        )
    }
    };

    private DialogMeta GetDialogMeta(string title)
    {
        if (_dialogMetaMap.TryGetValue(title, out var meta))
        {
            return meta;
        }

        return new DialogMeta(
            IconCss: "e-icons e-circle-check",
            IconCircleCss: "dialog-icon-circle",
            IconColor: "#4CAF50",
            ButtonCss: "e-success",
            ButtonContent: "Yes"
        );
    }
}
