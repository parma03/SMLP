namespace Smlp.Shared.Documents.Constants;

public static class ApiEndpoint
{
    public static class V1
    {
        public static class Documents
        {
            public const string Segment = $"{nameof(V1)}/{nameof(Documents)}";

            public static class RouteTemplateFor
            {
                public const string DocumentId = "DocumentId/{documentId:guid}";
                public const string Download = "Download/{documentId:guid}";
                public const string DownloadByDocumentType = nameof(DownloadByDocumentType);
                public const string CreatByDocumentType = nameof(CreatByDocumentType);
                public const string ById = "ById/{documentId:guid}";
            }
        }
    }
}
