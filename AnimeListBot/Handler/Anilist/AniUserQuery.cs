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
                using (var graphQLClient = new GraphQLHttpClient(AnilistConstants.AnilistAPILink, new NewtonsoftJsonSerializer()))
                {
                    var response = await graphQLClient.SendQueryAsync<AniUserResponse>(userRequest);

                    if (response.Errors != null && response.Errors.Length > 0)
                    {
                        if (response.Errors[0].Message.Contains("Not Found.")) return null;
                        throw new Exception(string.Join("\n", response.Errors.Select(x => x.Message)));
                    }

                    return response.Data.User;
                }
            }catch(Exception e)
            {
                if (!e.Message.Contains("Not Found."))
                {
                    await Program._logger.LogError(e);
                }
                return null;
            }
        }
    }
}
