using System;

namespace TetriON.Shared.Models {
    /// <summary>
    /// User account data model
    /// </summary>
    public class UserAccount {
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public required PlayerStats Stats { get; set; }
        public required PlayerConfig Config { get; set; }
    }

    public class PlayerConfig {
        public Dictionary<string, Configuration>? SettingsBySection { get; set; }
    }

    public class Configuration {
        public Dictionary<string, string> Settings { get; set; }
        public Type ConfigType { get; set; }

        public Configuration() {
            Settings = [];
        }

        public T GetConfigAs<T>() where T : new() {
            var configInstance = new T();
            var configType = typeof(T);

            foreach (var setting in Settings) {
                var property = configType.GetProperty(setting.Key);
                if (property != null && property.CanWrite) {
                    var convertedValue = Convert.ChangeType(setting.Value, property.PropertyType);
                    property.SetValue(configInstance, convertedValue);
                }
            }

            return configInstance;
        }

        public void LoadFromConfig<T>(T configInstance) {
            var configType = typeof(T);
            Settings.Clear();

            foreach (var property in configType.GetProperties()) {
                if (property.CanRead) {
                    var value = property.GetValue(configInstance);
                    Settings[property.Name] = value?.ToString() ?? string.Empty;
                }
            }
        }
    }
}
