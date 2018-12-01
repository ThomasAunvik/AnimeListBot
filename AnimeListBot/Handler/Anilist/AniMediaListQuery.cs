using GraphQL.Client.Http;
using GraphQL.Common.Request;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AnimeListBot.Handler.Anilist
{
    public class MediaListQuery
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

        public static async Task<IAnilistMediaList> GetMediaList(string username, int id, AnilistMediaType mediaType)
        {
            try
            {
                var mediaListRequest = new GraphQLRequest
                {
                    Query = searchQuery,
                    Variables = new
                    {
                        name = username,
                        mediaId = id,
                        type = Enum.GetName(typeof(AnilistMediaType), mediaType),
                        scoreFormat = Enum.GetName(typeof(ScoreFormat), ScoreFormat.POINT_10_DECIMAL)
                    }
                };
                var graphQLClient = new GraphQLHttpClient(AnilistConstants.AnilistAPILink);
                var response = await graphQLClient.SendQueryAsync(mediaListRequest);
                graphQLClient.Dispose();

                if (response.Errors != null && response.Errors.Length > 0)
                {
                    throw new Exception(string.Join("\n", response.Errors.Select(x => x.Message)));
                }
                var mediaList = response.GetDataFieldAs<AnilistMediaList>("MediaList");

                return mediaList;
            }
            catch (Exception e)
            {
                await Program._logger.LogError(e);
                return null;
            }
        }
    }
}
