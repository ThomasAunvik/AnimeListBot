/*
 * This file is part of AnimeList Bot
 *
 * AnimeList Bot is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * AnimeList Bot is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with AnimeList Bot.  If not, see <https://www.gnu.org/licenses/>
 */
using JikanDotNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace AnimeListBot.Handler.Misc
{
    public class MalClassTransfer
    {
        public static MangaSearchEntry MangaToSearchEntry(Manga manga)
        {
            MangaSearchEntry entry = new MangaSearchEntry();
            entry.MalId = manga.MalId;
            entry.URL = manga.LinkCanonical;
            entry.ImageURL = manga.ImageURL;
            entry.Title = manga.Title;
            entry.Description = manga.Synopsis;
            entry.Type = manga.Type;
            entry.Score = manga.Score;
            entry.Members = manga.Members;
            entry.Publishing = manga.Publishing;
            entry.StartDate = manga.Published.From;
            entry.EndDate = manga.Published.To;

            int volumes;
            int.TryParse(manga.Volumes, out volumes);
            entry.Volumes = volumes;

            int chapters;
            int.TryParse(manga.Volumes, out chapters);
            entry.Chapters = chapters;
            return entry;
        }

        public static AnimeSearchEntry AnimeToSearchEntry(Anime anime)
        {
            AnimeSearchEntry entry = new AnimeSearchEntry();
            entry.MalId = anime.MalId;
            entry.URL = anime.LinkCanonical;
            entry.ImageURL = anime.ImageURL;
            entry.Title = anime.Title;
            entry.Description = anime.Synopsis;
            entry.Type = anime.Type;
            entry.Score = anime.Score;
            entry.Members = anime.Members;
            entry.Airing = anime.Airing;
            entry.StartDate = anime.Aired.From;
            entry.EndDate = anime.Aired.To;
            entry.Rated = anime.Rating;

            int episodes;
            int.TryParse(anime.Episodes, out episodes);
            entry.Episodes = episodes;
            return entry;
        }
    }
}
