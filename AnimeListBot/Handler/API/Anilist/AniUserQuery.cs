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
using System.Threading.Tasks;
using System.Linq;
using GraphQL.Client;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL;
using Discord.Net;
using System.Net;
using System.Net.Http;

namespace AnimeListBot.Handler.Anilist
{
    public class AniUserQuery
    {
        public const string query = @"
                query($name: String){
                    User(name: $name){
                    id
		            name
   	                avatar{
                      large
                      medium
                    }

                    favourites {
                      anime {
                        nodes {
                          id
                          title {
                            romaji
                            english
                            native
                            userPreferred
                          }
          
                        }
                        pageInfo {
                          total
                          perPage
                          currentPage
                          lastPage
                          hasNextPage
                        }
                      }

                      manga {
                        nodes {
                          id
                          title {
                            romaji
                            english
                            native
                            userPreferred
                          }
          
                        }
                        pageInfo {
                          total
                          perPage
                          currentPage
                          lastPage
                          hasNextPage
                        }
                      }
                    }
    
                    statistics{
                      anime{
                        count
                        minutesWatched
                        episodesWatched
                        meanScore
                        statuses{
         	              status
                          count
                          meanScore
                          chaptersRead
                        }
                      }
      
                      manga{
                        count
                        volumesRead
                        chaptersRead
                        meanScore
                        statuses{
         	              status
                          count
                          meanScore
                          chaptersRead
                        }
                      }
                    }
    
		            siteUrl
                  }
                }";

        public static async Task<IAniUser> GetUser(string username)
        {
            try
            {
                var userRequest = new GraphQLRequest
                {
                    Query = query,
                    Variables = new
                    {
                        name = username
                    }
                };
                using var graphQLClient = new GraphQLHttpClient(AnilistConstants.AnilistAPILink, new NewtonsoftJsonSerializer());
                var response = await graphQLClient.SendQueryAsync<AniUserResponse>(userRequest);
                return response.Data.User;
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
