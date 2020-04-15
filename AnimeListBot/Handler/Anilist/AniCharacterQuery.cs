using GraphQL.Client.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL;

namespace AnimeListBot.Handler.Anilist
{
    public class AniCharacterQuery
    {
        public const string searchQuery = @"
                query ($search: String, $asHtml: Boolean){
                    Character(search: $search) {
                        id
                        name {
                            first
                            last
                            native
                            alternative
                        }
                        description(asHtml: $asHtml)
                        image {
                            large
                            medium
                        }
                        siteUrl
                    }
                }
                ";

        public static async Task<IAniCharacter> SearchCharacter(string characterSearch)
        {
            try
            {
                var mediaRequest = new GraphQLRequest
                {
                    Query = searchQuery,
                    Variables = new
                    {
                        search = characterSearch,
                        asHtml = false
                    }
                };

                using (var graphQLClient = new GraphQLHttpClient(AnilistConstants.AnilistAPILink, new NewtonsoftJsonSerializer()))
                {
                    var response = await graphQLClient.SendQueryAsync<AniCharacterResponse>(mediaRequest);

                    if (response.Errors != null && response.Errors.Length > 0)
                    {
                        if (response.Errors[0].Message.Contains("Not Found.")) return null;
                        throw new Exception(string.Join("\n", response.Errors.Select(x => x.Message)));
                    }
                    var character = response.Data.Character;
                    character.description = character?.description?.Replace("<br>", "\n");
                    return character;
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
