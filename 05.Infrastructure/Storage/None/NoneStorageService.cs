using Smlp.Application.Services.Storage;
using Smlp.Base.Document.Enums;

namespace Smlp.Infrastructure.Storage.None;

public class NoneStorageService : IStorageService
{
    public Task<string> CreateAsync(byte[] data) => Task.FromResult(string.Empty);
    public Task<string> CreateAsync(byte[] data, DocumentType documentType, string filename) => Task.FromResult(filename);
    public Task<byte[]> ReadAsync(string storedFileName) => Task.FromResult(Array.Empty<byte>());
    public Task<byte[]> ReadAsync(string storedFileName, DocumentType documentType) => Task.FromResult(Array.Empty<byte>());
    public Task<string> ReadAsBase64Async(string storedFileName) => Task.FromResult(string.Empty);
    public Task<string> ReadAsBase64Async(string storedFileName, DocumentType documentType) => Task.FromResult(string.Empty);
    public Task<bool> DeleteAsync(string storedFileName) => Task.FromResult(true);
    public Task<bool> DeleteAsync(string storedFileName, DocumentType documentType) => Task.FromResult(true);
    public Task<string> UpdateAsync(string storedFileName, byte[] newData) => Task.FromResult(string.Empty);
    public Task<string> UpdateAsync(string storedFileName, DocumentType documentType, byte[] newData) => Task.FromResult(string.Empty);
    public string ReadPathAsync(string storedFileName) => string.Empty;
    public string ReadPathAsync(string storedFileName, DocumentType documentType) => string.Empty;
}
