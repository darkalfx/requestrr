using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Requestrr.WebApi
{
    public static class SettingsFileUpgrader
    {
        public static void Upgrade(string settingsFilePath)
        {
            dynamic settingsJson = JObject.Parse(File.ReadAllText(settingsFilePath));

            if (settingsJson.Version.ToString().Equals("1.0.0", StringComparison.InvariantCultureIgnoreCase))
            {
                var botClientJson = settingsJson["BotClient"] as JObject;

                var monitoredChannels = !string.IsNullOrWhiteSpace(botClientJson.GetValue("MonitoredChannels").ToString())
                    ? botClientJson.GetValue("MonitoredChannels").ToString().Split(" ").Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim())
                    : Array.Empty<string>();

                ((JObject)settingsJson["ChatClients"]["Discord"]).Add("MonitoredChannels", JToken.FromObject(monitoredChannels));
                ((JObject)settingsJson["BotClient"]).Remove("MonitoredChannels");

                ((JObject)settingsJson["ChatClients"]["Discord"]).Add("TvShowRoles", JToken.FromObject(Array.Empty<string>()));
                ((JObject)settingsJson["ChatClients"]["Discord"]).Add("MovieRoles", JToken.FromObject(Array.Empty<string>()));

                settingsJson.ChatClients.Discord.EnableDirectMessageSupport = false;

                ((JObject)settingsJson["DownloadClients"]["Ombi"]).Add("BaseUrl", string.Empty);

                ((JObject)settingsJson["DownloadClients"]["Radarr"]).Add("BaseUrl", string.Empty);
                ((JObject)settingsJson["DownloadClients"]["Radarr"]).Add("SearchNewRequests", true);
                ((JObject)settingsJson["DownloadClients"]["Radarr"]).Add("MonitorNewRequests", true);

                ((JObject)settingsJson["DownloadClients"]["Sonarr"]).Add("BaseUrl", string.Empty);
                ((JObject)settingsJson["DownloadClients"]["Sonarr"]).Add("SearchNewRequests", true);
                ((JObject)settingsJson["DownloadClients"]["Sonarr"]).Add("MonitorNewRequests", true);

                settingsJson.Version = "1.0.1";
                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(settingsJson));
            }

            if (settingsJson.Version.ToString().Equals("1.0.1", StringComparison.InvariantCultureIgnoreCase)
            || settingsJson.Version.ToString().Equals("1.0.2", StringComparison.InvariantCultureIgnoreCase)
            || settingsJson.Version.ToString().Equals("1.0.3", StringComparison.InvariantCultureIgnoreCase)
            || settingsJson.Version.ToString().Equals("1.0.4", StringComparison.InvariantCultureIgnoreCase))
            {
                settingsJson.Version = "1.0.5";
                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(settingsJson));
            }

            if (settingsJson.Version.ToString().Equals("1.0.5", StringComparison.InvariantCultureIgnoreCase))
            {
                settingsJson.Version = "1.0.6";
                ((JObject)settingsJson).Add("Port", 5060);
                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(settingsJson));
            }

            if (settingsJson.Version.ToString().Equals("1.0.6", StringComparison.InvariantCultureIgnoreCase))
            {
                settingsJson.Version = "1.0.9";

                ((JObject)settingsJson["ChatClients"]["Discord"]).Add("AutomaticallyNotifyRequesters", true);
                ((JObject)settingsJson["ChatClients"]["Discord"]).Add("NotificationMode", "PrivateMessages");
                ((JObject)settingsJson["ChatClients"]["Discord"]).Add("NotificationChannels", JToken.FromObject(Array.Empty<int>()));
                ((JObject)settingsJson["ChatClients"]["Discord"]).Add("AutomaticallyPurgeCommandMessages", false);
                ((JObject)settingsJson["ChatClients"]["Discord"]).Add("DisplayHelpCommandInDMs", true);
                ((JObject)settingsJson["ChatClients"]["Discord"]).Add("EnableRequestsThroughDirectMessages", (bool)((JObject)settingsJson["ChatClients"]["Discord"]).GetValue("EnableDirectMessageSupport"));
                ((JObject)settingsJson["ChatClients"]["Discord"]).Remove("EnableDirectMessageSupport");

                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(settingsJson));
            }

            if (settingsJson.Version.ToString().Equals("1.0.9", StringComparison.InvariantCultureIgnoreCase))
            {
                settingsJson.Version = "1.10.0";
                ((JObject)settingsJson["TvShows"]).Add("Restrictions", "None");

                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(settingsJson));
            }

            if (settingsJson.Version.ToString().Equals("1.10.0", StringComparison.InvariantCultureIgnoreCase))
            {
                settingsJson.Version = "1.11.0";
                ((JObject)settingsJson).Add("BaseUrl", string.Empty);

                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(settingsJson));
            }
        }
    }
}