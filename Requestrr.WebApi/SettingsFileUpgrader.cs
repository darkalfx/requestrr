using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Requestrr.WebApi.config;
using Requestrr.WebApi.Controllers.DownloadClients.Ombi;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Overseerr;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Sonarr;

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

            if (settingsJson.Version.ToString().Equals("1.11.0", StringComparison.InvariantCultureIgnoreCase))
            {
                settingsJson.Version = "1.12.0";

                ((JObject)settingsJson["DownloadClients"]).Add("Overseerr", JToken.FromObject(new
                {
                    Hostname = string.Empty,
                    Port = 5055,
                    UseSSL = false,
                    ApiKey = string.Empty,
                    DefaultApiUserID = string.Empty,
                    Version = "1"
                }));

                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(settingsJson));
            }

            if (settingsJson.Version.ToString().Equals("1.12.0", StringComparison.InvariantCultureIgnoreCase))
            {
                settingsJson.Version = "1.13.0";

                ((JObject)settingsJson["ChatClients"]).Add("Language", "english");

                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(settingsJson));
            }

            if (settingsJson.Version.ToString().Equals("1.13.0", StringComparison.InvariantCultureIgnoreCase))
            {
                settingsJson.Version = "1.14.0";

                if (((JObject)settingsJson["Movies"]).TryGetValue("Command", StringComparison.InvariantCultureIgnoreCase, out _))
                {
                    ((JObject)settingsJson["ChatClients"]["Discord"]).Remove("DisplayHelpCommandInDMs");
                    ((JObject)settingsJson["BotClient"]).Remove("CommandPrefix");

                    ((JObject)settingsJson["ChatClients"]["Discord"]).Remove("TvShowRoles");
                    ((JObject)settingsJson["ChatClients"]["Discord"]).Add("TvShowRoles", JToken.FromObject(Array.Empty<int>()));

                    ((JObject)settingsJson["ChatClients"]["Discord"]).Remove("MovieRoles");
                    ((JObject)settingsJson["ChatClients"]["Discord"]).Add("MovieRoles", JToken.FromObject(Array.Empty<int>()));

                    ((JObject)settingsJson["ChatClients"]["Discord"]).Remove("NotificationChannels");
                    ((JObject)settingsJson["ChatClients"]["Discord"]).Add("NotificationChannels", JToken.FromObject(Array.Empty<int>()));

                    ((JObject)settingsJson["ChatClients"]["Discord"]).Remove("MonitoredChannels");
                    ((JObject)settingsJson["ChatClients"]["Discord"]).Add("MonitoredChannels", JToken.FromObject(Array.Empty<int>()));

                    ((JObject)settingsJson["Movies"]).Remove("Command");
                    ((JObject)settingsJson["TvShows"]).Remove("Command");
                }

                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(settingsJson));
            }

            if (settingsJson.Version.ToString().Equals("1.14.0", StringComparison.InvariantCultureIgnoreCase))
            {
                settingsJson.Version = "1.15.0";

                ((JObject)settingsJson).Add("DisableAuthentication", false);

                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(settingsJson));
            }

            if (settingsJson.Version.ToString().Equals("1.15.0", StringComparison.InvariantCultureIgnoreCase))
            {
                settingsJson.Version = "2.0.0";

                var radarrCategories = new[]
                {
                    new
                    {
                        Id = 0,
                        Name = "movie",
                        ProfileId = (int)((JObject)settingsJson["DownloadClients"]["Radarr"]).GetValue("MovieProfileId"),
                        RootFolder = (string)((JObject)settingsJson["DownloadClients"]["Radarr"]).GetValue("MovieRootFolder"),
                        MinimumAvailability = (string)((JObject)settingsJson["DownloadClients"]["Radarr"]).GetValue("MovieMinimumAvailability"),
                        Tags = ((JObject)settingsJson["DownloadClients"]["Radarr"]).GetValue("MovieTags").ToObject<int[]>()
                    },
                    new
                    {
                        Id = 1,
                        Name = "movie-anime",
                        ProfileId = (int)((JObject)settingsJson["DownloadClients"]["Radarr"]).GetValue("AnimeProfileId"),
                        RootFolder = (string)((JObject)settingsJson["DownloadClients"]["Radarr"]).GetValue("AnimeRootFolder"),
                        MinimumAvailability = (string)((JObject)settingsJson["DownloadClients"]["Radarr"]).GetValue("AnimeMinimumAvailability"),
                        Tags = ((JObject)settingsJson["DownloadClients"]["Radarr"]).GetValue("AnimeTags").ToObject<int[]>()
                    },
                };

                ((JObject)settingsJson["DownloadClients"]["Radarr"]).Remove("MovieProfileId");
                ((JObject)settingsJson["DownloadClients"]["Radarr"]).Remove("MovieRootFolder");
                ((JObject)settingsJson["DownloadClients"]["Radarr"]).Remove("MovieMinimumAvailability");
                ((JObject)settingsJson["DownloadClients"]["Radarr"]).Remove("MovieTags");
                ((JObject)settingsJson["DownloadClients"]["Radarr"]).Remove("AnimeProfileId");
                ((JObject)settingsJson["DownloadClients"]["Radarr"]).Remove("AnimeRootFolder");
                ((JObject)settingsJson["DownloadClients"]["Radarr"]).Remove("AnimeMinimumAvailability");
                ((JObject)settingsJson["DownloadClients"]["Radarr"]).Remove("AnimeTags");

                ((JObject)settingsJson["DownloadClients"]["Radarr"]).Add("Categories", JToken.FromObject(radarrCategories));

                var sonarrCategories = new[]
                {
                    new
                    {
                        Id = 0,
                        Name = "tv",
                        ProfileId = (int)((JObject)settingsJson["DownloadClients"]["Sonarr"]).GetValue("TvProfileId"),
                        RootFolder = (string)((JObject)settingsJson["DownloadClients"]["Sonarr"]).GetValue("TvRootFolder"),
                        Tags = ((JObject)settingsJson["DownloadClients"]["Sonarr"]).GetValue("TvTags").ToObject<int[]>(),
                        LanguageId = (int)((JObject)settingsJson["DownloadClients"]["Sonarr"]).GetValue("TvLanguageId"),
                        UseSeasonFolders = (bool)((JObject)settingsJson["DownloadClients"]["Sonarr"]).GetValue("TvUseSeasonFolders"),
                        SeriesType = "standard",
                    },
                    new
                    {
                        Id = 1,
                        Name = "tv-anime",
                        ProfileId = (int)((JObject)settingsJson["DownloadClients"]["Sonarr"]).GetValue("AnimeProfileId"),
                        RootFolder = (string)((JObject)settingsJson["DownloadClients"]["Sonarr"]).GetValue("AnimeRootFolder"),
                        Tags = ((JObject)settingsJson["DownloadClients"]["Sonarr"]).GetValue("AnimeTags").ToObject<int[]>(),
                        LanguageId = (int)((JObject)settingsJson["DownloadClients"]["Sonarr"]).GetValue("AnimeLanguageId"),
                        UseSeasonFolders = (bool)((JObject)settingsJson["DownloadClients"]["Sonarr"]).GetValue("AnimeUseSeasonFolders"),
                        SeriesType = "anime",
                    },
                };

                ((JObject)settingsJson["DownloadClients"]["Sonarr"]).Remove("TvProfileId");
                ((JObject)settingsJson["DownloadClients"]["Sonarr"]).Remove("TvRootFolder");
                ((JObject)settingsJson["DownloadClients"]["Sonarr"]).Remove("TvTags");
                ((JObject)settingsJson["DownloadClients"]["Sonarr"]).Remove("TvLanguageId");
                ((JObject)settingsJson["DownloadClients"]["Sonarr"]).Remove("TvUseSeasonFolders");

                ((JObject)settingsJson["DownloadClients"]["Sonarr"]).Remove("AnimeProfileId");
                ((JObject)settingsJson["DownloadClients"]["Sonarr"]).Remove("AnimeRootFolder");
                ((JObject)settingsJson["DownloadClients"]["Sonarr"]).Remove("AnimeTags");
                ((JObject)settingsJson["DownloadClients"]["Sonarr"]).Remove("AnimeLanguageId");
                ((JObject)settingsJson["DownloadClients"]["Sonarr"]).Remove("AnimeUseSeasonFolders");

                ((JObject)settingsJson["DownloadClients"]["Sonarr"]).Add("Categories", JToken.FromObject(sonarrCategories));

                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(settingsJson));
            }

            if (settingsJson.Version.ToString().Equals("2.0.0", StringComparison.InvariantCultureIgnoreCase))
            {
                settingsJson.Version = "2.1.0";

                var defaultApiUserID = (string)((JObject)settingsJson["DownloadClients"]["Overseerr"]).GetValue("DefaultApiUserID");
                ((JObject)settingsJson["DownloadClients"]["Overseerr"]).Remove("DefaultApiUserID");

                ((JObject)settingsJson["DownloadClients"]["Overseerr"]).Add("Movies", JToken.FromObject(new { DefaultApiUserId = defaultApiUserID, Categories = Array.Empty<object>() }));
                ((JObject)settingsJson["DownloadClients"]["Overseerr"]).Add("TvShows", JToken.FromObject(new { DefaultApiUserId = defaultApiUserID, Categories = Array.Empty<object>() }));

                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(settingsJson));
            }

            if (settingsJson.Version.ToString().Equals("2.1.0", StringComparison.InvariantCultureIgnoreCase))
            {
                settingsJson.Version = "2.2.0";

                var sonarrSettings = ((JObject)settingsJson["DownloadClients"]["Sonarr"]).ToObject<dynamic>();
                var radarrSettings = ((JObject)settingsJson["DownloadClients"]["Radarr"]).ToObject<dynamic>();
                var overseerrSettings = ((JObject)settingsJson["DownloadClients"]["Overseerr"]).ToObject<dynamic>();
                var ombiSettings = ((JObject)settingsJson["DownloadClients"]["Ombi"]).ToObject<dynamic>();

                var movieSettings = ((JObject)settingsJson).GetValue("Movies").ToObject<dynamic>();
                var tvShowSettings = ((JObject)settingsJson).GetValue("TvShows").ToObject<dynamic>();

                var downloadClients = new List<dynamic>();

                if (movieSettings.Client == "Radarr")
                {
                    downloadClients.Add(
                        new
                        {
                            ClientType = "RadarrDownloadClientSettings",
                            Id = 1,
                            Name = "Radarr",
                            ApiKey = radarrSettings.ApiKey,
                            BaseUrl = radarrSettings.BaseUrl,
                            Hostname = radarrSettings.Hostname,
                            MonitorNewRequests = radarrSettings.MonitorNewRequests,
                            SearchNewRequests = radarrSettings.MonitorNewRequests,
                            Port = radarrSettings.Port,
                            UseSSL = radarrSettings.UseSSL,
                            Version = radarrSettings.Version
                        });
                }
                else if (movieSettings.Client == "Overseerr")
                {
                    downloadClients.Add(
                        new
                        {
                            ClientType = "OverseerrDownloadClientSettings",
                            Id = 2,
                            Name = "Overseerr",
                            ApiKey = overseerrSettings.ApiKey,
                            Hostname = overseerrSettings.Hostname,
                            Port = overseerrSettings.Port,
                            UseSSL = overseerrSettings.UseSSL,
                            Version = overseerrSettings.Version
                        });
                }
                else if (movieSettings.Client == "Ombi")
                {
                    downloadClients.Add(
                        new
                        {
                            ClientType = "OmbiDownloadClientSettings",
                            Id = 3,
                            Name = "Ombi",
                            ApiKey = ombiSettings.ApiKey,
                            BaseUrl = ombiSettings.BaseUrl,
                            Hostname = ombiSettings.Hostname,
                            Port = ombiSettings.Port,
                            UseSSL = ombiSettings.UseSSL,
                            Version = ombiSettings.Version
                        });
                }

                if (tvShowSettings.Client == "Sonarr")
                {
                    downloadClients.Add(
                        new
                        {
                            ClientType = "SonarrDownloadClientSettings",
                            Id = 4,
                            Name = "Sonarr",
                            ApiKey = sonarrSettings.ApiKey,
                            BaseUrl = sonarrSettings.BaseUrl,
                            Hostname = sonarrSettings.Hostname,
                            MonitorNewRequests = sonarrSettings.MonitorNewRequests,
                            SearchNewRequests = sonarrSettings.MonitorNewRequests,
                            Port = sonarrSettings.Port,
                            UseSSL = sonarrSettings.UseSSL,
                            Version = sonarrSettings.Version
                        });
                }
                else if (tvShowSettings.Client == "Overseerr" && downloadClients.All(x => x.ClientType != "OverseerrDownloadClientSettings"))
                {
                    downloadClients.Add(
                        new
                        {
                            ClientType = "OverseerrDownloadClientSettings",
                            Id = 5,
                            Name = "Overseerr",
                            ApiKey = overseerrSettings.ApiKey,
                            Hostname = overseerrSettings.Hostname,
                            Port = overseerrSettings.Port,
                            UseSSL = overseerrSettings.UseSSL,
                            Version = overseerrSettings.Version
                        });
                }
                else if (tvShowSettings.Client == "Ombi" && downloadClients.All(x => x.ClientType != "OmbiDownloadClientSettings"))
                {
                    downloadClients.Add(
                        new
                        {
                            ClientType = "OmbiDownloadClientSettings",
                            Id = 6,
                            Name = "Ombi",
                            ApiKey = ombiSettings.ApiKey,
                            BaseUrl = ombiSettings.BaseUrl,
                            Hostname = ombiSettings.Hostname,
                            Port = ombiSettings.Port,
                            UseSSL = ombiSettings.UseSSL,
                            Version = ombiSettings.Version
                        });
                }

                ((JObject)settingsJson).Remove("DownloadClients");
                ((JObject)settingsJson).Add("DownloadClients", JToken.FromObject(downloadClients.ToArray()));

                var movieCategories = new List<dynamic>();

                if (movieSettings.Client == "Radarr")
                {
                    foreach (var category in radarrSettings.Categories)
                    {
                        movieCategories.Add(new
                        {
                            Id = category.Id,
                            CategoryType = "RadarrCategory",
                            DownloadClientId = downloadClients.Single(x => x.ClientType == "RadarrDownloadClientSettings").Id,
                            MinimumAvailability = category.MinimumAvailability,
                            Name = category.Name,
                            ProfileId = category.ProfileId,
                            RootFolder = category.RootFolder,
                            Tags = category.Tags,
                        });
                    }
                }
                else if (movieSettings.Client == "Overseerr")
                {
                    if (overseerrSettings.Movies.Categories.Any())
                    {
                        foreach (var category in overseerrSettings.Movies.Categories)
                        {
                            movieCategories.Add(new
                            {
                                Id = category.Id,
                                CategoryType = "OverseerrMovieCategory",
                                DownloadClientId = downloadClients.Single(x => x.ClientType == "OverseerrDownloadClientSettings").Id,
                                Is4K = category.Is4K,
                                DefaultApiUserId = overseerrSettings.Movies.DefaultApiUserID,
                                ServiceId = category.ServiceId,
                                UseOverseerrSettings = false,
                                Name = category.Name,
                                ProfileId = category.ProfileId,
                                RootFolder = category.RootFolder,
                                Tags = category.Tags,
                            });
                        }
                    }
                    else
                    {
                        movieCategories.Add(new
                        {
                            Id = 85,
                            CategoryType = "OverseerrMovieCategory",
                            DownloadClientId = downloadClients.Single(x => x.ClientType == "OverseerrDownloadClientSettings").Id,
                            Is4K = false,
                            DefaultApiUserId = overseerrSettings.Movies.DefaultApiUserID,
                            ServiceId = -1,
                            UseOverseerrSettings = true,
                            Name = "Movies",
                            ProfileId = 01,
                            RootFolder = string.Empty,
                            Tags = Array.Empty<int>(),
                        });
                    }
                }
                else if (movieSettings.Client == "Ombi")
                {
                    movieCategories.Add(new
                    {
                        Id = 75,
                        CategoryType = "OmbiMovieCategory",
                        DownloadClientId = downloadClients.Single(x => x.ClientType == "OmbiDownloadClientSettings").Id,
                        UseOmbiSettings = true,
                    });
                }

                var tvShowCategories = new List<dynamic>();

                if (tvShowSettings.Client == "Sonarr")
                {
                    foreach (var category in sonarrSettings.Categories)
                    {
                        tvShowCategories.Add(new
                        {
                            Id = category.Id,
                            CategoryType = "SonarrCategory",
                            DownloadClientId = downloadClients.Single(x => x.ClientType == "RadarrDownloadClientSettings").Id,
                            LanguageId = category.LanguageId,
                            SeriesType = category.SeriesType,
                            UseSeasonFolders = category.UseSeasonFolders,
                            Name = category.Name,
                            ProfileId = category.ProfileId,
                            RootFolder = category.RootFolder,
                            Tags = category.Tags,
                        });
                    }

                }
                else if (tvShowSettings.Client == "Overseerr")
                {
                    if (overseerrSettings.TvShows.Categories.Any())
                    {
                        foreach (var category in overseerrSettings.TvShows.Categories)
                        {
                            tvShowCategories.Add(new
                            {
                                Id = category.Id,
                                CategoryType = "OverseerrTvShowCategory",
                                DownloadClientId = downloadClients.Single(x => x.ClientType == "OverseerrDownloadClientSettings").Id,
                                Is4K = category.Is4K,
                                LanguageProfileId = category.ServiceId,
                                DefaultApiUserId = overseerrSettings.TvShows.DefaultApiUserID,
                                ServiceId = category.ServiceId,
                                UseOverseerrSettings = false,
                                Name = category.Name,
                                ProfileId = category.ProfileId,
                                RootFolder = category.RootFolder,
                                Tags = category.Tags,
                            });
                        }
                    }
                    else
                    {
                        tvShowCategories.Add(new
                        {
                            Id = 55,
                            CategoryType = "OverseerrTvShowCategory",
                            DownloadClientId = downloadClients.Single(x => x.ClientType == "OverseerrDownloadClientSettings").Id,
                            Is4K = false,
                            LanguageProfileId = -1,
                            DefaultApiUserId = overseerrSettings.TvShows.DefaultApiUserID,
                            ServiceId = -1,
                            UseOverseerrSettings = true,
                            Name = "TvShows",
                            ProfileId = 01,
                            RootFolder = string.Empty,
                            Tags = Array.Empty<int>(),
                        });
                    }
                }
                else if (tvShowSettings.Client == "Ombi")
                {
                    tvShowCategories.Add(new
                    {
                        Id = 45,
                        CategoryType = "OmbiTvShowCategory",
                        DownloadClientId = downloadClients.Single(x => x.ClientType == "OmbiDownloadClientSettings").Id,
                        UseOmbiSettings = true,
                    });
                }

                ((JObject)settingsJson["Movies"]).Remove("Client");
                ((JObject)settingsJson["TvShows"]).Remove("Client");

                ((JObject)settingsJson["Movies"]).Add("Categories", JToken.FromObject(movieCategories));
                ((JObject)settingsJson["TvShows"]).Add("Categories", JToken.FromObject(tvShowCategories));

                File.WriteAllText(settingsFilePath, JsonConvert.SerializeObject(settingsJson));
            }
        }
    }
}