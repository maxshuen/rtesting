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
                Console.WriteLine("Usage: MT4BackTester.exe <json_file> <output_folder>");
                return;
            }

            string jsonFile = args[0];
            string outputFolder = args[1];

            if (!File.Exists(jsonFile))
            {
                Console.WriteLine($"Json file not found: {jsonFile}");
                return;
            }

            try
            {
                string json = File.ReadAllText(jsonFile);
                var settings = JsonConvert.DeserializeObject<BacktestSettings>(json);

                RunBacktest(settings, outputFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing json file {jsonFile}: {ex.Message}");
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
            sb.AppendLine("TestGraph=true"); // Include graph in the report

            // Add expert advisor parameters
            foreach (var param in settings.Parameters)
            {
                sb.AppendLine($"{param.Key}={param.Value}");
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

                // Save the settings
                string settingsPath = Path.Combine(outputFolder, $"{reportFileName}.set");
                using (var writer = new StreamWriter(settingsPath))
                {
                    foreach (var param in settings.Parameters)
                    {
                        writer.WriteLine($"{param.Key}={param.Value}");
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

        static Dictionary<string, string> GetOptimizedParametersFromReport(string report)
        {
            var parameters = new Dictionary<string, string>();
            try
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(report);
                var table = doc.DocumentNode.SelectSingleNode("//table[.//th[text()='Inputs']]");
                if (table != null)
                {
                    var rows = table.SelectNodes(".//tr[td]");
                    if (rows != null)
                    {
                        foreach (var row in rows)
                        {
                            var cells = row.SelectNodes("td");
                            if (cells != null && cells.Count >= 2)
                            {
                                parameters.Add(cells[0].InnerText, cells[1].InnerText);
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignore errors
            }
            return parameters;
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
    }
}
