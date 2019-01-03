#tool "nuget:?package=xunit.runner.console"

var target = Argument("target", "Default");

string configuration;
var appVeyorBranch = EnvironmentVariable("APPVEYOR_REPO_BRANCH");

switch (appVeyorBranch)
{
    case "master":
        configuration = "Release";
        break;
    case "development":
        configuration = "Debug";
        break;
    default:
        configuration = "Release";
        break;
}

var artifactsDir = Directory("./artifacts");
var solution = "./AnimeListBot.sln";

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(artifactsDir);
    });

Task("Restore-NuGet-Packages")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        NuGetRestore(solution);
    });

Task("Build")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
    {
        var settings = new DotNetCorePublishSettings
		{
			Framework = "netcoreapp2.0",
			Configuration = "Release",
			OutputDirectory = "../" + artifactsDir
		};
	
        DotNetCorePublish("AnimeListBot/AnimeListBot.csproj", settings);
    });

Task("Run-Tests")
    .IsDependentOn("Build")
    .Does(() =>
    {
        XUnit2("./tests/**/bin/" + configuration + "/*.Tests.dll", new XUnit2Settings
        {
            // If needed:
            // Parallelism = ParallelismOption.None
            // or similar.
        });
    });

Task("Package")
    .IsDependentOn("Run-Tests")
    .Does(() =>
    {
		var settings = new DotNetCorePublishSettings
		{
			Framework = "netcoreapp2.0",
			Configuration = "Release",
			OutputDirectory = "../" + artifactsDir
		};
	
        DotNetCorePublish("AnimeListBot/AnimeListBot.csproj", settings);
    });

Task("Default")
    .IsDependentOn("Package");

RunTarget(target);