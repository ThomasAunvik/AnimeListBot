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
    public class AniMediaListQuery
    {
        public const string searchQuery = @"
                query ($userName: String, $mediaId: Int, $type: MediaType, $scoreFormat: ScoreFormat){
                    MediaList(userName: $userName, mediaId: $mediaId, type: $type) {
                        id
                        userId
                        mediaId
                        status
                        score(format: $scoreFormat)
                        progress
                        progressVolumes
                        repeat
                        startedAt {
                            year
                            month
                            day
                        }
                        completedAt {
                            year
                            month
                            day
                        }
                    }
                }
                ";

        public static async Task<IAniMediaList> GetMediaList(string username, int id, AniMediaType mediaType)
        {
            try
            {
                var mediaListRequest = new GraphQLRequest
                {
                    Query = searchQuery,
                    Variables = new
                    {
                        userName = username,
                        mediaId = id,
                        type = Enum.GetName(typeof(AniMediaType), mediaType),
                        scoreFormat = Enum.GetName(typeof(AniScoreFormat), AniScoreFormat.POINT_10_DECIMAL)
                    }
                };

                using (var graphQLClient = new GraphQLHttpClient(AnilistConstants.AnilistAPILink, new NewtonsoftJsonSerializer()))
                {
                    var response = await graphQLClient.SendQueryAsync<AniMediaListResponse>(mediaListRequest);
                    return response.Data.MediaList;
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
