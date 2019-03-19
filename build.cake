using IO = System.IO;

var configuration = Argument("-c", "Debug");
var target = Argument("Target", "Default");

Task("Default")
    .IsDependentOn("Test")
    .IsDependentOn("Pack")
    ;
Task("Build")
    .Does(() =>
    {
        var setting = new DotNetCoreBuildSettings()
        {
            Configuration = configuration,
        };
        DotNetCoreBuild("PooledStream.sln", setting);
    });
Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        var setting = new DotNetCoreTestSettings()
        {
            Configuration = configuration,
            Framework = "netcoreapp2.0"
        };
        DotNetCoreTest(IO.Path.Combine("PooledStream.Test", "PooledStream.Test.csproj"), setting);
        setting.Framework = "netcoreapp3.0";
        DotNetCoreTest(IO.Path.Combine("PooledStream.Test", "PooledStream.Test.csproj"), setting);
    })
    ;
Task("Pack")
    .IsDependentOn("Build")
    .Does(() =>
    {
        var settings = new DotNetCorePackSettings()
        {
            Configuration = configuration
        };
        DotNetCorePack("PooledStream.sln", settings);
    });
RunTarget(target);