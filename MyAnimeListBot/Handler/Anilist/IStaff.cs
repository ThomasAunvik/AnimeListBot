using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IStaff
    {
        int id { get; }
        IStaffName name { get; }
        StaffLanguage language { get; }

        IStaffImage image { get; }

        string description { get; }
        bool asHtml { get; }

        bool isFavourite { get; }

        string siteUrl { get; }

        IMediaConnection staffMedia { get; }

        ICharacterConnection characters { get; }
    }
}
