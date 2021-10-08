using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;
using Requestrr.WebApi.RequestrrBot.ChatClients.Discord;
using Requestrr.WebApi.RequestrrBot.DownloadClients;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Radarr;
using Requestrr.WebApi.RequestrrBot.DownloadClients.Sonarr;
using Requestrr.WebApi.RequestrrBot.Locale;

namespace Requestrr.WebApi.RequestrrBot
{
    public static class SlashCommandBuilder
    {
        public static string DLLFileName = "slashcommandsbuilder";

        public static Type Build(ILogger logger, DiscordSettings settings, RadarrSettingsProvider radarrSettingsProvider, SonarrSettingsProvider sonarrSettingsProvider)
        {
            string code = GetCode(settings, radarrSettingsProvider.Provide(), sonarrSettingsProvider.Provide());
            var tree = SyntaxFactory.ParseSyntaxTree(code);
            string fileName = $"{DLLFileName}-{Guid.NewGuid()}.dll";

            var references = new List<PortableExecutableReference>()
            {
              MetadataReference.CreateFromFile(typeof(object).GetTypeInfo().Assembly.Location),
              MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).GetTypeInfo().Assembly.Location),
              MetadataReference.CreateFromFile(typeof(ApplicationCommandModule).GetTypeInfo().Assembly.Location),
              MetadataReference.CreateFromFile(typeof(SlashCommandBuilder).GetTypeInfo().Assembly.Location),
              MetadataReference.CreateFromFile(typeof(Attribute).GetTypeInfo().Assembly.Location),
              MetadataReference.CreateFromFile(typeof(Task).GetTypeInfo().Assembly.Location),
              MetadataReference.CreateFromFile(typeof(DiscordUser).GetTypeInfo().Assembly.Location),
              MetadataReference.CreateFromFile(typeof(IServiceProvider).GetTypeInfo().Assembly.Location),
              MetadataReference.CreateFromFile(typeof(ILogger).GetTypeInfo().Assembly.Location),
            };

            references.Add(MetadataReference.CreateFromFile(AppDomain.CurrentDomain.GetAssemblies().Single(a => a.GetName().Name == "netstandard").Location));

            Assembly.GetEntryAssembly().GetReferencedAssemblies().ToList()
                       .ForEach(a => references.Add(MetadataReference.CreateFromFile(Assembly.Load(a).Location)));

            var compilation = CSharpCompilation.Create(fileName)
              .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
              .AddReferences(references)
              .AddSyntaxTrees(tree);

            var dirInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
            var tmpDirectory = dirInfo.EnumerateDirectories().Where(x => x.Name == "tmp").Single().FullName;

            string path = Path.Combine(tmpDirectory, fileName);

            var compilationResult = compilation.Emit(path);
            if (compilationResult.Success)
            {
                var asm = AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                return asm.GetType("Requestrr.WebApi.RequestrrBot.SlashCommands");
            }
            else
            {
                foreach (Diagnostic codeIssue in compilationResult.Diagnostics)
                {
                    string issue = $"ID: {codeIssue.Id}, Message: {codeIssue.GetMessage()}, Location: { codeIssue.Location.GetLineSpan()},Severity: { codeIssue.Severity}";
                    logger.LogError("Failed to build SlashCommands assembly: " + issue);
                }
            }

