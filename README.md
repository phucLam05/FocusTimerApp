# FocusTimerApp

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

FocusTimerApp is an open-source Windows desktop application built with **WPF (.NET 9)** that helps users improve focus while working or studying on their computer.

---

## Description

FocusTimerApp allows users to register the applications they use for studying or working. When you switch away from those tracked apps, a notification is sent.  

The app supports two timer modes:  

- **Work Tracking**: Counts total working time and notifies you to take a break after a configurable interval.  
- **Pomodoro**: Implements typical work/break intervals (e.g., 25 minutes work / 5 minutes break).  

Additional features include:  

- Adding a local playlist to play music while working.  
- Configurable app settings: tracked apps, work/break intervals, playlist folder, notifications, etc.  
- MVVM architecture for maintainable, clean code.  
- JSON files for persisting settings and user data.  

NuGet packages used:  

```xml
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
<PackageReference Include="Microsoft.Xaml.Behaviors.Wpf" Version="1.1.135" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
<PackageReference Include="TagLibSharp" Version="2.3.0" />
```

---

## System Requirements

- Windows 10 or later  
- .NET 9 SDK (if building from source)  
- Visual Studio 2022 or any IDE supporting WPF / .NET 9  
- Access to tracked applications and permission to display notifications  

---

## Installation & Running

1. Clone the repository:

```bash
git clone https://github.com/phucLam05/FocusTimerApp.git
```

2. Open the solution in Visual Studio.  
3. Restore NuGet packages (`dotnet restore` if needed).  
4. Build the solution (Debug or Release).  
5. Run the application (`.exe` or from IDE).  
6. Configure settings: add tracked apps, select timer mode, set intervals, add playlist, and start your session.

---

## Architecture & Technology

- **Architecture**: MVVM (Model-View-ViewModel)  
- **Models**: Data representations (tracked apps, settings, playlist)  
- **ViewModels**: Handles logic, commands, and data binding  
- **Views**: WPF XAML + minimal code-behind  
- **Data Persistence**: JSON files, serialized with Newtonsoft.Json  
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection  
- **Audio**: TagLibSharp for music metadata and WPF-based playback  
- **Behaviors**: Microsoft.Xaml.Behaviors.Wpf for UI behaviors  

---

## Usage Example

1. Launch the app → go to **Settings** → add apps to track (e.g., Visual Studio, Chrome).  
2. Choose timer mode:  
   - **Tracking**: set work and break intervals.  
   - **Pomodoro**: set work/break duration per session.  
3. Add a playlist folder for music playback.  
4. Configure notifications.  
5. Click **Start** → session begins.  
   - Notifications occur if you leave tracked apps.  
   - Session end triggers break notifications.  
6. Review session stats: total work time, number of sessions, break time.

---

## License

This project is licensed under the **MIT License**, allowing anyone to use, modify, and distribute it freely.  

© 2025 phucLam05  

Full MIT License text can be included in a `LICENSE` file.

---

Thank you for using **FocusTimerApp**! ⭐ Star the repository if you like it, and feel free to open issues or submit improvements.
