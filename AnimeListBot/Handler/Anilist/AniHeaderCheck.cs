/*
 * This file is part of AnimeList Bot
 *
 * AnimeList Bot is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AnimeList Bot is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AnimeList Bot.  If not, see <https://www.gnu.org/licenses/>
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Linq;

namespace AnimeListBot.Handler.Anilist
{
    public class AniHeaderCheck
    {
        const string API_URL = "https://graphql.anilist.co";
        private static readonly HttpClient client = new HttpClient();

        public static async Task<AniHeaderResult> CheckHeaders()
        {
            var values = new Dictionary<string, string>();
            var content = new FormUrlEncodedContent(values);
            
            var response = await client.PostAsync(API_URL, content);

            AniHeaderResult result = new AniHeaderResult();

            IEnumerable<string> headers;
            if (response.Headers.TryGetValues("X-RateLimit-Limit", out headers))
            {
                int rateLimit_Limit = 0;
                if (int.TryParse(headers.First(), out rateLimit_Limit))
                {
                    result.RateLimit_Limit = rateLimit_Limit;
                }
            }
            
            if (response.Headers.TryGetValues("X-RateLimit-Remaining", out headers))
            {
                int rateLimit_Remaining = 0;
                if (int.TryParse(headers.First(), out rateLimit_Remaining))
                {
                    result.RateLimit_Remaining = rateLimit_Remaining;
                }
            }

            if (response.Headers.TryGetValues("X-RateLimit-Reset", out headers))
            {
                int rateLimit_ResetTime = 0;
                if (int.TryParse(headers.First(), out rateLimit_ResetTime))
                {
                    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    dateTime = dateTime.AddSeconds(rateLimit_ResetTime);
                    result.RateLimit_Reset = dateTime;
                }
            }

            if (response.Headers.TryGetValues("Retry-After", out headers))
            {
                int retryAfter = 0;
                if (int.TryParse(headers.First(), out retryAfter))
                {
                    result.RetryAfter = retryAfter;
                }
            }
            return result;
        }
    }
}
