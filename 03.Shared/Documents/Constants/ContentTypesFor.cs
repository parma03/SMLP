using Smlp.Shared.Common.Constants;

namespace Smlp.Shared.Documents.Constants;

public static class ContentTypesFor
{
    public static class DocumentFile
    {
        public const string Value = @"
                application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,
                application/vnd.ms-excel,
                application/pdf,
                application/vnd.openxmlformats-officedocument.presentationml.presentation,
                application/vnd.ms-powerpoint,
                application/vnd.openxmlformats-officedocument.wordprocessingml.document,
                application/msword,
                application/zip,
                image/jpeg,
                image/png,
                text/plain";

        public const string AllowedFileExtensions = @"
                .xlsx, .xls,
                .pdf,
                .pptx, .ppt,
                .docx, .doc,
                .zip,
                .jpg, .jpeg,
                .png,
                .txt";

        public static readonly string[] List = new string[]
        {
                ContentTypes.ApplicationOpenXmlExcel,
                ContentTypes.ApplicationExcel,
                ContentTypes.ApplicationPdf,
                ContentTypes.ApplicationOpenXmlWord,
                ContentTypes.ApplicationWord,
                ContentTypes.ApplicationOpenXmlPowerPoint,
                ContentTypes.ApplicationPowerPoint,
                ContentTypes.ApplicationZip,
                ContentTypes.ImageJpeg,
                ContentTypes.ImagePng,
                ContentTypes.TextPlain
        };
    }
}
