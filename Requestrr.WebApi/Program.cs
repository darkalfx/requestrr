using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Requestrr.WebApi.RequestrrBot;

namespace Requestrr.WebApi
{
    public class Program
    {
        public static int Port = 4545;
        public static string BaseUrl = string.Empty;
        public static dynamic LocalizedStrings = null;
        public static void Main(string[] args)
        {
            UpdateSettingsFile();
            CheckForLanguageFiles();
            Port = (int)SettingsFile.Read().Port;
            BaseUrl = SettingsFile.Read().BaseUrl;
            LocalizedStrings = Localization.Read(SettingsFile.Read().Language.ToString());
            CreateWebHostBuilder(args).Build().Run();
        }

        private static void CheckForLanguageFiles()
        {
            if(!Directory.Exists("config/local"))
            {
                Directory.CreateDirectory("config/local");
                FileInfo[] files = new DirectoryInfo("local/").GetFiles();
                foreach(FileInfo file in files)
                {
                    string tempPath = Path.Combine("config/local",file.Name);
                    file.CopyTo(tempPath, false);
                }
            }
        }

        private static void UpdateSettingsFile()
        {
            if (!File.Exists(SettingsFile.FilePath))
            {
                File.WriteAllText(SettingsFile.FilePath, File.ReadAllText("SettingsTemplate.json").Replace("[PRIVATEKEY]", Guid.NewGuid().ToString()));
            }
            else
            {
                SettingsFileUpgrader.Upgrade(SettingsFile.FilePath);
            }

            if (!File.Exists(NotificationsFile.FilePath))
            {
                File.WriteAllText(NotificationsFile.FilePath, File.ReadAllText("NotificationsTemplate.json"));
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls($"http://*:{Port}")
                .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile(SettingsFile.FilePath, optional: false, reloadOnChange: true);
            });
    }
}
