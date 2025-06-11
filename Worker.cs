using Microsoft.Extensions.Options;
using WorkerServiceTemplate.Models;
using static WorkerServiceTemplate.Utilities;

namespace WorkerServiceTemplate;


public struct Files
{
    public static string appLog = string.Empty; // log service lifecycle 
    public static string errorLog = string.Empty;


    public static void InitializeFields(IOptions<AppConfiguration> config)
    {
        appLog = GetConfiguredFilePath("ApplicationLog", config.Value.Directories.Logs);
        errorLog = GetConfiguredFilePath("ErrorLog", config.Value.Directories.Logs);

    }

}

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public const string ServiceInternalName = "DtabaseBackupService";

    public Worker(ILogger<Worker> logger, IOptions<AppConfiguration> config, IServiceProvider serviceProvider)
    {
        _logger = logger;

        // Initialize utilities with service provider
        Initialize(serviceProvider);
        Files.InitializeFields(config);


    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        string runMode = Environment.UserInteractive ?
            "console mode (UserInteractive = true)" :
            "Windows Service (UserInteractive = false)";

        LogMessage($"Service '{ServiceInternalName}' starting in {runMode}", Files.appLog);

        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        LogMessage($"Service '{ServiceInternalName}' stopping...", Files.appLog);

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
                LogMessage("Worker execution cancelled", Files.appLog);
                break;
            }
            catch (Exception ex)
            {
                LogMessage($"Error in worker execution: {ex.Message}", Files.appLog);
                _logger.LogError(ex, "Worker execution failed");

                // Wait before retrying to avoid rapid error loops
                await Task.Delay(10000, stoppingToken);
            }
        }

        // LogMessage($"Service '{ServiceInternalName}' execution ended", Files.appLog);
    }
}