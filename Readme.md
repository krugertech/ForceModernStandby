# ForceModernStandby

<!--## Table of Contents
- [Overview](#overview)
- [How It Works](#how-it-works)
- [Usage](#usage)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Running the Application](#running-the-application)
- [Error Handling](#error-handling)
- [Contributing](#contributing)
- [Important Note](#important-note)
- [License](#license)
- [Contact](#contact)-->

## Overview

**ForceModernStandby** is a C# application developed to address persistent issues with Windows Modern Standby mode not activating correctly on certain computers.

Users experiencing continuous power drain or failed suspension attempts can utilize this tool as an experimental solution to enforce Modern Standby.

## How It Works

**ForceModernStandby** leverages several system components to manage the Modern Standby state:

1. **Airplane Mode Management:**
   - **Enabling Airplane Mode:** Disables all wireless connections to minimize power consumption and prepare the system for standby.
   - **Disabling Airplane Mode:** Restores the system's previous wireless settings, re-enabling network connectivity.

2. **Network Adapter Control:**
   - **Disabling Network Adapters:** Turns off all network adapters when airplane mode is enabled to ensure no wireless communication persists.
   - **Enabling Network Adapters:** Re-enables all network adapters when airplane mode is disabled to restore network functionality.

3. **Power Monitoring:**
   - **PowerMonitor:** Observes changes in the system's power state. When a power state change is detected, it triggers the logic to toggle airplane mode and manage network adapters accordingly.

4. **Modern Standby Activation:**
   - Utilizes `SleepManager.ModernStandbySleepWorkaround()` to programmatically put the computer into Modern Standby mode after configuring airplane mode and network adapters.

## Usage

### Prerequisites

- **Operating System:** Windows 10 or later.
- **Hardware:** A computer that supports S0/Modern Standby.
- **.NET Runtime:** Ensure that the .NET 8 runtime is installed when building from source.

### Installation

1. **Build or Download the Latest Release:**
   - **Build from Source:**
     - Clone the repository:
       ```bash
       git clone https://github.com/yourusername/ForceModernStandby.git
       ```
     - Open the solution in your preferred C# development environment (e.g., Visual Studio).
     - Restore dependencies and build the project.
   - **Download Executable:**
     - Visit the [Releases](https://github.com/krugertech/ForceModernStandby/releases) section.
     - Download the latest `ForceModernStandby.exe` release.

2. **Create a Shortcut for Quick Access:**
   - Navigate to the executable (`ForceModernStandby.exe`).
   - Right-click and select **Create shortcut**.
   - Drag the shortcut to your taskbar for easy access.

### Running the Application

1. **Launch the Application:**
   - Click the taskbar shortcut or execute `ForceModernStandby.exe` directly.

2. **Application Behavior:**
   - **Initial State:**
     - The application enables airplane mode, disabling all wireless connections.
     - Disables all network adapters to ensure no active wireless communication.
   - **Activating Modern Standby:**
     - Puts the computer into Modern Standby mode using a workaround method.
   - **Monitoring Power Changes:**
     - The application listens for power state changes. If a change is detected:
       - **If Airplane Mode is Enabled:**
         - Disables airplane mode.
         - Re-enables all network adapters.
         - Logs the restoration of power settings.
       - **If Airplane Mode is Disabled:**
         - No action is taken.

3. **Exiting the Application:**
   - The application will continue running until the power state is restored.
   - To manually exit, press **Ctrl+C** in the console window if no response is detected.

## Error Handling

All exceptions are logged to the console to aid in troubleshooting.

## Contributing

Contributions are welcome! If you'd like to help improve **ForceModernStandby**, follow these steps:

1. **Fork the Repository**
2. **Make Your Changes**
3. **Submit a Pull Request**

## Important Note

**ForceModernStandby** is an experimental tool designed to help resolve Modern Standby issues. While it may work for some systems, results can vary based on your hardware and configuration. Use it with caution, as it might not fix all problems and could introduce new ones.
