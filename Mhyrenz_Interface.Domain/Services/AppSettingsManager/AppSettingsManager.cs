using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.CategoryService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mhyrenz_Interface.Domain.Services.AppSettingsManager
{
    public class AppSettingsManager
    {
        private readonly ICategoryService _categoryService;

        public string Path { get; }

        public class FilePath
        {
            public string Path { get; }
            public FilePath(string path)
            {
                Path = path;
            }
        }

        public class AppSettings
        {
            public string ExportTemplate { get; set; }
            public string BarcodePort { get; set; }
        }


        public AppSettingsManager(ICategoryService categoryService, FilePath filePath)
        {
            _categoryService = categoryService;
            Path = filePath.Path;
        }


        public void UpdateAppSettingsNode<T>(Action<T> updater, string section = null)
        {
            var text = File.Exists(Path) ? File.ReadAllText(Path) : "{}";

            var rootObj = JObject.Parse(text)[section ?? typeof(T).Name] ?? throw new InvalidOperationException($"Section '{typeof(T).Name}' not found.");
            var config = rootObj.ToObject<T>();

            updater(config);

            var root = JObject.Parse(text);
            root[section ?? typeof(T).Name] = JToken.FromObject(config);
            File.WriteAllText(Path, root.ToString(Formatting.Indented));
        }



        public async Task GenerateAppSettings()
        {
            JObject root;
            if (File.Exists(Path))
            {
                var text = File.ReadAllText(Path);
                root = JObject.Parse(text);
            }
            else
            {
                root = new JObject();
            }

            if (!(root["AppSettings"] is JObject appSettings))
            {
                appSettings = new JObject();
                root["AppSettings"] = appSettings;
            }

            await GenerateInventory(root);

            File.WriteAllText(Path, root.ToString(Formatting.Indented));
        }
        public async Task GenerateInventory(JObject root)
        {
            var categories = await _categoryService.GetAllCategories(); // REFACTOR: INJECT INSTEAD AFTER LOAD

            var categoryDict = categories.Select(c =>
            {
                // DEFAULTS
                return new InventorySettings
                {
                    Id = c.Id,
                    Name = c.Name,
                };
            });

            var inventoryArray = root["InventorySettings"] as JArray ?? new JArray();
            var inventoryDict = inventoryArray
                .OfType<JObject>()
                .Where(obj => obj["Id"] != null) // CONSIDER FOR NAME AS THE KEY AS WELL
                .ToDictionary(
                    obj => (int)obj["Id"],
                    obj => obj
                );

            foreach (var newSettings in categoryDict)
            {
                var isExisting = inventoryDict.TryGetValue(newSettings.Id, out var categoryNode);
                if (!isExisting)
                {
                    categoryNode = new JObject();
                    inventoryArray.Add(categoryNode); // only append new node if not exisiting
                }

                void AddPropertyIfMissing(string propName, object val)
                {
                    if (!categoryNode.TryGetValue(propName, out var _))
                        categoryNode[propName] = JToken.FromObject(val);
                }

                AddPropertyIfMissing(nameof(newSettings.Id), newSettings.Id);
                AddPropertyIfMissing(nameof(newSettings.Name), newSettings.Name);
            }

            root["InventorySettings"] = inventoryArray;

        }
    }
}
