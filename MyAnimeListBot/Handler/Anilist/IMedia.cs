using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IMedia
    {
        int id { get; }
        int idMal { get; }

        IMediaTitle title { get; }

        MediaFormat format { get; }

        MediaStatus status { get; }

        string description { get; }
        bool asHtml { get; }

        IFuzzyDate startDate { get; }

        IFuzzyDate endDate { get; }

        MediaSeason season { get; }

        int episodes { get; }

        int duration { get; }

        int chapters { get; }

        int volumes { get; }

        // SCALAR
        //public CountryCode countryOfOrigin;

        MediaSource source { get; }

        string hastag { get; }

        IMediaTrailer trailer { get; }

        int updatedAt { get; }

        IMediaCoverImage coverImage { get; }

        string bannerImage { get; }

        List<string> genres { get; }

        List<string> synonyms { get; }

        int averageScore { get; }

        int meanScore { get; }

        int popularity { get; }

        int trending { get; }

        List<IMediaTag> tags { get; }

        //characters
        ICharacterConnection characters { get; }

        //staff
        IStaffConnection staff { get; }

        //studios
        IStudioConnection studios { get; }

        bool isFavourite { get; }

        bool isAdult { get; }

        IAiringSchedule nextAiringSchedule { get; }

        List<IMediaExternalLink> externalLinks { get; }

        List<IMediaStreamingEpisode> streamingEpisodes { get; }

        List<IMediaRank> rankings { get; }

        IMediaList mediaListEntry { get; }

        MediaStats stats { get; }

        string siteUrl { get; }

        bool autoCreateForumThread { get; }

        string modNotes { get; }
    }
}
