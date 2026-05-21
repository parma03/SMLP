using Smlp.Base.Document.Enums;

namespace Smlp.Application.Services.Storage;

public interface IStorageService
{
    Task<string> CreateAsync(byte[] data);
    Task<string> CreateAsync(byte[] data, DocumentType documentType, string filename);

    Task<byte[]> ReadAsync(string storedFileName);
    Task<byte[]> ReadAsync(string storedFileName, DocumentType documentType);

    Task<string> ReadAsBase64Async(string storedFileName);
    Task<string> ReadAsBase64Async(string storedFileName, DocumentType documentType);

    Task<bool> DeleteAsync(string storedFileName);
    Task<bool> DeleteAsync(string storedFileName, DocumentType documentType);

    Task<string> UpdateAsync(string storedFileName, byte[] newData);
    Task<string> UpdateAsync(string storedFileName, DocumentType documentType, byte[] newData);

    string ReadPathAsync(string storedFileName);
    string ReadPathAsync(string storedFileName, DocumentType documentType);
}
