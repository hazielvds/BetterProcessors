using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Newtonsoft.Json;
using System.Linq;

namespace BetterProcessors
{
    public class FileHandler
    {
        public List<Trader> TraderList = new List<Trader>();
        public void LoadProcessors()
        {
            List<Trader> list = new List<Trader>();

            if (!Directory.Exists(Path.Combine("Plugins", "BetterProcessors", "Traders"))) Directory.CreateDirectory(Path.Combine("Plugins", "BetterProcessors", "Traders"));

            string[] ProcessorsFiles = Directory.GetFiles(Path.Combine("Plugins", "BetterProcessors", "Traders"), "*.json");
            if(ProcessorsFiles.Length < 1)
            {
                Trader newTrader = new Trader()
                {
                    Id = "TraderExample",
                    Name = "Trader Name",
                    Jobs = new string[]
                    {
                        "Police",
                        "Paramedic"
                    },
                    Permission = "trader.TraderExample",
                    Trades = new List<Trade>()
                    {
                        new Trade()
                        {
                            Id = "TradeExmp",
                            Display = "Trade example",
                            Input = new Dictionary<string, int>()
                            {
                                { "Money", 10 }
                            },
                            Output = new Dictionary<string, int>()
                            {
                                { "Orange", 10 }
                            },
                            Time = 100,
                            Delay = 0
                        }
                    }
                };

                string j = JsonConvert.SerializeObject(newTrader, Formatting.Indented);
                File.WriteAllText(Path.Combine("Plugins", "BetterProcessors", "Traders", "Example.json"), j);
                ProcessorsFiles.Append("Plugins/BetterProcessors/Traders/Example.json");
            }
            foreach(string file in ProcessorsFiles)
            {
                try
                {
                    string content = File.ReadAllText(file);

                    Trader trader = JsonConvert.DeserializeObject<Trader>(content);

                    list.Add(trader);
                } 
                catch (Exception ex)
                {
                    Debug.Log($"Error al procesar el archivo '{file}': {ex.Message}");
                }
            }

            TraderList = list;
        }
    }
    public class Trader
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string[] Jobs { get; set; }
        public string Permission { get; set; }
        public List<Trade> Trades {get; set;}
    }

    public class Trade
    {
        public string Id { get; set; }
        public string Display { get; set; }
        public Dictionary<string, int> Input { get; set; }
        public Dictionary<string, int> Output { get; set; }
        public int Time { get; set; }
        public int Delay { get; set; }
    }
}
