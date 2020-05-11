using Discord.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Cache;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AnimeListBot.Handler.API.SauceNAO
{
    public class SauceNao
    {
        private const string API_URL = "https://saucenao.com/search.php?output_type=2";
        private readonly string api_key;
        private static HttpClient client;

        public SauceNao(string api_key)
        {
            this.api_key = api_key;
            if(client == null) client = new HttpClient();
        }

        public async Task<SauceHttpResult> Trace(string url)
        {
            try
            {
                string reqUrl = API_URL + "&api_key=" + api_key + "&url=" + url;
                HttpResponseMessage res = await client.GetAsync(reqUrl);
                await Program._logger.Log("Sending Trace Request to: " + reqUrl);
                if (!res.IsSuccessStatusCode)
                {
                    return null;
                }

                string content = await res.Content.ReadAsStringAsync();
                SauceHttpResult result = JsonConvert.DeserializeObject<SauceHttpResult>(content);
                return result;
            }
            catch (HttpException) { }
            catch (JsonException jsonE) { await Program._logger.LogError(jsonE); }
            return null;
        }

        public static async Task<SauceSourceRating> GetSourceRating(SauceResult result)
        {
            async Task<Match> WebRequest(string url, string pattern)
            {
                Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
                HttpResponseMessage res = await client.GetAsync(url);
                Match webMatch = regex.Match((await res.Content.ReadAsStringAsync()));
                return webMatch;
            }

            Match match;
            SauceSourceRating rating;
            switch (result.Header.IndexId)
            {
                case SauceSiteIndex.DoujinshiMangaLexicon:
                    match = await WebRequest(result.Media.ExternalUrl[0], @"<td>.*?<b>Adult:<\/b><\/td><td>(.*)<\/td>");
                    if (match.Success)
                        rating = match.Groups[1].Value == "Yes" ? SauceSourceRating.Nsfw : SauceSourceRating.Safe;
                    else rating = SauceSourceRating.Unknown;
                    break;

                case SauceSiteIndex.Pixiv:
                case SauceSiteIndex.PixivArchive:
                    match = await WebRequest(result.Media.ExternalUrl[0], @"<div class=""introduction-modal""><p class=""title"">(.*?)<\/p>");
                    if (!match.Success) rating = SauceSourceRating.Safe;
                    else rating = match.Groups[1].Value.ToLowerInvariant().Contains("r-18") ? SauceSourceRating.Nsfw : SauceSourceRating.Safe;
                    break;

                case SauceSiteIndex.Gelbooru:
                case SauceSiteIndex.Danbooru:
                case SauceSiteIndex.SankakuChannel:
                case SauceSiteIndex.IdolComplex:
                    match = await WebRequest(result.Media.ExternalUrl[0], @"<li>Rating: (.*?)<\/li>");
                    if (!match.Success) rating = SauceSourceRating.Unknown;
                    else rating = (SauceSourceRating)Array.IndexOf(new[] { null, "Safe", "Questionable", "Explicit" }, match.Groups[1].Value);
                    break;

                case SauceSiteIndex.Yandere:
                case SauceSiteIndex.Konachan:
                    match = await WebRequest(result.Media.ExternalUrl[0], @"<li>Rating: (.*?) <span class="".*?""><\/span><\/li>");
                    if (!match.Success) rating = SauceSourceRating.Unknown;
                    else rating = (SauceSourceRating)Array.IndexOf(new[] { null, "Safe", "Questionable", "Explicit" }, match.Groups[1].Value);
                    break;

                case SauceSiteIndex.e621:
                    match = await WebRequest(result.Media.ExternalUrl[0], @"<li>Rating: <span class="".*?"">(.*)<\/span><\/li>");
                    if (!match.Success) rating = SauceSourceRating.Unknown;
                    else rating = (SauceSourceRating)Array.IndexOf(new[] { null, "Safe", "Questionable", "Explicit" }, match.Groups[1].Value);
                    break;

                case SauceSiteIndex.FAKKU:
                case SauceSiteIndex.TwoDMarket:
                case SauceSiteIndex.nHentai:
                    rating = SauceSourceRating.Nsfw;
                    break;

                case SauceSiteIndex.DeviantArt:
                    match = await WebRequest(result.Media.ExternalUrl[0], @"<h1>Mature Content<\/h1>");
                    rating = match.Success ? SauceSourceRating.Nsfw : SauceSourceRating.Safe;
                    break;

                default:
                    rating = SauceSourceRating.Unknown;
                    break;
            }

            if (rating is SauceSourceRating.Unknown)
                rating = SauceSourceRating.Questionable;

            return rating;
        }
    }
}
