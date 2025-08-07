# MT4 Back-tester

This project contains two tools:

*   **SetToJson:** A tool to convert `.set` files to `.json` files.
*   **MT4BackTester:** A tool to run backtests in MetaTrader 4 (MT4) for your Expert Advisors (EAs).

## SetToJson

This tool converts a `.set` file from the MT4 terminal to a `.json` file that can be used with the back-tester.

### Usage

```bash
SetToJson.exe "path\to\your\set\file.set" "path\to\your\json\file.json"
```

## MT4BackTester

This tool runs a backtest in MT4 for your Expert Advisor. It takes a `.json` file as input and saves the backtest report and settings to a specified folder.

### Prerequisites

*   Windows operating system
*   Visual Studio
*   MetaTrader 4 (MT4) installed

### Setup

1.  **Open the project in Visual Studio:**
    Open the `MT4BackTester.sln` file in Visual Studio.

2.  **Build the project:**
    Build the solution by clicking on `Build > Build Solution` in the menu bar. This will create the `MT4BackTester.exe` and `SetToJson.exe` files in the `bin/Debug/net8.0` folder.

3.  **Configure the MT4 terminal path:**
    Open the `MT4BackTester/Program.cs` file and update the `terminalPath` variable with the correct path to your `terminal.exe` file.

    ```csharp
    string terminalPath = @"C:\Program Files (x86)\MetaTrader 4\terminal.exe";
    ```

### Running the Back-tester

1.  **Convert your `.set` file to a `.json` file:**
    Use the `SetToJson.exe` tool to convert your `.set` file to a `.json` file.

    ```bash
    SetToJson.exe "path\to\your\set\file.set" "path\to\your\json\file.json"
    ```

2.  **Run the back-tester:**
    Run the back-tester by providing the path to your `.json` file and the output folder as command-line arguments.

    ```bash
    MT4BackTester.exe "path\to\your\json\file.json" "path\to\your\output\folder"
    ```

    The back-tester will then run a single backtest with the settings from the `.json` file. The backtest report and settings will be saved in the specified output folder.
