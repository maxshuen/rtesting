# ===================================================================================
# PowerShell Script for Automating MetaTrader 4 Backtests
# ===================================================================================
#
# Description:
# This script automates the process of running backtests in MT4. It finds all
# expert advisor parameter files (.set) in a specified directory and runs a
# backtest for each one, generating a separate report for each test.
#
# ===================================================================================
# --- CONFIGURATION ---
# Please update these paths to match your system configuration.
# ===================================================================================

# Path to the MT4 terminal executable.
$mt4TerminalPath = "C:\Program Files (x86)\Alpari MT4 - bktst\terminal.exe"

# Directory where your .set parameter files are located.
$setFilesDirectory = "D:\max\AI_CAD\MT4_backtester"

# Directory where the backtest reports (.htm files) will be saved.
$reportsDirectory = "D:\max\AI_CAD\MT4_backtester\Reports"

# Path to the master .ini template file.
$templateIniPath = ".\template.ini"


# ===================================================================================
# --- SCRIPT LOGIC ---
# Do not edit below this line unless you know what you are doing.
# ===================================================================================

function Write-Log {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host "[$([datetime]::now.ToString('yyyy-MM-dd HH:mm:ss'))] $Message" -ForegroundColor $Color
}

# --- 1. Initial Sanity Checks ---
Write-Log "Starting the backtesting script..." -Color Cyan

if (-not (Test-Path -Path $mt4TerminalPath)) {
    Write-Log "FATAL ERROR: MT4 Terminal not found at '$mt4TerminalPath'." -Color Red
    Write-Log "Please correct the path in the script's configuration section." -Color Red
    exit
}

if (-not (Test-Path -Path $setFilesDirectory)) {
    Write-Log "FATAL ERROR: The directory for .set files was not found at '$setFilesDirectory'." -Color Red
    Write-Log "Please correct the path in the script's configuration section." -Color Red
    exit
}

if (-not (Test-Path -Path $templateIniPath)) {
    Write-Log "FATAL ERROR: The template INI file was not found at '$templateIniPath'." -Color Red
    Write-Log "Please ensure 'template.ini' is in the same directory as this script." -Color Red
    exit
}

# Create the reports directory if it doesn't exist
if (-not (Test-Path -Path $reportsDirectory)) {
    Write-Log "Reports directory not found. Creating it at: $reportsDirectory" -Color Yellow
    New-Item -Path $reportsDirectory -ItemType Directory | Out-Null
}

# --- 2. Find and Process .set Files ---
$setFiles = Get-ChildItem -Path $setFilesDirectory -Filter *.set
if ($setFiles.Count -eq 0) {
    Write-Log "No .set files found in '$setFilesDirectory'. Nothing to do." -Color Yellow
    exit
}

Write-Log "Found $($setFiles.Count) '.set' files to process." -Color Green
$templateIniContent = Get-Content $templateIniPath -Raw

# --- 3. Main Processing Loop ---
foreach ($setFile in $setFiles) {
    Write-Log "------------------------------------------------------------" -Color Cyan
    Write-Log "Processing: $($setFile.Name)" -Color White

    # Define paths for this specific run
    $reportFileName = "Report-$($setFile.BaseName).htm"
    $reportFilePath = Join-Path $reportsDirectory $reportFileName
    $tempIniPath = ".\_temp_current_test.ini"

    Write-Log "Report will be saved to: $reportFilePath"

    # Create the temporary .ini file for this run
    Write-Log "Generating temporary configuration file..."
    $runIniContent = $templateIniContent -replace "__SET_FILE_PATH__", $setFile.FullName
    $runIniContent = $runIniContent -replace "__REPORT_FILE_PATH__", $reportFilePath
    $runIniContent | Out-File -FilePath $tempIniPath -Encoding utf8

    # Launch the backtest
    Write-Log "Launching MT4 terminal in backtesting mode..." -Color Yellow
    Write-Log "Command: `"$mt4TerminalPath`" /config:`"$tempIniPath`""

    try {
        $process = Start-Process -FilePath $mt4TerminalPath -ArgumentList "/config:`"$tempIniPath`"" -Wait -PassThru -ErrorAction Stop
        Write-Log "MT4 process finished with exit code: $($process.ExitCode)."
    }
    catch {
        Write-Log "FATAL ERROR launching MT4. Details: $_" -Color Red
        Remove-Item -Path $tempIniPath -ErrorAction SilentlyContinue
        exit
    }

    # Verify that the report was created
    if (Test-Path $reportFilePath) {
        Write-Log "SUCCESS: Report file created successfully." -Color Green
    } else {
        Write-Log "ERROR: Report file was NOT found after the test." -Color Red
        Write-Log "Please check MT4's 'Journal' and 'Experts' tabs for errors." -Color Red
    }

    # Clean up the temporary file
    Write-Log "Cleaning up temporary files..."
    Remove-Item -Path $tempIniPath -ErrorAction SilentlyContinue
}

Write-Log "------------------------------------------------------------" -Color Cyan
Write-Log "All backtests have been processed." -Color Green
Write-Log "You can find the reports in: $reportsDirectory"
