using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace MT4BackTester
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please provide the path to the settings folder.");
                return;
            }

            string settingsFolder = args[0];
            if (!Directory.Exists(settingsFolder))
            {
                Console.WriteLine($"Settings folder not found: {settingsFolder}");
                return;
            }

            var settingFiles = Directory.GetFiles(settingsFolder, "*.json");
            foreach (var settingFile in settingFiles)
            {
                try
                {
                    string json = File.ReadAllText(settingFile);
                    var settings = JsonConvert.DeserializeObject<BacktestSettings>(json);

                    RunBacktest(settings);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing setting file {settingFile}: {ex.Message}");
                }
            }
        }

        static void RunBacktest(BacktestSettings settings)
        {
            // Path to the MT4 terminal executable
            string terminalPath = @"C:\Program Files (x86)\MetaTrader 4\terminal.exe";

            // Path to the MT4 configuration file
            string configFile = Path.Combine(Path.GetTempPath(), "mt4config.ini");

            // Create the configuration file content
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[Tester]");
            sb.AppendLine($"Expert={settings.ExpertAdvisor}");
            sb.AppendLine($"Symbol={settings.Symbol}");
            sb.AppendLine($"Period={settings.Period}");
            sb.AppendLine($"FromDate={settings.FromDate:yyyy.MM.dd}");
            sb.AppendLine($"ToDate={settings.ToDate:yyyy.MM.dd}");
            string reportFileName = $"{settings.ExpertAdvisor}-{settings.Symbol}-{settings.Period}-{DateTime.Now:yyyyMMddHHmmss}";
            sb.AppendLine($"Report=Reports\\{reportFileName}");
            sb.AppendLine("Model=0"); // 0 for Every tick, 1 for Control points, 2 for Open prices only
            sb.AppendLine("TestOnTick=true"); // Use tick data

            // Add expert advisor parameters
            foreach (var param in settings.Parameters)
            {
                if (param.Value is string)
                {
                    sb.AppendLine($"{param.Key}={param.Value}");
                }
                else if (param.Value is Newtonsoft.Json.Linq.JObject)
                {
                    var optimizationSettings = ((Newtonsoft.Json.Linq.JObject)param.Value).ToObject<OptimizationSettings>();
                    sb.AppendLine($"{param.Key},F=1");
                    sb.AppendLine($"{param.Key},1={optimizationSettings.Start}");
                    sb.AppendLine($"{param.Key},2={optimizationSettings.Step}");
                    sb.AppendLine($"{param.Key},3={optimizationSettings.Stop}");
                }
            }

            if (settings.Optimization)
            {
                sb.AppendLine("TestExpertEnable=true");
                sb.AppendLine("TestOptimization=true");
            }

            File.WriteAllText(configFile, sb.ToString());

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = terminalPath,
                Arguments = $"/config:\"{configFile}\"",
                UseShellExecute = false
            };

            try
            {
                Console.WriteLine($"Starting backtest for {settings.ExpertAdvisor} on {settings.Symbol}...");
                using (Process process = Process.Start(startInfo))
                {
                    while (!process.HasExited)
                    {
                        Console.Write(".");
                        System.Threading.Thread.Sleep(5000);
                    }
                }
                Console.WriteLine($"\nBacktest for {settings.ExpertAdvisor} on {settings.Symbol} completed.");

                // Save the optimized settings
                if (settings.Optimization)
                {
                    string optimizedSettingsPath = Path.Combine("Reports", $"{reportFileName}.json");
                    string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    File.WriteAllText(optimizedSettingsPath, json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting MT4: {ex.Message}");
            }
            finally
            {
                if (File.Exists(configFile))
                {
                    File.Delete(configFile);
                }
            }
        }
    }

    public class BacktestSettings
    {
        public string ExpertAdvisor { get; set; }
        public string Symbol { get; set; }
        public int Period { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public bool Optimization { get; set; }
    }

    public class OptimizationSettings
    {
        public double Start { get; set; }
        public double Step { get; set; }
        public double Stop { get; set; }
    }
}
