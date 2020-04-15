using GraphQL.Client.Http;
using GraphQL;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GraphQL.Client.Serializer.Newtonsoft;

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

                    if (response.Errors != null && response.Errors.Length > 0)
                    {
                        if (response.Errors[0].Message.Contains("Not Found.")) return null;
                        throw new Exception(string.Join("\n", response.Errors.Select(x => x.Message)));
                    }
                    var staff = response.Data.Staff;
                    staff.description = staff?.description?.Replace("<br>", "\n");
                    return staff;
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
