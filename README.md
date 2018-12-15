# AnimeList Bot [![CodeFactor](https://www.codefactor.io/repository/github/thomasaunvik/animelistbot/badge)](https://www.codefactor.io/repository/github/thomasaunvik/animelistbot)
This bot is made in [
RogueException/Discord.Net](https://github.com/RogueException/Discord.Net) C#, and is built in dotnet core.

You can invite my bot to your discord server by clicking: [here](https://discordapp.com/api/oauth2/authorize?client_id=377558188826034216&permissions=0&scope=bot)!

If you want to contact me via discord, use the `!contact [message]` command, or add me as friend: `Thaun_#7409`

# Introduction

Welcome to my Anime List Bot, where you can make people in your server compete on how much anime you have seen.

## Profile

Once the bot is online, you can add your MAL or Anilist profile: `.setup MAL/Anilist Username` and you will be added to the bot. You are able to register both of the same time, but you can only use one of them.

Use `.setlist MAL/Anilist` to choose which one to use.

`.profile` Shows Anime and Manga statistics, just only days and rank.

`.profile anime` or `.animeprofile` shows you full statistics for anime. Rank, Days, Mean Score, Total Entries, Episodes watched, normal statistics.

`.profile manga` or `.mangaprofile` shows full manga statistics like anime statistics, just with chapters and volumes.

`.leaderboard` will give you a leaderboard of which one of the people in the server have watched/read most anime/manga.

## Ranks

Ranks in this bot are setup this way if you have enough days you gain a role.

*[Requires: ManageRoles]* `.addrank [anime/manga] [@role/id] [days]` which will add a rank to the server, if someone ever has watched the amount of [days] in either [anime] or [manga] they get the rank.

*[Requires: ManageRoles]* `.editrank [anime/manga] [@role/id] [newdays]` which will edit the current needed days tor the rank to be given.

*[Requires: ManageRoles]* `.removerank [anime/manga] [@role/id]` which will remove the rank from the server, and noone will be able to get it anymore.

*[Requires: ManageRoles]* `.updateranks` Updates ranks for all users in the server.

`.updaterank` Updates your rank if needed.

`.ranks` Shows a list of current ranks, for anime and manga.

## Search

`.anime` Searches for anime from your animelist (MAL/Anilist), and also shows your statistics of that anime if you have registered it.

`.manga` Searches for mang afrom your animelist (MAL/Anilist), and also shows your statistics of that manga if you have registered it.

## Trace

Thanks to [soruly/trace.moe](https://github.com/soruly/trace.moe/)!

`.trace <image-link>` searches that image for what anime that image is from.

`.trace (with image as attachment)` same as above, but you have to upload the image as attachment while you do the command.

## AutoAdder

*[Requires: ManageRoles]* `.automal [#channel/id]` sets the channel where users can just paste their MAL/Anilist link there, and it will be automaticly added to the bot.

*[Requires: ManageRoles]* `.automalupdate` checks all the messages from the autmal channel and adds them.

`.automalchannel` sends a message back where it gives you the link for the channel.

## Help

`.help` Replies with a list of commands.

`.help [command]` Replies with a description and usage of the command specified.

`.contact [message]` sends a DM to the bot owner.

`.invite` gets an invite link for the bot

`.github` gets the github link for the bot (this github)

## Administrator

These commands are special and can only be used by the bot owner.
If you yourself want to host this bot for yourself, you can edit the `botOwners.txt` file and add all the user'ids per line in there for those you want to give bot owner power.

`.stop` stops the bot and exits the program.

`.errortest [errormessage]` bot sends a fake exception to the program

`.setgamestatus [activityType] [gameMessage]` sets the game status for the bot to whichever Playing, Streaming, Listening or Watching and then adds a message to the side.

`setonlinestatus [status]` sets which status the bot should have, Online, Idle, AFK, etc.

Extra Info: As a bot owner, you get to have the exlusive right to be able to get your exception messages right inside your DM's, thou the ID is hard coded. Found in [Logger.cs](https://github.com/ThomasAunvik/AnimeListBot/blob/master/AnimeListBot/Handler/Logger.cs)

# Setup
1. Download from GitHub or clone the repository: https://github.com/ThomasAunvik/AnimeListBot.git
2. Setup an app in https://discordapp.com/developers/applications/me
3. Copy the token and insert in `botToken_template.txt` 
4. Rename `botToken_template.txt` to `botToken.txt`
5. In Visual Studio 2017, publish the app from the Build toolbar.
6. Run command `dotnet AnimeListBot.dll` in `repo\AnimeListBot\bin\Debug\netcoreapp2.0`

# Libs and API's

Thanks to these who made these libraries, and made this bot possible.

[RogueException/Discord.Net](https://github.com/RogueException/Discord.Net)

[Ervie/jikan.net](https://github.com/Ervie/jikan.net)

[graphql-dotnet/graphql-client](https://github.com/graphql-dotnet/graphql-client)

[JamesNK/Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
