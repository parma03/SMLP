namespace Smlp.Shared.Common.Extensions;
public static class DocumentEditorExtensions
{
    public static string[] ToolBarItems()
    {
        return new string[] { "Undo", "Redo", "Separator", "Image", "Table", "Hyperlink", "Bookmark", "TableOfContents", "Separator", "Header", "Footer", "PageSetup", "PageNumber", "Break", "InsertFootnote", "InsertEndnote", "Separator", "Find", "Separator", "Comments", "TrackChanges", "Separator", "LocalClipboard", "RestrictEditing", "Separator", "FormFields", "UpdateFields" };
    }
}
