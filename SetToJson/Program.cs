using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace SetToJson
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: SetToJson.exe <set_file> <json_file>");
                return;
            }

            string setFile = args[0];
            string jsonFile = args[1];

            if (!File.Exists(setFile))
            {
                Console.WriteLine($"Set file not found: {setFile}");
                return;
            }

            var parameters = new Dictionary<string, string>();
            var lines = File.ReadAllLines(setFile);
            foreach (var line in lines)
            {
                var parts = line.Split('=');
                if (parts.Length == 2)
                {
                    parameters.Add(parts[0], parts[1]);
                }
            }

            var settings = new BacktestSettings
            {
                ExpertAdvisor = "AcePhoenixStd3.71",
                Symbol = "EURUSD",
                Period = 15,
                FromDate = new DateTime(2023, 1, 1),
                ToDate = new DateTime(2023, 12, 31),
                Optimization = false,
                Parameters = new Dictionary<string, object>()
            };

            foreach (var param in parameters)
            {
                settings.Parameters.Add(param.Key, param.Value);
            }

            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(jsonFile, json);

            Console.WriteLine($"Successfully converted {setFile} to {jsonFile}");
        }
    }

    public class BacktestSettings
    {
        public string ExpertAdvisor { get; set; }
        public string Symbol { get; set; }
        public int Period { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool Optimization { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
    }
}
