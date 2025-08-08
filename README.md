# MT4 Batch Back-tester

This tool automates the process of running backtests in MetaTrader 4 (MT4) for your Expert Advisors (EAs). It can process an entire folder of `.set` files, running a backtest for each one and saving the results.

## Prerequisites

*   Windows operating system
*   Visual Studio
*   MetaTrader 4 (MT4) installed

## Setup

1.  **Open the project in Visual Studio:**
    Open the `MT4BackTester.csproj` file in Visual Studio.

2.  **Build the project:**
    Build the solution by clicking on `Build > Build Solution` in the menu bar. This will create the `MT4BackTester.exe` file in the `bin/Debug/net8.0` folder.

3.  **Configure the MT4 terminal path:**
    Open the `Program.cs` file and update the `terminalPath` variable with the correct path to your `terminal.exe` file.

    ```csharp
    string terminalPath = @"C:\Program Files (x86)\MetaTrader 4\terminal.exe";
    ```

## Running the Back-tester

1.  **Prepare your input folder:**
    Create a folder and place all the `.set` files you want to test inside it.

2.  **Run the back-tester:**
    Open a command prompt or PowerShell and run the back-tester. You need to provide the path to your input folder (containing the `.set` files) and the path to an output folder where the reports will be saved.

    ```bash
    MT4BackTester.exe "path\to\your\input\folder" "path\to\your\output\folder"
    ```

    The tool will then iterate through all the `.set` files in the input folder, launch an MT4 backtest for each one, and save the corresponding report (`.htm`) and settings (`.set`) files to your specified output folder.
