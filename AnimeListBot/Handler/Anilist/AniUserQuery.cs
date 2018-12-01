using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using GraphQL.Client;
using GraphQL.Client.Http;
using GraphQL.Common.Request;

namespace AnimeListBot.Handler.Anilist
{
    public class AniUserQuery
    {
        public const string query = @"
                query ($name: String){
                    User(name: $name) {
                        id
                        name
                        avatar {
                            large
                            medium
                        }
                        stats {
                            watchedTime
                            chaptersRead

                            animeStatusDistribution {
                                status
                                amount
                            }

                            mangaStatusDistribution {
                                status
                                amount
                            }

                            animeScoreDistribution {
                                score
                                amount
                            }

                            mangaScoreDistribution {
                                score
                                amount
                            }
                            
                            animeListScores {
                                meanScore
                                standardDeviation
                            }

                            mangaListScores {
                                meanScore
                                standardDeviation
                            }
                        }
                        siteUrl
                    }
                }
                ";

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
                var graphQLClient = new GraphQLHttpClient(AnilistConstants.AnilistAPILink);
                var response = await graphQLClient.SendQueryAsync(userRequest);
                graphQLClient.Dispose();

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    throw new Exception(string.Join("\n", response.Errors.Select(x => x.Message)));
                }
                var userType = response.GetDataFieldAs<AnilistUser>("User");
                return userType;
            }catch(Exception e)
            {
                await Program._logger.LogError(e);
                return null;
            }
        }
    }
}
