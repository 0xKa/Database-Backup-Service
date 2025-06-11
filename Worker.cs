using Microsoft.Extensions.Options;
using WorkerServiceTemplate.Models;
using Microsoft.Data.SqlClient;
using static WorkerServiceTemplate.Utilities;

namespace WorkerServiceTemplate;


public struct App
{
    public static string connectionString = string.Empty;
    public static string appLog = string.Empty; // log service lifecycle 
    public static string errorLog = string.Empty;
    public static string backupDir = string.Empty;


    public static void InitializeFields(IOptions<AppConfiguration> config)
    {
        connectionString = config.Value.connectionString ?? string.Empty;
        appLog = GetConfiguredFilePath("ApplicationLog", config.Value.Directories.Logs);
        errorLog = GetConfiguredFilePath("ErrorLog", config.Value.Directories.Logs);

        backupDir = string.IsNullOrEmpty(config.Value.CustomPaths.DatabaseBackupPath) ? GetConfiguredDirectory("Backups") : config.Value.CustomPaths.DatabaseBackupPath;

    }

}

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public const string ServiceInternalName = "DatabaseBackupService";

    public Worker(ILogger<Worker> logger, IOptions<AppConfiguration> config, IServiceProvider serviceProvider)
    {
        _logger = logger;

        // Initialize utilities with service provider
        Initialize(serviceProvider);
        App.InitializeFields(config);

        LogMessage($"backup path: '{App.backupDir}'", "test.log");
    }


    private void DoBackup(object state)
    {
        try
        {
            string backupFile = Path.Combine(App.backupDir, $"MyDbBackup_{DateTime.Now:yyyyMMdd_HHmmss}.bak");
            string connectionString = App.connectionString;
            string sql = $"BACKUP DATABASE [Temp-DB] TO DISK = '{backupFile}' WITH  NAME = 'Full Backup of Temp-DB',FORMAT, SKIP, STATS = 10;";

            using var connection = new SqlConnection(connectionString);
            using var command = new SqlCommand(sql, connection);
            connection.Open();
            command.ExecuteNonQuery();

            LogMessage($"Backup completed at: {DateTime.Now}", App.appLog);
        }
        catch (Exception ex)
        {
            LogMessage($"Backup failed. Error: {ex.Message}", App.errorLog);
        }
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        string runMode = Environment.UserInteractive ?
            "console mode (UserInteractive = true)" :
            "Windows Service (UserInteractive = false)";

        LogMessage($"Service '{ServiceInternalName}' starting in {runMode}", App.appLog);

        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        LogMessage($"Service '{ServiceInternalName}' stopping...", App.appLog);

        return base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // LogMessage($"Service '{ServiceInternalName}' execution started.", Files.appLog);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // ===== WORKER LOGIC GOES HERE =====

                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                // ===== END OF WORKER LOGIC =====

                await Task.Delay(5000, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                LogMessage("Worker execution cancelled", App.appLog);
                break;
            }
            catch (Exception ex)
            {
                LogMessage($"Error in worker execution: {ex.Message}", App.appLog);
                _logger.LogError(ex, "Worker execution failed");

                // Wait before retrying to avoid rapid error loops
                await Task.Delay(10000, stoppingToken);
            }
        }

        // LogMessage($"Service '{ServiceInternalName}' execution ended", Files.appLog);
    }
}