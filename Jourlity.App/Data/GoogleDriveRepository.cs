using Google.Apis.Drive.v3;
using Google.Apis.Upload;
using Jourlity.App.Data.Interface;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;

namespace Jourlity.App.Data;

/// <summary>
/// A repository implementation that uses Google Drive as a backing store.
/// It stores each entity as a separate JSON file in a dedicated application folder.
/// </summary>
/// <typeparam name="T">The type of entity, which must implement IBaseEntity.</typeparam>
public class GoogleDriveRepository<T> : IRepository<T> where T : IBaseEntity
{
    private readonly DriveService _driveService;
    private readonly string _appDataFolderName = "JourlityApp_Data"; // A dedicated folder in the user's Drive.
    private string? _folderId;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleDriveRepository{T}"/> class.
    /// </summary>
    /// <param name="driveService">An authenticated Google Drive service instance.</param>
    public GoogleDriveRepository(DriveService driveService)
    {
        _driveService = driveService ?? throw new ArgumentNullException(nameof(driveService));
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        var fileId = await FindFileIdByEntityIdAsync(id);
        if (string.IsNullOrEmpty(fileId))
        {
            return default;
        }

        var request = _driveService.Files.Get(fileId);
        await using var stream = new MemoryStream();
        await request.DownloadAsync(stream);
        stream.Position = 0; // Reset stream for reading

        return await JsonSerializer.DeserializeAsync<T>(stream);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        var folderId = await GetOrCreateAppFolderIdAsync();
        var request = _driveService.Files.List();
        request.Q = $"'{folderId}' in parents and name contains '{GetEntityTypePrefix()}' and trashed = false";
        request.Fields = "files(id, name)";
        var fileList = await request.ExecuteAsync();

        var entities = new List<T>();
        foreach (var file in fileList.Files)
        {
            var getRequest = _driveService.Files.Get(file.Id);
            await using var stream = new MemoryStream();
            await getRequest.DownloadAsync(stream);
            stream.Position = 0;

            var entity = await JsonSerializer.DeserializeAsync<T>(stream);
            if (entity != null)
            {
                entities.Add(entity);
            }
        }

        return entities;
    }

    /// <summary>
    /// Finds entities based on a predicate.
    /// WARNING: This implementation downloads ALL entities of type T from Google Drive
    /// and then filters them in memory. Use with caution on large datasets.
    /// </summary>
    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        var allEntities = await GetAllAsync();
        // Compile and run the filter in-memory
        return allEntities.Where(predicate.Compile()).ToList();
    }

    public async Task AddAsync(T entity)
    {
        var folderId = await GetOrCreateAppFolderIdAsync();
        var fileName = GetFileNameForEntity(entity.Id);

        var fileMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = fileName,
            Parents = new List<string> { folderId }
        };

        var jsonContent = JsonSerializer.Serialize(entity);
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonContent));

        var request = _driveService.Files.Create(fileMetadata, stream, "application/json");
        request.Fields = "id";
        await request.UploadAsync();
    }

    public async Task AddRangeAsync(IEnumerable<T> entities)
    {
        // For a more robust implementation, consider using the Google Drive Batch API
        // to send requests in parallel and reduce overhead.
        foreach (var entity in entities)
        {
            await AddAsync(entity);
        }
    }

    /// <summary>
    /// Not implemented. The IRepository interface uses a synchronous signature,
    /// which is not suitable for network-based APIs like Google Drive.
    /// Consider changing the interface to `Task UpdateAsync(T entity)`.
    /// </summary>
    public void Update(T entity)
    {
        throw new NotImplementedException("Synchronous updates are not supported. Please use an asynchronous method for API calls.");
    }

    /// <summary>
    /// Not implemented. The IRepository interface uses a synchronous signature,
    /// which is not suitable for network-based APIs like Google Drive.
    /// Consider changing the interface to `Task RemoveAsync(T entity)`.
    /// </summary>
    public void Remove(T entity)
    {
        throw new NotImplementedException("Synchronous removes are not supported. Please use an asynchronous method for API calls.");
    }

    /// <summary>
    /// Not implemented. See notes on `Remove`.
    // </summary>
    public void RemoveRange(IEnumerable<T> entities)
    {
        throw new NotImplementedException("Synchronous removes are not supported. Please use an asynchronous method for API calls.");
    }

    #region Private Helpers

    private string GetEntityTypePrefix() => $"{typeof(T).Name}_";
    private string GetFileNameForEntity(Guid id) => $"{GetEntityTypePrefix()}{id}.json";

    private async Task<string?> FindFileIdByEntityIdAsync(Guid entityId)
    {
        var folderId = await GetOrCreateAppFolderIdAsync();
        var fileName = GetFileNameForEntity(entityId);

        var request = _driveService.Files.List();
        // Query for a file with the exact name within our app folder, that isn't in the trash.
        request.Q = $"name = '{fileName}' and '{folderId}' in parents and trashed = false";
        request.Spaces = "drive";
        request.Fields = "files(id)";

        var result = await request.ExecuteAsync();
        return result.Files.FirstOrDefault()?.Id;
    }

    private async Task<string> GetOrCreateAppFolderIdAsync()
    {
        if (!string.IsNullOrEmpty(_folderId))
        {
            return _folderId;
        }

        var request = _driveService.Files.List();
        request.Q = $"mimeType = 'application/vnd.google-apps.folder' and name = '{_appDataFolderName}' and trashed = false";
        request.Spaces = "drive";
        request.Fields = "files(id)";

        var fileList = await request.ExecuteAsync();
        var existingFolder = fileList.Files.FirstOrDefault();

        if (existingFolder != null)
        {
            _folderId = existingFolder.Id;
            return _folderId;
        }

        var folderMetadata = new Google.Apis.Drive.v3.Data.File()
        {
            Name = _appDataFolderName,
            MimeType = "application/vnd.google-apps.folder"
        };
        var createRequest = _driveService.Files.Create(folderMetadata);
        createRequest.Fields = "id";
        var folder = await createRequest.ExecuteAsync();

        _folderId = folder.Id;
        return _folderId;
    }

    #endregion
}