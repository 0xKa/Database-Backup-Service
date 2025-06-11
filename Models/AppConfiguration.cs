namespace WorkerServiceTemplate.Models
{
    public class AppConfiguration
    {
        public string? connectionString { get; set; }
        public DirectoryConfig Directories { get; set; } = new();
        public FileConfig Files { get; set; } = new();
        public CustomPathsConfig CustomPaths { get; set; } = new();
    }

    public class DirectoryConfig
    {
        public string Logs { get; set; } = "Logs";
        public string Backups { get; set; } = "Backups";
        public string Config { get; set; } = "Config";
    }

    public class FileConfig
    {
        public string ApplicationLog { get; set; } = "app.log";
        public string ErrorLog { get; set; } = "errors.log";
        public string DatabaseBackup { get; set; } = "database.bak";
    }

    public class CustomPathsConfig
    {
        // These paths can be used to override the default paths defined in FileConfig
        //can be set to null if not used
        public string? ApplicationLogPath { get; set; }
        public string? ErrorLogPath { get; set; }
        public string? DatabaseBackupPath { get; set; }
    }
}