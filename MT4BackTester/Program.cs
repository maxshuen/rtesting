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
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: MT4BackTester.exe <settings_folder> <output_folder>");
                return;
            }

            string settingsFolder = args[0];
            string outputFolder = args[1];
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

                    RunBacktest(settings, outputFolder);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing setting file {settingFile}: {ex.Message}");
                }
            }
        }

        static void RunBacktest(BacktestSettings settings, string outputFolder)
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
            sb.AppendLine($"Report={Path.Combine(outputFolder, reportFileName)}");
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
                var stopWatch = new Stopwatch();
                stopWatch.Start();

                using (Process process = Process.Start(startInfo))
                {
                    while (!process.HasExited)
                    {
                        Console.Write($"\rElapsed time: {stopWatch.Elapsed:hh\\:mm\\:ss}");
                        System.Threading.Thread.Sleep(1000);
                    }
                }
                stopWatch.Stop();
                Console.WriteLine($"\nBacktest for {settings.ExpertAdvisor} on {settings.Symbol} completed in {stopWatch.Elapsed:hh\\:mm\\:ss}.");

                // Save the optimized settings and summary
                if (settings.Optimization)
                {
                    string optimizedSettingsPath = Path.Combine(outputFolder, $"{reportFileName}.json");
                    string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
                    File.WriteAllText(optimizedSettingsPath, json);

                    string summaryPath = Path.Combine(outputFolder, "summary.csv");
                    bool fileExists = File.Exists(summaryPath);
                    using (var writer = new StreamWriter(summaryPath, true))
                    {
                        if (!fileExists)
                        {
                            writer.WriteLine("Set,Source,CCY,Frequency,Profit,Draw Down,Trades,Profit Factor,Backtest Dates," + string.Join(",", settings.Parameters.Keys));
                        }

                        string reportPath = Path.Combine(outputFolder, $"{reportFileName}.htm");
                        if (File.Exists(reportPath))
                        {
                            var report = new StreamReader(reportPath).ReadToEnd();
                            var profit = GetValueFromReport(report, "Total net profit");
                            var drawdown = GetValueFromReport(report, "Maximal drawdown");
                            var trades = GetValueFromReport(report, "Total trades");
                            var profitFactor = GetValueFromReport(report, "Profit factor");

                            writer.WriteLine($"{reportFileName},Preset A,{settings.Symbol},H1,{profit},{drawdown},{trades},{profitFactor},{settings.FromDate:yyyyMMdd}-{settings.ToDate:yyyyMMdd}," + string.Join(",", settings.Parameters.Values));
                        }
                    }
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

        static string GetValueFromReport(string report, string key)
        {
            try
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(report);
                var node = doc.DocumentNode.SelectSingleNode($"//td[contains(text(),'{key}')]/following-sibling::td");
                return node?.InnerText;
            }
            catch
            {
                return "N/A";
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
