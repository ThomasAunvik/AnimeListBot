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
                            minutesWatched
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
                using (var graphQLClient = new GraphQLHttpClient(AnilistConstants.AnilistAPILink))
                {
                    var response = await graphQLClient.SendQueryAsync(userRequest);

                    if (response.Errors != null && response.Errors.Length > 0)
                    {
                        if (response.Errors[0].Message.Contains("Not Found.")) return null;
                        throw new Exception(string.Join("\n", response.Errors.Select(x => x.Message)));
                    }
                    var userType = response.GetDataFieldAs<AniUser>("User");
                    return userType;
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
