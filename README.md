# MT4 Back-tester

This tool automates the process of running backtests in MetaTrader 4 (MT4) for your Expert Advisors (EAs). It reads settings from JSON files in a specified folder, and for each file, it launches MT4 with the corresponding parameters and saves the backtest report to a local folder.

## Prerequisites

*   Windows operating system
*   Visual Studio
*   MetaTrader 4 (MT4) installed

## Setup

1.  **Open the project in Visual Studio:**
    Open the `MT4BackTester.csproj` file in Visual Studio.

2.  **Build the project:**
    Build the solution by clicking on `Build > Build Solution` in the menu bar. This will create the `MT4BackTester.exe` file in the `bin/Debug/net8.0` folder.

3.  **Create a settings folder:**
    Create a folder where you will store your backtest setting files. For example, you can create a folder named `Settings` in the project's root directory.

4.  **Create setting files:**
    In the settings folder, create a JSON file for each backtest you want to run. The JSON file should have the following structure:

    ```json
    {
      "ExpertAdvisor": "Moving Average",
      "Symbol": "EURUSD",
      "Period": 15,
      "FromDate": "2023-01-01",
      "ToDate": "2023-12-31",
      "Parameters": {
        "Lots": "0.1",
        "StopLoss": "50",
        "TakeProfit": "100",
        "MovingPeriod": {
            "Start": 10,
            "Step": 1,
            "Stop": 20
        },
        "MovingShift": "0"
      },
      "Optimization": true
    }
    ```

    *   `ExpertAdvisor`: The name of your Expert Advisor.
    *   `Symbol`: The symbol to run the backtest on (e.g., "EURUSD").
    *   `Period`: The chart period in minutes (e.g., 1, 5, 15, 60).
    *   `FromDate`: The start date of the backtest in "YYYY-MM-DD" format.
    *   `ToDate`: The end date of the backtest in "YYYY-MM-DD" format.
    *   `Parameters`: A dictionary of the parameters for your Expert Advisor. For parameters that you want to optimize, you can provide an object with `Start`, `Step`, and `Stop` values.
    *   `Optimization`: A boolean value that indicates whether to run an optimization.

### Constraints

Currently, the tool does not support constraints directly in the setting files. However, you can implement constraints in your Expert Advisor's code. For example, you can add code to your EA to check if the parameter values are valid and to skip the backtest if they are not.

5.  **Configure the MT4 terminal path:**
    Open the `Program.cs` file and update the `terminalPath` variable with the correct path to your `terminal.exe` file.

    ```csharp
    string terminalPath = @"C:\Program Files (x86)\MetaTrader 4\terminal.exe";
    ```

## Running the Back-tester

1.  **Open a command prompt:**
    Open a command prompt or PowerShell window.

2.  **Navigate to the project's output directory:**
    Navigate to the directory where the `MT4BackTester.exe` file was created.

    ```bash
    cd path\to\your\project\MT4BackTester\bin\Debug\net8.0
    ```

3.  **Run the back-tester:**
    Run the back-tester by providing the path to your settings folder and the output folder as command-line arguments.

    ```bash
    MT4BackTester.exe "path\to\your\settings\folder" "path\to\your\output\folder"
    ```

    The back-tester will then iterate through all the JSON files in the settings folder, launch MT4 for each file, and run the backtest. The backtest reports will be saved in the specified output folder.

### New Features in This Version

*   **Use Tick Data:** The back-tester now uses tick data for backtesting, which provides the most accurate results.
*   **Progress Indication:** The back-tester now provides progress indication during the optimization process, so you can see that the tool is still working and has not halted.
*   **Save Optimized Settings:** The back-tester now saves the optimized settings to a `.set` file in the same format as the MT4 terminal.
*   **Save Backtest Report:** The back-tester now saves the backtest report as an `.htm` file, including the graph.
*   **Timer:** The back-tester now shows the elapsed time of the backtest.
*   **Summary File:** The back-tester now creates a CSV file that summarizes the results of the optimization.
