using System;
using System.IO;
using System.Linq;
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

        public class Settings
        {
            public string ExportTemplate { get; set; }
            public string BarcodePort { get; set; }
        }


        public AppSettingsManager(ICategoryService categoryService, FilePath filePath)
        {
            _categoryService = categoryService;
            Path = filePath.Path;
        }


        public void UpdateAppSettingsNode(string[] path, object newValue)
        {
            var keyPath = path.ToList();

            if (keyPath == null || keyPath.Count == 0)
                throw new ArgumentException("Key path must not be empty.");

            var text = File.Exists(Path) ? File.ReadAllText(Path) : "{}";

            JObject root;
            try
            {
                root = JObject.Parse(text);
            }
            catch
            {
                root = new JObject();
            }

            if (!(root["AppSettings"] is JObject currentNode))
            {
                currentNode = new JObject();
                root["AppSettings"] = currentNode;
            }

            for (int i = 0; i < keyPath.Count - 1; i++)
            {
                var key = keyPath[i];

                if (!(currentNode[key] is JObject nextNode))
                {
                    nextNode = new JObject();
                    currentNode[key] = nextNode;
                }

                currentNode = nextNode;
            }

            var lastKey = keyPath[keyPath.Count - 1];
            currentNode[lastKey] = newValue != null ? JToken.FromObject(newValue) : JValue.CreateNull();

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
                    IdColumn = false,
                    BatchColumn = false,
                    ExpiryDateColumn = true,
                    SupplierColumn = true,
                };
            });

            var inventoryArray = root["Inventory"] as JArray ?? new JArray();
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
                AddPropertyIfMissing(nameof(newSettings.IdColumn), newSettings.IdColumn);
                AddPropertyIfMissing(nameof(newSettings.BatchColumn), newSettings.BatchColumn);
                AddPropertyIfMissing(nameof(newSettings.ExpiryDateColumn), newSettings.ExpiryDateColumn);
                AddPropertyIfMissing(nameof(newSettings.SupplierColumn), newSettings.SupplierColumn);
            }

            root["Inventory"] = inventoryArray;

        }
    }
}
