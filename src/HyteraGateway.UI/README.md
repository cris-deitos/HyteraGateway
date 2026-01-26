# HyteraGateway.UI - WPF Desktop Application

A modern WPF desktop application for monitoring and configuring the HyteraGateway system.

## Features

- **Material Design Theme**: Modern, clean UI using MaterialDesignInXaml
- **MVVM Pattern**: Proper separation of concerns using CommunityToolkit.Mvvm
- **Real-time Updates**: SignalR integration for live status updates
- **System Tray Support**: Minimize to tray functionality
- **Navigation**: Easy navigation between different views
- **Dashboard**: System status overview with key metrics
- **Responsive Design**: Clean, professional interface

## Architecture

### Project Structure

```
HyteraGateway.UI/
├── App.xaml              # Application entry point with Material Design theme
├── App.xaml.cs           # Dependency injection configuration
├── MainWindow.xaml       # Main application shell with navigation
├── MainWindow.xaml.cs    # Main window code-behind
├── Assets/               # Icons and other resources
├── Models/               # View models for data binding
├── Services/             # Business logic and API communication
│   ├── SignalRService.cs    # Real-time updates via SignalR
│   ├── ApiService.cs        # HTTP API communication
│   └── ConfigurationService.cs
├── ViewModels/           # MVVM ViewModels
│   ├── MainViewModel.cs
│   ├── DashboardViewModel.cs
│   └── ...
└── Views/                # XAML views
    ├── DashboardView.xaml
    └── ...
```

### Technology Stack

- **.NET 8.0** (Windows target framework)
- **WPF** - Windows Presentation Foundation
- **MaterialDesignThemes** (5.0.0) - Material Design UI components
- **MaterialDesignColors** (3.0.0) - Color themes
- **LiveChartsCore.SkiaSharpView.WPF** (2.0.0-rc2) - Charts and graphs
- **CommunityToolkit.Mvvm** (8.2.2) - MVVM helpers and source generators
- **Microsoft.AspNetCore.SignalR.Client** (8.0.0) - Real-time communication
- **Hardcodet.NotifyIcon.Wpf** (1.1.0) - System tray icon support
- **System.Net.Http.Json** (8.0.0) - HTTP JSON API calls

## Views

### Dashboard View
- System status cards (Service, Active Radios, Calls Today, Recordings)
- Active radio list with DMR ID, Name, Status, Signal Strength, Last Activity
- Connection status (Database, FTP Server)

### Statistics View
- Placeholder for statistics and analytics (Coming Soon)

### Logs View
- Placeholder for log viewing (Coming Soon)

### Settings View
- Placeholder for configuration settings (Coming Soon)

### Tools View
- Placeholder for diagnostic tools (Coming Soon)

## Building

### Prerequisites

- .NET 8.0 SDK or later
- Windows OS (WPF is Windows-only)

### Build Commands

```bash
# Restore packages
dotnet restore src/HyteraGateway.UI/HyteraGateway.UI.csproj

# Build
dotnet build src/HyteraGateway.UI/HyteraGateway.UI.csproj

# Run
dotnet run --project src/HyteraGateway.UI/HyteraGateway.UI.csproj
```

## Configuration

The application connects to the HyteraGateway backend API and SignalR hub at:
- API Base URL: `http://localhost:5000`
- SignalR Hub: `http://localhost:5000/hubs/radio-events`

These can be configured in the `Services/ApiService.cs` and `Services/SignalRService.cs` files.

## Development

### Adding New Views

1. Create XAML view in `Views/` folder
2. Create corresponding ViewModel in `ViewModels/` folder
3. Register ViewModel in `App.xaml.cs` DI container
4. Add navigation menu item in `MainWindow.xaml`
5. Update navigation switch in `MainWindow.xaml.cs`

### Working with Material Design

The application uses Material Design In XAML Toolkit. Common components:

- `materialDesign:Card` - Card container
- `materialDesign:PackIcon` - Material icons
- `materialDesign:ColorZone` - Colored header/footer areas
- `materialDesign:DialogHost` - Dialogs and popups

See [MaterialDesignInXamlToolkit Documentation](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) for more details.

## Known Limitations

- System tray icon requires a valid `.ico` file (currently placeholder)
- Real-time features require HyteraGateway backend API to be running
- Some views are placeholders and not fully implemented yet

## Future Enhancements

- Complete Statistics view with charts and graphs
- Complete Logs view with real-time log streaming
- Complete Settings view for configuration management
- Complete Tools view for diagnostics
- Add proper application icon
- Add user authentication
- Add configuration persistence
- Add themes support (Dark/Light mode toggle)
