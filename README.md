# Database Backup Service

A Windows Service application built with .NET 9.0 that automates database backup operations. This service runs continuously in the background, performing scheduled database backups according to configured parameters.

## Features

- **Automated Database Backups**: Continuously runs as a Windows Service to perform scheduled database backups
- **Configurable Settings**: Uses JSON configuration files for flexible setup
- **Comprehensive Logging**: Built-in logging system with timestamped entries
- **Error Handling**: Robust error handling with fallback mechanisms
- **Directory Management**: Automatic creation and management of backup directories
- **Environment Detection**: Different behavior for development vs production environments

## Configuration

The service uses JSON configuration files:

- `appsettings.json` - Production settings
- `appsettings.Development.json` - Development settings

Configuration is managed through the [`AppConfiguration`](Models/AppConfiguration.cs) model and accessed via the [`Utilities`](Utilities.cs) class.

## Key Components

### Worker Service

The main service logic is implemented in [`Worker.cs`](Worker.cs), which runs continuously and performs the backup operations.

### Utilities

The [`Utilities`](Utilities.cs) class provides essential functionality:

- **Logging**: [`LogMessage`](Utilities.cs) method for timestamped logging
- **Directory Management**: [`CreateDirectoryInside`](Utilities.cs) and [`CreateDirectoryWithinProject`](Utilities.cs) methods
- **Configuration Access**: [`GetConfiguredFilePath`](Utilities.cs) and [`GetConfiguredDirectory`](Utilities.cs) methods
- **Path Resolution**: [`GetProjectRootDirectory`](Utilities.cs) method with environment-specific logic

## Installation & Setup

### Prerequisites

- .NET 9.0
- Windows operating system (for Windows Service functionality)
- Appropriate database access permissions

### Building the Project

1. Clone the repository
2. Navigate to the project directory
3. Build the solution:

   ```bash
   dotnet build
   ```

### Running in Development

```bash
dotnet run
```

### Publishing for Production

```bash
dotnet publish -c Release -o publish
```

### Installing as Windows Service

After publishing, install as a Windows Service using PowerShell (run as Administrator):

```powershell
sc create "Database Backup Service" binPath="C:\path\to\your\publish\Database-Backup-Service.exe"
sc start "Database Backup Service"
```

### Add dependency services

To ensure the service starts correctly, add any required dependencies:

```powershell
sc config "Database Backup Service" depend= "MSSQLSERVER/RpcSs/EventLog"
```

## Usage

The service automatically starts performing backups based on the configured schedule. Monitor the operation through:

- **Log Files**: Check [`Logs/app.log`](Logs/app.log) for general operations and [`Logs/errors.log`](Logs/errors.log) for errors
- **Windows Event Viewer**: Service-related events are logged to the Windows Event Log
- **Console Output**: When running in development mode
