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
using GraphQL.Client.Http;
using GraphQL;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GraphQL.Client.Serializer.Newtonsoft;
using System.Net;

namespace AnimeListBot.Handler.Anilist
{
    public class AniStaffQuery
    {
        public const string searchQuery = @"
                query ($search: String, $asHtml: Boolean){
                    Staff(search: $search) {
                        id
                        name {
                            first
                            last
                            native
                        }
                        language
                        image {
                            large
                            medium
                        }
                        description(asHtml: $asHtml)
                        siteUrl
                    }
                }
                ";

        public static async Task<IAniStaff> SearchStaff(string staffSearch)
        {
            try
            {
                var mediaRequest = new GraphQLRequest
                {
                    Query = searchQuery,
                    Variables = new
                    {
                        search = staffSearch,
                        asHtml = false
                    }
                };
                using (var graphQLClient = new GraphQLHttpClient(AnilistConstants.AnilistAPILink, new NewtonsoftJsonSerializer()))
                {
                    var response = await graphQLClient.SendQueryAsync<AniStaffResponse>(mediaRequest);
                    var staff = response.Data.Staff;
                    staff.description = staff?.description?.Replace("<br>", "\n");

                    if (staff.description == null) staff.description = string.Empty;
                    return staff;
                }
            }
            catch (GraphQLHttpException http)
            {
                if (http.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound) return null;
                await Program._logger.LogError(http);
                return null;
            }
            catch (Exception e)
            {
                await Program._logger.LogError(e);
                return null;
            }
        }
    }
}
