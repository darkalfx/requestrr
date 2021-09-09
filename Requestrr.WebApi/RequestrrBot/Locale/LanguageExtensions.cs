using System.Collections.Generic;
using Requestrr.WebApi.RequestrrBot.Movies;
using Requestrr.WebApi.RequestrrBot.TvShows;

namespace Requestrr.WebApi.RequestrrBot.Locale
{
    public static class LanguageExtensions
    {
        public static string ReplaceTokens(this string text, TvShow tvShow, int? seasonNumber = null, Dictionary<string, string> additionalTokenReplacements = null)
        {
            var tvShowReplacementTokens = new Dictionary<string, string>
            {
                {LanguageTokens.TvShowTitle, tvShow.Title},
            };

            if (seasonNumber.HasValue)
            {
                tvShowReplacementTokens.Add(LanguageTokens.SeasonNumber, seasonNumber.Value.ToString());
            }

            var replaced = ReplaceTokens(text, tvShowReplacementTokens);

            if (additionalTokenReplacements != null)
            {
                replaced = ReplaceTokens(text, additionalTokenReplacements);
            }

            return replaced;
        }

        public static string ReplaceTokens(this string text, Movie movie, Dictionary<string, string> additionalTokenReplacements = null)
        {
            var tvShowReplacementTokens = new Dictionary<string, string>
            {
                {LanguageTokens.MovieTitle, movie.Title},
            };

            var replaced = ReplaceTokens(text, tvShowReplacementTokens);

            if (additionalTokenReplacements != null)
            {
                replaced = ReplaceTokens(text, additionalTokenReplacements);
            }

            return replaced;
        }

        public static string ReplaceTokens(this string text, string token, string value)
        {
            return text.Replace(token, value);
        }

        public static string ReplaceTokens(this string text, Dictionary<string, string> tokenReplacements)
        {
            var replaced = text;

            foreach (var replacement in tokenReplacements)
            {
                replaced = replaced.Replace(replacement.Key, replacement.Value);
            }

            return replaced;
        }
    }
}