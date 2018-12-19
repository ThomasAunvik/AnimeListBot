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
        MSBuild(solution, settings =>
            settings.SetConfiguration(configuration)
                .WithProperty("TreatWarningsAsErrors", "True")
                .SetVerbosity(Verbosity.Minimal)
                .AddFileLogger());
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
        MSBuild("AnimeListBot/AnimeListBot.csproj", settings =>
            settings.SetConfiguration(configuration)
                .WithProperty("TreatWarningsAsErrors", "True")
                .SetVerbosity(Verbosity.Minimal)
                .WithProperty("PackageLocation", Directory("../..") + artifactsDir));
    });

Task("Default")
    .IsDependentOn("Package");

RunTarget(target);