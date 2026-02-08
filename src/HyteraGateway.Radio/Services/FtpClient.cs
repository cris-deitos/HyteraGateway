using FluentFTP;
using Microsoft.Extensions.Logging;

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

            _logger.LogDebug("Uploading {FileName} to {FtpHost}:{FtpPort}{RemotePath}", fileName, FtpHost, FtpPort, remoteFilePath);

            using var ftp = new AsyncFtpClient(FtpHost, FtpUsername, FtpPassword, FtpPort);
            ftp.Config.DataConnectionType = UsePassive ? FtpDataConnectionType.PASV : FtpDataConnectionType.PORT;
            
            await ftp.Connect(cancellationToken);
            
            var status = await ftp.UploadFile(
                localFilePath, 
                remoteFilePath, 
                FtpRemoteExists.Overwrite, 
                createRemoteDir: true, 
                verifyOptions: FtpVerify.Retry, 
                progress: null, 
                cancellationToken);
            
            _logger.LogInformation("Upload completed: {FileName} - Status: {Status}", fileName, status);

            // Delete local file if configured
            if (DeleteAfterUpload && status == FtpStatus.Success)
            {
                await DeleteLocalFileAsync(localFilePath, cancellationToken);
            }

            return status == FtpStatus.Success;
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
            using var ftp = new AsyncFtpClient(FtpHost, FtpUsername, FtpPassword, FtpPort);
            ftp.Config.DataConnectionType = UsePassive ? FtpDataConnectionType.PASV : FtpDataConnectionType.PORT;
            ftp.Config.ConnectTimeout = 5000;
            
            await ftp.Connect(cancellationToken);
            
            var isConnected = ftp.IsConnected;
            
            _logger.LogInformation("FTP connection test {Result}", isConnected ? "successful" : "failed");
            
            return isConnected;
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
            using var ftp = new AsyncFtpClient(FtpHost, FtpUsername, FtpPassword, FtpPort);
            ftp.Config.DataConnectionType = UsePassive ? FtpDataConnectionType.PASV : FtpDataConnectionType.PORT;
            
            await ftp.Connect(cancellationToken);
            
            var exists = await ftp.DirectoryExists(directoryPath, cancellationToken);
            
            if (!exists)
            {
                await ftp.CreateDirectory(directoryPath, cancellationToken);
                _logger.LogDebug("Created FTP directory: {Directory}", directoryPath);
            }
            else
            {
                _logger.LogDebug("FTP directory already exists: {Directory}", directoryPath);
            }
            
            return true;
        }
        catch (Exception ex)
        {
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