            throw new Exception("Failed to build SlashCommands assembly.");
        }

        private static string GetCode(DiscordSettings settings, RadarrSettings radarrSettings, SonarrSettings sonarrSettings)
        {
            var code = File.ReadAllText("SlashCommands.txt");

            code = code.Replace("[REQUEST_GROUP_NAME]", Language.Current.DiscordCommandRequestGroupName);
            code = code.Replace("[REQUEST_GROUP_DESCRIPTION]", Language.Current.DiscordCommandRequestGroupDescription);
            code = code.Replace("[REQUEST_MOVIE_TITLE_DESCRIPTION]", Language.Current.DiscordCommandMovieRequestTitleDescription);
            code = code.Replace("[REQUEST_MOVIE_TITLE_OPTION_NAME]", Language.Current.DiscordCommandMovieRequestTitleOptionName);
            code = code.Replace("[REQUEST_MOVIE_TITLE_OPTION_DESCRIPTION]", Language.Current.DiscordCommandMovieRequestTitleOptionDescription);
            code = code.Replace("[REQUEST_MOVIE_TMDB_DESCRIPTION]", Language.Current.DiscordCommandMovieRequestTmbdDescription);
            code = code.Replace("[REQUEST_MOVIE_TMDB_OPTION_NAME]", Language.Current.DiscordCommandMovieRequestTmbdOptionName);
            code = code.Replace("[REQUEST_MOVIE_TMDB_OPTION_DESCRIPTION]", Language.Current.DiscordCommandMovieRequestTmbdOptionDescription);
            code = code.Replace("[REQUEST_TV_TITLE_DESCRIPTION]", Language.Current.DiscordCommandTvRequestTitleDescription);
            code = code.Replace("[REQUEST_TV_TITLE_OPTION_NAME]", Language.Current.DiscordCommandTvRequestTitleOptionName);
            code = code.Replace("[REQUEST_TV_TITLE_OPTION_DESCRIPTION]", Language.Current.DiscordCommandTvRequestTitleOptionDescription);
            code = code.Replace("[REQUEST_TV_TVDB_DESCRIPTION]", Language.Current.DiscordCommandTvRequestTvdbDescription);
            code = code.Replace("[REQUEST_TV_TVDB_OPTION_NAME]", Language.Current.DiscordCommandTvRequestTvdbOptionName);
            code = code.Replace("[REQUEST_TV_TVDB_OPTION_DESCRIPTION]", Language.Current.DiscordCommandTvRequestTvdbOptionDescription);
            code = code.Replace("[REQUEST_PING_NAME]", Language.Current.DiscordCommandPingRequestName);
            code = code.Replace("[REQUEST_PING_DESCRIPTION]", Language.Current.DiscordCommandPingRequestDescription);
            code = code.Replace("[REQUEST_HELP_NAME]", Language.Current.DiscordCommandHelpRequestName);
            code = code.Replace("[REQUEST_HELP_DESCRIPTION]", Language.Current.DiscordCommandHelpRequestDescription);
            code = code.Replace("[REQUIRED_MOVIE_ROLE_IDS]", string.Join(",", settings.MovieRoles.Select(x => $"{x}UL")));
            code = code.Replace("[REQUIRED_TV_ROLE_IDS]", string.Join(",", settings.TvShowRoles.Select(x => $"{x}UL")));
            code = code.Replace("[REQUIRED_CHANNEL_IDS]", string.Join(",", settings.MonitoredChannels.Select(x => $"{x}UL")));

            if (settings.MovieDownloadClient == DownloadClient.Disabled && settings.TvShowDownloadClient == DownloadClient.Disabled)
            {
                var beginIndex = code.IndexOf("[REQUEST_COMMAND_START]");
                var endIndex = code.IndexOf("[REQUEST_COMMAND_END]") + "[REQUEST_COMMAND_END]".Length;

                code = code.Replace(code.Substring(beginIndex, endIndex - beginIndex), string.Empty);
            }
            else
            {
                if (settings.MovieDownloadClient == DownloadClient.Disabled)
                {
                    var beginIndex = code.IndexOf("[MOVIE_COMMAND_START]");
                    var endIndex = code.IndexOf("[MOVIE_COMMAND_END]") + "[MOVIE_COMMAND_END]".Length;

                    code = code.Replace(code.Substring(beginIndex, endIndex - beginIndex), string.Empty);
                }
                else if (settings.MovieDownloadClient == DownloadClient.Radarr)
                {
                    var beginIndex = code.IndexOf("[MOVIE_COMMAND_START]");
                    var endIndex = code.IndexOf("[MOVIE_COMMAND_END]") + "[MOVIE_COMMAND_END]".Length;
                    var categoryCommandTemplate = code.Substring(beginIndex, endIndex - beginIndex);
                    categoryCommandTemplate = categoryCommandTemplate.Replace("[MOVIE_COMMAND_START]", string.Empty);
                    categoryCommandTemplate = categoryCommandTemplate.Replace("[MOVIE_COMMAND_END]", string.Empty);

                    var tmdbStartIndex = categoryCommandTemplate.IndexOf("[TMDB_COMMAND_START]");
                    var tmdbEndIndex = categoryCommandTemplate.IndexOf("[TMDB_COMMAND_END]") + "[TMDB_COMMAND_END]".Length;
                    categoryCommandTemplate = categoryCommandTemplate.Replace(categoryCommandTemplate.Substring(tmdbStartIndex, tmdbEndIndex - tmdbStartIndex), string.Empty);

                    var sb = new StringBuilder();

                    foreach (var category in radarrSettings.Categories)
                    {
                        var currentTemplate = categoryCommandTemplate;
                        currentTemplate = currentTemplate.Replace("[MOVIE_CATEGORY_ID]", category.Id.ToString());
                        currentTemplate = currentTemplate.Replace("[REQUEST_MOVIE_TITLE_NAME]", category.Name);
                        currentTemplate = currentTemplate.Replace("[REQUEST_MOVIE_TMDB_NAME]", $"{category.Name}-tmdb");

                        sb.Append(currentTemplate);
                    }

                    code = code.Replace(code.Substring(beginIndex, endIndex - beginIndex), sb.ToString());
                }
                else
                {
                    code = code.Replace("[REQUEST_MOVIE_TITLE_NAME]", Language.Current.DiscordCommandMovieRequestTitleName);
                    code = code.Replace("[REQUEST_MOVIE_TMDB_NAME]", Language.Current.DiscordCommandMovieRequestTmbdName);
                    code = code.Replace("[MOVIE_COMMAND_START]", string.Empty);
                    code = code.Replace("[MOVIE_COMMAND_END]", string.Empty);
                    code = code.Replace("[TMDB_COMMAND_START]", string.Empty);
                    code = code.Replace("[TMDB_COMMAND_END]", string.Empty);
                    code = code.Replace("[MOVIE_CATEGORY_ID]", "99999");
                }

                if (settings.TvShowDownloadClient == DownloadClient.Disabled)
                {
                    var beginIndex = code.IndexOf("[TV_COMMAND_START]");
                    var endIndex = code.IndexOf("[TV_COMMAND_END]") + "[TV_COMMAND_END]".Length;

                    code = code.Replace(code.Substring(beginIndex, endIndex - beginIndex), string.Empty);
                }
                else if (settings.TvShowDownloadClient == DownloadClient.Sonarr)
                {
                    var beginIndex = code.IndexOf("[TV_COMMAND_START]");
                    var endIndex = code.IndexOf("[TV_COMMAND_END]") + "[TV_COMMAND_END]".Length;
                    var categoryCommandTemplate = code.Substring(beginIndex, endIndex - beginIndex);
                    categoryCommandTemplate = categoryCommandTemplate.Replace("[TV_COMMAND_START]", string.Empty);
                    categoryCommandTemplate = categoryCommandTemplate.Replace("[TV_COMMAND_END]", string.Empty);

                    var tvdbStartIndex = categoryCommandTemplate.IndexOf("[TVDB_COMMAND_START]");
                    var tvdbEndIndex = categoryCommandTemplate.IndexOf("[TVDB_COMMAND_END]") + "[TVDB_COMMAND_END]".Length;
                    categoryCommandTemplate = categoryCommandTemplate.Replace(categoryCommandTemplate.Substring(tvdbStartIndex, tvdbEndIndex - tvdbStartIndex), string.Empty);

                    var sb = new StringBuilder();

                    foreach (var category in sonarrSettings.Categories)
                    {
                        var currentTemplate = categoryCommandTemplate;
                        currentTemplate = currentTemplate.Replace("[TV_CATEGORY_ID]", category.Id.ToString());
                        currentTemplate = currentTemplate.Replace("[REQUEST_TV_TITLE_NAME]", category.Name);
                        currentTemplate = currentTemplate.Replace("[REQUEST_TV_TVDB_NAME]", $"{category.Name}-tvdb");

                        sb.Append(currentTemplate);
                    }

                    code = code.Replace(code.Substring(beginIndex, endIndex - beginIndex), sb.ToString());
                }
                else
                {
                    code = code.Replace("[REQUEST_TV_TITLE_NAME]", Language.Current.DiscordCommandTvRequestTitleName);
                    code = code.Replace("[REQUEST_TV_TVDB_NAME]", Language.Current.DiscordCommandTvRequestTvdbName);
                    code = code.Replace("[TV_COMMAND_START]", string.Empty);
                    code = code.Replace("[TV_COMMAND_END]", string.Empty);
                    code = code.Replace("[TVDB_COMMAND_START]", string.Empty);
                    code = code.Replace("[TVDB_COMMAND_END]", string.Empty);
                    code = code.Replace("[TV_CATEGORY_ID]", "99999");
                }

                code = code.Replace("[REQUEST_COMMAND_START]", string.Empty);
                code = code.Replace("[REQUEST_COMMAND_END]", string.Empty);
            }

            return code;
        }

        public static void CleanUp()
        {
            try
            {
                var dirInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
                var tmpDirectory = dirInfo.EnumerateDirectories().Where(x => x.Name == "tmp").Single().FullName;
                var filesToDelete = Directory.GetFiles(tmpDirectory, $"*.dll");

                foreach (var dllToDelete in filesToDelete.Where(x => x.Contains(SlashCommandBuilder.DLLFileName, StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        File.Delete(dllToDelete);
                    }
                    catch { }
                }
            }
            catch { }
        }
    }
}