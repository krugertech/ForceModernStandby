# ForceModernStandby

## Overview

**ForceModernStandby** is a C# application developed to address persistent issues with Windows Modern Standby mode not activating correctly on certain laptops.

Users experiencing continuous power drain or failed suspension attempts can utilize this tool as an experimental solution to enforce Modern Standby.

## How It Works

  - **If Airplane Mode is Disabled:** The application enables airplane mode to disable all wireless connections, ensuring minimal power consumption and preparing the system for standby.

  - **If Airplane Mode is Already Enabled:** The application disables airplane mode, restoring the system's previous wireless settings.

## Usage

### Prerequisites

- Windows 10 or later and a S0/modern standby capable computer.

### Installation

- Build or download the latest release and drag a shortcut from the exe to your taskbar for quick access.

## Contributing

Contributions are welcome! If you'd like to help:

1. **Fork the Repository**
2. **Make Your Changes**
3. **Submit a Pull Request**

Open an issue if you find any bugs or have suggestions.

## Important Note

**ForceModernStandby** is an experimental tool designed to help resolve Modern Standby issues. While it may work for some systems, results can vary based on your hardware and configuration. Use it with caution, as it might not fix all problems and could introduce new ones.
