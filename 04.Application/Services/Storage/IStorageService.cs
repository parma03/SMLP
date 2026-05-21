using Smlp.Base.Selections.Enums;

namespace Smlp.Application.Services.Storage;

public interface IStorageService
{
    public Task<string> CreateAsync(byte[] data);
    public Task<string> CreateAsync(byte[] data, DocumentType documentType, string filename);

    public Task<byte[]> ReadAsync(string storedFileName);
    public Task<byte[]> ReadAsync(string storedFileName, DocumentType documentType);

    public Task<string> ReadAsBase64Async(string storedFileName);
    public Task<string> ReadAsBase64Async(string storedFileName, DocumentType documentType);

    public Task<bool> DeleteAsync(string storedFileName);
    public Task<bool> DeleteAsync(string storedFileName, DocumentType documentType);

    public Task<string> UpdateAsync(string storedFileName, byte[] newData);
    public Task<string> UpdateAsync(string storedFileName, DocumentType documentType, byte[] newData);

    public string ReadPathAsync(string storedFileName);
    public string ReadPathAsync(string storedFileName, DocumentType documentType);
}
