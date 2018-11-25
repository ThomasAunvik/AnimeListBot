using System;
using System.Collections.Generic;
using System.Text;
using GraphQL;
using GraphQL.Types;

namespace MALBot.Handler.Anilist
{
    /// <summary>
    /// # User query
    /// #
    /// # Arguments
    /// # id: Filter by the user id
    /// # name: Filter by the name of the user
    /// # search: Filter by search query
    /// # sort: The order the results will be returned in
    /// User(id: Int, name: String, search: String, sort: [UserSort]) : User
    /// 
    /// # Get the currently authenticated user
    /// Viewer: User
    /// </summary>
    public class UserQuery
    {
        public int id;
        public string name;
        public string search;
        public UserSort sort;

        public ISchema GetSchema()
        {
            var schema = Schema.For(@"
                type Query {
                    User(id: Int, name: String, search: String, sort: [UserSort]) : User
                }
                ");
            return schema;
        }
    }

    public enum UserSort
    {
        ID,
        ID_DESC,
        USERNAME,
        USERNAME_DESC,
        WATCHED_TIME,
        WATCHED_TIME_DESC,
        CHAPTERS_READ,
        CHAPTERS_READ_DESC,
        SEARCH_MATCH
    }
}
