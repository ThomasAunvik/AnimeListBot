# AnimeList Bot [![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT) [![CodeFactor](https://www.codefactor.io/repository/github/thomasaunvik/animelistbot/badge)](https://www.codefactor.io/repository/github/thomasaunvik/animelistbot) [![Build status](https://ci.appveyor.com/api/projects/status/0qse6o8yrx64gjf1?svg=true)](https://ci.appveyor.com/project/ThomasAunvik/animelistbot) ![.NET Core](https://github.com/ThomasAunvik/AnimeListBot/workflows/.NET%20Core/badge.svg)
This bot is made in [
RogueException/Discord.Net](https://github.com/RogueException/Discord.Net) C#, and is built in dotnet core.

You can invite my bot to your discord server by clicking: [here](https://discordapp.com/oauth2/authorize?client_id=515269277553655823&scope=bot&permissions=287808)!

If you want to contact me via discord, use the `al!contact [message]` command, or add me as friend: `Thaun_#0001`

Else, you can join the support discord: https://discord.gg/Q9cf46R

# Introduction

Welcome to my Anime List Bot, where you can make people in your server compete on how much anime you have seen.

## Profile

Once the bot is online, you can add your MAL or Anilist profile: `al!setup MAL/Anilist Username` and you will be added to the bot. You are able to register both of the same time, but you can only use one of them.

Use `al!setlist MAL/Anilist` to choose which one to use.

`al!profile` Shows Anime and Manga statistics, just only days and rank.

`al!profile anime` or `al!animeprofile` shows you full statistics for anime. Rank, Days, Mean Score, Total Entries, Episodes watched, normal statistics.

`al!profile manga` or `al!mangaprofile` shows full manga statistics like anime statistics, just with chapters and volumes.

`al!leaderboard` will give you a leaderboard of which one of the people in the server have watched/read most anime/manga.

`al!setlist` allow you to swap between profiles, from MAL to Anilist without reapplying profile settings.

`al!resetuser [Y]` removes all stored user data that the bot currently holds. Use the argument "Y" to confirm deletion of data. Warning: This is not reversible.

## Ranks

Ranks in this bot are setup this way if you have enough days you gain a role.

*[Requires: ManageRoles]* `al!addrank [anime/manga] [@role/id] [days]` which will add a rank to the server, if someone ever has watched the amount of [days] in either [anime] or [manga] they get the rank.

*[Requires: ManageRoles]* `al!editrank [anime/manga] [@role/id] [newdays]` which will edit the current needed days tor the rank to be given.

*[Requires: ManageRoles]* `al!removerank [anime/manga] [@role/id]` which will remove the rank from the server, and noone will be able to get it anymore.

*[Requires: ManageRoles]* `al!updateranks` Updates ranks for all users in the server.

`al!updaterank` Updates your rank if needed.

`al!ranks` Shows a list of current ranks, for anime and manga.

## Search

`al!anime` Searches for anime from your animelist (MAL/Anilist), and also shows your statistics of that anime if you have registered it.

`al!manga` Searches for manga from your animelist (MAL/Anilist), and also shows your statistics of that manga if you have registered it.

`al!character` Searches for character from your animelist (MAL/Anilist).

`al!staff` Searches for staff from your animelist (MAL/Anilist).

## Trace

Thanks to [soruly/trace.moe](https://github.com/soruly/trace.moe/)!

`al!trace <image-link>` searches that image for what anime that image is from.

`al!trace (with image as attachment)` same as above, but you have to upload the image as attachment while you do the command.

## AutoAdder

*[Requires: ManageRoles]* `al!autolist [#channel/id]` sets the channel where users can just paste their MAL/Anilist link there, and it will be automaticly added to the bot.

*[Requires: ManageRoles]* `al!autolistupdate` checks all the messages from the autmal channel and adds them.

`al!autolistchannel` sends a message back where it gives you the link for the channel.

## Help

`al!help` Replies with a list of commands.

`al!help [command]` Replies with a description and usage of the command specified.

`al!contact [message]` sends a DM to the bot owner.

`al!invite` gets an invite link for the bot

`al!github` gets the github link for the bot (this github)

## Bot Info

`a!stats` shows statistics of the server, for example uptime, usercount, guilds and more.

`a!info` shows bot information, owner, shards and current shard, Discord.NET version, and links to invite and github.

`a!support` gives discord invite link to the support server, and shows the command for contact.

## Administrator

These commands are special and can only be used by the bot owner.
If you yourself want to host this bot for yourself, you can edit the `botOwners.txt` file and add all the user'ids per line in there for those you want to give bot owner power.

`al!stop` stops the bot and exits the program.

`al!errortest [errormessage]` bot sends a fake exception to the program

`al!setgamestatus [activityType] [gameMessage]` sets the game status for the bot to whichever Playing, Streaming, Listening or Watching and then adds a message to the side.

`al!setonlinestatus [status]` sets which status the bot should have, Online, Idle, AFK, etc.

`al!anilimit` checks your rate limit for anilist.

`al!gitstatus` checks the bot's current git status.

`al!prefix` allows you to change the bot's prefix. Max 2 characters.

Extra Info: As a bot owner, you get to have the exlusive right to be able to get your exception messages right inside your DM's, thou the ID is hard coded. Found in [Logger.cs](https://github.com/ThomasAunvik/AnimeListBot/blob/master/AnimeListBot/Handler/Logger.cs)

# Setup
1. Download from GitHub or clone the repository: https://github.com/ThomasAunvik/AnimeListBot.git
2. Setup an app in https://discordapp.com/developers/applications/me

3. Copy the token and insert in `config_template.json` 
4. Rename `config_template.json` to `config.json`
5. In Visual Studio 2019, publish the app from the Build toolbar.
6. Run command `dotnet AnimeListBot.dll` in `repo\AnimeListBot\bin\Debug\netcoreapp2.0`

## Database Setup

To be able to use the bot, you would have to have a database setup. And at the moment, the database currently used is **PostgreSQL**.

To connect your PostgreSQL database to the server, make sure you have the tables correctly set up as shown below, and edit the `config.json` to the correct login information.

### Database Table Structure

discord_server
| server_id  | register_channel_id | animerole_id | animerole_days | mangarole_id | mangarole_days | prefix |
| ------------- | ------------- | ------------- | ------------- | ------------- | ------------- | ------------- |
| *bigint*  | *bigint* | *bigint[]* | *double precision[]* | *bigint[]* | *double precision[]* | *text* |
| 697542156373721101  | 697545325748944997  | {id0,id1,id2} | {days0,days1,days2} | {id0,id1,id2} | {days0,days1,days2} | a! |

discord_user
| user_id  | mal_username | anilist_username | list_preference | anime_days | manga_days |
| ------------- | ------------- | ------------- | ------------- | ------------- | ------------- |
| *bigint*  | *text* | *text* | *integer* | *bigint* | *bigint* |
| 96580514021912576  | Thaun_  | Thaun | 0(MAL) or 1(Anilist) | 100 | 25 |


***IN PROGRESS***

cluster
| id  | shard_id_start | shard_id_end |
| ------------- | ------------- | ------------- |
| *bigint*  | *integer* | *integer* |
| 0  | 0  | 2 |
| 1  | 3  | 5 |

# Libs and API's

Thanks to these who made these libraries, and made this bot possible.

[RogueException/Discord.Net](https://github.com/RogueException/Discord.Net)

[Ervie/jikan.net](https://github.com/Ervie/jikan.net)

[graphql-dotnet/graphql-client](https://github.com/graphql-dotnet/graphql-client)

[JamesNK/Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
