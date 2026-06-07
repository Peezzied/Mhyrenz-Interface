using System;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Wordprocessing;
using Mhyrenz_Interface.Domain.Models;
using Mhyrenz_Interface.Domain.Services.CategoryService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Mhyrenz_Interface.Domain.Services.Settings
{
    public class ConfigManager<T> where T: new()
    {
        public string Path { get; }

        public ConfigManager(string path)
        {
            Path = path;
        }

        public void Save(T settings)
        {
            var name = typeof(T).Name;
            var root = EnsureRoot();

            root[name] = JToken.FromObject(settings);

            Write(root);
        }

        public void GenerateConfig(string name)
        {
            var root = EnsureRoot();

            if (!(root[name] is JObject))
            {
                var appSettings = JToken.FromObject(new T());
                root[name] = appSettings;
            }

            Write(root);
        }

        private void Write(JObject root)
        {
            File.WriteAllText(Path, root.ToString(Formatting.Indented));
        }

        private JObject EnsureRoot()
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

            return root;
        }
    }
}
