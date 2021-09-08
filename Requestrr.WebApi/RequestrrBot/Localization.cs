using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Requestrr.WebApi.RequestrrBot
{
    public class Localization
    {
        private static object _lock = new object();
        private static dynamic _cachedText = null;
        private static string _languageFilePath = "config/local/";
        public static dynamic Read(string languageCode)
        {
            dynamic texts = null;

            lock(_lock)
            {
                if (_cachedText == null)
                {
                    _cachedText = JObject.Parse(File.ReadAllText($"{_languageFilePath}{languageCode}.json"));
                }
            }

            texts = _cachedText;

            return texts;
        }
    }
}