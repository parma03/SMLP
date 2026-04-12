namespace Smlp.Domain.Interfaces
{
    public interface IHasFile
    {
        public string FileName { get; set; }
        public string FileContentType { get; set; }
        public long FileSize { get; set; }
        public string StorageFileId { get; set; }
    }

}
