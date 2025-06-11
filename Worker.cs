using Microsoft.Extensions.Options;
using WorkerServiceTemplate.Models;
using Microsoft.Data.SqlClient;
using static WorkerServiceTemplate.Utilities;

namespace WorkerServiceTemplate;


public struct App
{
    public static string databaseName = string.Empty;
    public static string connectionString = string.Empty;
    public static string appLog = string.Empty; // log service lifecycle 
    public static string errorLog = string.Empty;
    public static string backupDir = string.Empty;


    public static void InitializeFields(IOptions<AppConfiguration> config)
    {
        databaseName = config.Value.Database.Name ?? string.Empty;
        connectionString = config.Value.Database.ConnectionString ?? string.Empty;
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


    private void DoBackup()
    {
        try
        {
            string backupFile = Path.Combine(App.backupDir, $"{App.databaseName}_{DateTime.Now:yyyyMMdd_HHmmss}.bak");
            string connectionString = App.connectionString;
            string sql = $"BACKUP DATABASE [{App.databaseName}] TO DISK = '{backupFile}' WITH  NAME = 'Full Backup of Temp-DB',FORMAT, SKIP, STATS = 10;";

            LogMessage($"Backup file path: {backupFile}", App.appLog);
            LogMessage($"SQL: {sql}", App.appLog);


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
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                LogMessage($"Starting backup at: {DateTime.Now}", App.appLog);
                DoBackup();
            }
            catch (Exception ex)
            {
                LogMessage($"Backup error: {ex.Message}", App.errorLog);
            }

            // Wait 24 hours before next backup
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

}