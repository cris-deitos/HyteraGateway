using Microsoft.Extensions.Logging;
using System.Net;

namespace HyteraGateway.Radio.Services;

/// <summary>
/// FTP client for uploading recording files to an FTP server
/// </summary>
public class FtpClient
{
    private readonly ILogger<FtpClient> _logger;

    /// <summary>
    /// Gets or sets the FTP server host
    /// </summary>
    public string FtpHost { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the FTP server port
    /// </summary>
    public int FtpPort { get; set; } = 21;

    /// <summary>
    /// Gets or sets the FTP username
    /// </summary>
    public string FtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the FTP password
    /// </summary>
    public string FtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether to use passive mode
    /// </summary>
    public bool UsePassive { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to delete files after successful upload
    /// </summary>
    public bool DeleteAfterUpload { get; set; } = false;

    /// <summary>
    /// Gets or sets the remote directory path on the FTP server
    /// </summary>
    public string RemoteDirectory { get; set; } = "/recordings";

    /// <summary>
    /// Initializes a new instance of the FtpClient
    /// </summary>
    /// <param name="logger">Logger instance</param>
    public FtpClient(ILogger<FtpClient> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Uploads a file to the FTP server
    /// </summary>
    /// <param name="localFilePath">Local file path to upload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if upload succeeded</returns>
    public async Task<bool> UploadFileAsync(string localFilePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(localFilePath))
        {
            _logger.LogError("File not found: {FilePath}", localFilePath);
            return false;
        }

        if (string.IsNullOrEmpty(FtpHost))
        {
            _logger.LogError("FTP host not configured");
            return false;
        }

        try
        {
            var fileName = Path.GetFileName(localFilePath);
            var remoteFilePath = $"{RemoteDirectory.TrimEnd('/')}/{fileName}";
            var ftpUrl = $"ftp://{FtpHost}:{FtpPort}{remoteFilePath}";

            _logger.LogDebug("Uploading {FileName} to {FtpUrl}", fileName, ftpUrl);

            var request = (FtpWebRequest)WebRequest.Create(ftpUrl);
            request.Method = WebRequestMethods.Ftp.UploadFile;
            request.Credentials = new NetworkCredential(FtpUsername, FtpPassword);
            request.UsePassive = UsePassive;
            request.UseBinary = true;
            request.KeepAlive = false;

            // Upload the file
            await using var fileStream = File.OpenRead(localFilePath);
            await using var requestStream = await request.GetRequestStreamAsync();
            
            var buffer = new byte[8192];
            int bytesRead;
            
            while ((bytesRead = await fileStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await requestStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            }

            // Get response
            using var response = (FtpWebResponse)await request.GetResponseAsync();
            
            _logger.LogInformation("Upload completed: {FileName} - {Status}", fileName, response.StatusDescription);

            // Delete local file if configured
            if (DeleteAfterUpload)
            {
                await DeleteLocalFileAsync(localFilePath, cancellationToken);
            }

            return response.StatusCode == FtpStatusCode.ClosingData || 
                   response.StatusCode == FtpStatusCode.FileActionOK;
        }
        catch (WebException ex)
        {
            if (ex.Response is FtpWebResponse response)
            {
                _logger.LogError("FTP upload failed: {Status} - {StatusDescription}", 
                    response.StatusCode, response.StatusDescription);
            }
            else
            {
                _logger.LogError(ex, "FTP upload failed for {FilePath}", localFilePath);
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FilePath}", localFilePath);
            return false;
        }
    }

    /// <summary>
    /// Uploads multiple files to the FTP server
    /// </summary>
    /// <param name="localFilePaths">Collection of local file paths to upload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of files successfully uploaded</returns>
    public async Task<int> UploadFilesAsync(IEnumerable<string> localFilePaths, CancellationToken cancellationToken = default)
    {
        var successCount = 0;

        foreach (var filePath in localFilePaths)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            if (await UploadFileAsync(filePath, cancellationToken))
            {
                successCount++;
            }
        }

        _logger.LogInformation("Uploaded {SuccessCount} of {TotalCount} files", successCount, localFilePaths.Count());
        return successCount;
    }

    /// <summary>
    /// Tests the FTP connection
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if connection successful</returns>
    public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(FtpHost))
        {
            _logger.LogError("FTP host not configured");
            return false;
        }

        try
        {
            var ftpUrl = $"ftp://{FtpHost}:{FtpPort}/";
            var request = (FtpWebRequest)WebRequest.Create(ftpUrl);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.Credentials = new NetworkCredential(FtpUsername, FtpPassword);
            request.UsePassive = UsePassive;
            request.Timeout = 5000;

            using var response = (FtpWebResponse)await request.GetResponseAsync();
            
            _logger.LogInformation("FTP connection test successful: {Status}", response.StatusDescription);
            return response.StatusCode == FtpStatusCode.OpeningData || 
                   response.StatusCode == FtpStatusCode.DataAlreadyOpen;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FTP connection test failed");
            return false;
        }
    }

    /// <summary>
    /// Creates a directory on the FTP server if it doesn't exist
    /// </summary>
    /// <param name="directoryPath">Directory path to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if directory exists or was created successfully</returns>
    public async Task<bool> EnsureDirectoryExistsAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        try
        {
            var ftpUrl = $"ftp://{FtpHost}:{FtpPort}{directoryPath}";
            var request = (FtpWebRequest)WebRequest.Create(ftpUrl);
            request.Method = WebRequestMethods.Ftp.MakeDirectory;
            request.Credentials = new NetworkCredential(FtpUsername, FtpPassword);
            request.UsePassive = UsePassive;

            using var response = (FtpWebResponse)await request.GetResponseAsync();
            
            _logger.LogDebug("Created FTP directory: {Directory}", directoryPath);
            return true;
        }
        catch (WebException ex)
        {
            // Directory might already exist
            if (ex.Response is FtpWebResponse response)
            {
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    _logger.LogDebug("FTP directory already exists: {Directory}", directoryPath);
                    return true;
                }
            }
            
            _logger.LogWarning(ex, "Could not create FTP directory: {Directory}", directoryPath);
            return false;
        }
    }

    /// <summary>
    /// Deletes a local file
    /// </summary>
    /// <param name="filePath">File path to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private Task DeleteLocalFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            File.Delete(filePath);
            
            // Also delete metadata file if it exists
            var metadataPath = Path.ChangeExtension(filePath, ".json");
            if (File.Exists(metadataPath))
            {
                File.Delete(metadataPath);
            }

            _logger.LogDebug("Deleted local file: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete local file: {FilePath}", filePath);
        }

        return Task.CompletedTask;
    }
}
