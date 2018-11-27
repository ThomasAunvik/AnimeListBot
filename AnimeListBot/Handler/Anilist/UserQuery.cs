using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GraphQL.Client;
using GraphQL.Client.Http;
using GraphQL.Common.Request;

namespace AnimeListBot.Handler.Anilist
{
    public class UserQuery
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

        public static async Task<IAnilistUser> GetUser(string username)
        {
            var heroRequest = new GraphQLRequest
            {
                Query = query,
                Variables = new
                {
                    name = username
                }
            };
            var graphQLClient = new GraphQLHttpClient("https://graphql.anilist.co");
            var response = await graphQLClient.SendQueryAsync(heroRequest);
            graphQLClient.Dispose();

            if(response.Errors != null && response.Errors.Length > 0)
            {
                throw new Exception(response.Errors[0].Message);
            }
            var userType = response.GetDataFieldAs<AnilistUser>("User");
            return userType;
        }
    }
}
