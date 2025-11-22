using DocumentFormat.OpenXml.Wordprocessing;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.CategoryService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static Mhyrenz_Interface.Domain.Services.AppSettingsManager.AppSettingsManager;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
            public Dictionary<string, InventorySettings> Inventory { get; set; }
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

            await GenerateInventory(appSettings);

            File.WriteAllText(Path, root.ToString(Formatting.Indented));
        }
        private async Task GenerateInventory(JObject appSettings)
        {
            var categories = await _categoryService.GetAllCategories();

            var categoryDict = categories.Select(c =>
            {
                return new InventorySettings
                {
                    Name = c.Name,
                    Id = c.Id,
                    IdColumn = false,
                    BatchColumn = false,
                    ExpiryDateColumn = true,
                    SupplierColumn = true,
                };
            });

            if (!(appSettings["Inventory"] is JObject inventoryNode))
            {
                inventoryNode = new JObject();
                appSettings["Inventory"] = inventoryNode;
            }

            foreach (var newSettings in categoryDict)
            {
                var categoryId = newSettings.Id.ToString();

                JToken existingNode = inventoryNode[categoryId];
                JObject categoryNode;

                if (existingNode is JObject existingJObject)
                {
                    categoryNode = existingJObject;
                }
                else
                {
                    categoryNode = new JObject();
                    inventoryNode[categoryId] = categoryNode;  // Attach new JObject to parent
                }

                void AddPropertyIfMissing(string propName, object val, bool force = false)
                {
                    if (!categoryNode.TryGetValue(propName, out var _) || force)
                    {
                        categoryNode[propName] = JToken.FromObject(val);
                    }
                }

                AddPropertyIfMissing(nameof(newSettings.Name), newSettings.Name);
                if (newSettings.GenericColumn.HasValue)
                    AddPropertyIfMissing("GenericColumn", newSettings.GenericColumn.Value);
                AddPropertyIfMissing(nameof(newSettings.IdColumn), newSettings.IdColumn);
                AddPropertyIfMissing(nameof(newSettings.BatchColumn), newSettings.BatchColumn);
                AddPropertyIfMissing(nameof(newSettings.ExpiryDateColumn), newSettings.ExpiryDateColumn);
                AddPropertyIfMissing(nameof(newSettings.SupplierColumn), newSettings.SupplierColumn);

            }

        }
    }
}
