using Newtonsoft.Json;
using System.IO;

namespace Benner.Messaging.Configuration
{
    public abstract class JsonConfiguration
    {
        public abstract string FileName { get; }

        public static T LoadConfiguration<T>() where T : JsonConfiguration, new()
        {
            return new T().LoadConfigurationFromFile<T>() as T;
        }

        protected virtual object LoadConfigurationFromFile<T>()
        {
            return LoadConfigurationFromFile<T>(FileName);
        }

        protected static object LoadConfigurationFromFile<T>(string fileName)
        {
            return LoadConfigurationFromFile<T>(DirectoryHelper.GetExecutingDirectoryName(), fileName);
        }

        protected static object LoadConfigurationFromFile<T>(string path, string fileName)
        {
            string configPath = Path.Combine(path, fileName);

            if (File.Exists(configPath))
            {
                var configJson = File.ReadAllText(configPath);

                if (!string.IsNullOrWhiteSpace(configJson))
                    return JsonParser.Deserialize<T>(configJson, new JsonSerializerSettings());
            }
            return null;
        }

        public virtual void SaveConfigurationToFile()
        {
            SaveConfigurationToFile(FileName);
        }

        protected void SaveConfigurationToFile(string fileName)
        {
            SaveConfigurationToFile(DirectoryHelper.GetExecutingDirectoryName(), fileName);
        }

        protected void SaveConfigurationToFile(string path, string fileName)
        {
            string json = JsonParser.Serialize(this, new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            });
            File.WriteAllText(Path.Combine(path, fileName), json);
        }
    }
}
