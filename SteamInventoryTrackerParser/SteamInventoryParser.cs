using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SteamInventoryTrackerParser
{
    public class SteamInventoryParser
    {
        public async Task<List<InventoryItem>> ParseInventoryAsync(string steamId, int appId)
        {
            string url = $"https://steamcommunity.com/inventory/{steamId}/{appId}/2?l=english&count=5000";

            try
            {
                var response = await new HttpClient().GetStringAsync(url);
                var data = JObject.Parse(response);

                if (data["success"]?.Value<bool>() != true)
                    throw new Exception("Inventory is private or unavailable");

                return ProcessItems(data);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to parse inventory: {ex.Message}");
            }
        }

        private List<InventoryItem> ProcessItems(JObject data)
        {
            var items = new Dictionary<string, InventoryItem>();
            var assets = data["assets"] as JArray;
            var descriptions = data["descriptions"] as JArray;

            foreach (var asset in assets)
            {
                string classId = asset["classid"]?.ToString();
                var desc = descriptions?.FirstOrDefault(d => d["classid"]?.ToString() == classId);
                string itemName = desc?["name"]?.ToString() ?? "Unknown Item";

                if (items.ContainsKey(itemName))
                {
                    items[itemName].Count++;
                }
                else
                {
                    items[itemName] = new InventoryItem
                    {
                        Name = itemName,
                        Count = 1,
                        ClassId = classId
                    };
                }
            }

            return items.Values.OrderBy(i => i.Name).ToList();
        }
    }
}