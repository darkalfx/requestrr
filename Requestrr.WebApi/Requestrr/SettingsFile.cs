using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Requestrr.WebApi
{
    public static class SettingsFile
    {
        private static object _lock = new object();

        public const string FilePath = "config/settings.json";

        public static dynamic _cachedSettings = null;

        public static dynamic Read()
        {
            dynamic settings = null;

            lock (_lock)
            {
                if(_cachedSettings == null)
                {
                    _cachedSettings = JObject.Parse(File.ReadAllText(FilePath));
                }

                settings = _cachedSettings;
            }

            return settings;
        }

        public static void Write(Action<dynamic> modifyFunc)
        {
            lock (_lock)
            {
                modifyFunc(_cachedSettings);
                File.WriteAllText(FilePath, JsonConvert.SerializeObject(_cachedSettings));
            }
        }
    }
}