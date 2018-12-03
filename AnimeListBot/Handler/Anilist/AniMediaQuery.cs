using GraphQL.Client.Http;
using GraphQL.Common.Request;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AnimeListBot.Handler.Anilist
{
    public class AniMediaQuery
    {
        public const string searchQuery = @"
                query ($search: String, $type: MediaType, $asHtml: Boolean){
                    Media(search: $search, type: $type) {
                        id
                        idMal
                        title {
                            romaji
                            english
                            native
                        }
                        type
                        status
                        description(asHtml: $asHtml)
                        startDate {
                            year
                            month
                            day
                        }
                        endDate {
                            year
                            month
                            day
                        }
                        episodes
                        chapters
                        volumes
                        coverImage {
                            large
                            medium
                        }
                        siteUrl
                    }
                }
                ";

        public static async Task<IAniMedia> SearchMedia(string mediaSearch, AniMediaType mediaType)
        {
            try
            {
                var mediaRequest = new GraphQLRequest
                {
                    Query = searchQuery,
                    Variables = new
                    {
                        search = mediaSearch,
                        type = Enum.GetName(typeof(AniMediaType), mediaType),
                        asHtml = false
                    }
                };
                using (var graphQLClient = new GraphQLHttpClient(AnilistConstants.AnilistAPILink))
                {
                    var response = await graphQLClient.SendQueryAsync(mediaRequest);

                    if (response.Errors != null && response.Errors.Length > 0)
                    {
                        if (response.Errors[0].Message.Contains("Not Found.")) return null;
                        throw new Exception(string.Join("\n", response.Errors.Select(x => x.Message)));
                    }
                    var media = response.GetDataFieldAs<AniMedia>("Media");

                    media.description = media.description.Replace("<br>", "\n");

                    return media;
                }
            }
            catch (Exception e)
            {
                await Program._logger.LogError(e);
                return null;
            }
        }
    }
}
