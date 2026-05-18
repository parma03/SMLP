using Smlp.Base.Document.Enums;

namespace Smlp.Base.Document.Lists
{
    public static class DocumentList
    {
        public static IList<DocumentType> DocumentTypeLists()
        {
            var list = new List<DocumentType>
            {
                DocumentType.Laporan,
                DocumentType.Nota
            };

            return list;
        }
    }
}
