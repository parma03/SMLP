using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Smlp.Application.Services.Storage;
using Smlp.Base.Document.Enums;

namespace Smlp.Infrastructure.Storage.LocalFolder;

public class LocalFolderStorageService : IStorageService
{
    private readonly string _folderPath;
    private readonly ILogger<LocalFolderStorageService> _logger;

    public LocalFolderStorageService(IOptions<LocalFolderStorageOptions> options, ILogger<LocalFolderStorageService> logger)
    {
        _folderPath = options.Value.FolderPath;
        _logger = logger;

        if (!Directory.Exists(_folderPath))
            Directory.CreateDirectory(_folderPath);
    }

    public async Task<string> CreateAsync(byte[] data)
    {
        var storageFileId = $"{Guid.NewGuid()}{Guid.NewGuid()}";
        var filePath = Path.Combine(_folderPath, storageFileId);
        await File.WriteAllBytesAsync(filePath, data);
        return storageFileId;
    }

    public async Task<string> CreateAsync(byte[] data, DocumentType documentType, string filename)
    {
        var folder = GetFolderPath(documentType);
        if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
        var filePath = Path.Combine(folder, filename);
        await File.WriteAllBytesAsync(filePath, data);
        return filename;
    }

    public Task<byte[]> ReadAsync(string storedFileName)
    {
        var filePath = Path.Combine(_folderPath, storedFileName);
        return File.ReadAllBytesAsync(filePath);
    }

    public Task<byte[]> ReadAsync(string storedFileName, DocumentType documentType)
    {
        var filePath = Path.Combine(GetFolderPath(documentType), storedFileName);
        return File.ReadAllBytesAsync(filePath);
    }

    public async Task<string> ReadAsBase64Async(string storedFileName)
    {
        var bytes = await ReadAsync(storedFileName);
        return Convert.ToBase64String(bytes);
    }

    public async Task<string> ReadAsBase64Async(string storedFileName, DocumentType documentType)
    {
        var bytes = await ReadAsync(storedFileName, documentType);
        return Convert.ToBase64String(bytes);
    }

    public Task<bool> DeleteAsync(string storedFileName)
    {
        var filePath = Path.Combine(_folderPath, storedFileName);
        if (File.Exists(filePath)) File.Delete(filePath);
        return Task.FromResult(true);
    }

    public Task<bool> DeleteAsync(string storedFileName, DocumentType documentType)
    {
        var filePath = Path.Combine(GetFolderPath(documentType), storedFileName);
        if (File.Exists(filePath)) File.Delete(filePath);
        return Task.FromResult(true);
    }

    public async Task<string> UpdateAsync(string storedFileName, byte[] newData)
    {
        await DeleteAsync(storedFileName);
        return await CreateAsync(newData);
    }

    public async Task<string> UpdateAsync(string storedFileName, DocumentType documentType, byte[] newData)
    {
        await DeleteAsync(storedFileName, documentType);
        return await CreateAsync(newData, documentType, storedFileName);
    }

    public string ReadPathAsync(string storedFileName) => Path.Combine(_folderPath, storedFileName);

    public string ReadPathAsync(string storedFileName, DocumentType documentType) =>
        Path.Combine(GetFolderPath(documentType), storedFileName);

    private string GetFolderPath(DocumentType documentType)
    {
        var subFolder = documentType.ToString();
        return Path.Combine(_folderPath, subFolder);
    }
}
