using System;
using System.Collections.Generic;
using System.Text;

namespace MALBot.Handler.Anilist
{
    public interface IUserOptions
    {
        UserTitleLanguage titleLanguage { get; }

        bool displayAdultContent { get; }
        bool airingNotifications { get; }
        string profileColor { get; }
    }

    public enum UserTitleLanguage
    {
        // The romanization of the native language title
        ROMAJI,
        // The official english title
        ENGLISH,
        // Official title in it's native language
        NATIVE,
        // The romanization of the native language title, stylised by media creator
        ROMAJI_STYLISED,
        // The official english title, stylised by media creator
        ENGLISH_STYLISED,
        // Official title in it's native language, stylised by media creator
        NATIVE_STYLISED
    }
}
