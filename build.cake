#addin nuget:?package=Cake.Kudu.Client&version=0.4.0

string  target          = Argument("target", "Default"),
        configuration   = Argument("configuration", "Release"),
        solution        = "./src/ShippedFromBitrise.sln",
        project         = "./src/ShippedFromBitrise/ShippedFromBitrise.csproj",
        testProject     = "./src/ShippedFromBitrise.Tests/ShippedFromBitrise.Tests.csproj",
        output          = "./output",
        baseUri         = EnvironmentVariable("KUDU_CLIENT_BASEURI"),
        userName        = EnvironmentVariable("KUDU_CLIENT_USERNAME"),
        password        = EnvironmentVariable("KUDU_CLIENT_PASSWORD");

Task("Restore")
    .Does( () => {
    DotNetCoreRestore(solution);
});

Task("Clean")
    .Does( () => {
    CleanDirectory(output);
});

Task("Build")
    .IsDependentOn("Restore")
    .IsDependentOn("Clean")
    .Does( () => {
    DotNetCoreBuild(
        solution,
        new DotNetCoreBuildSettings {
            NoRestore = true,
            Configuration = configuration
        });
});

Task("Test")
    .IsDependentOn("Build")
    .Does( () => {
    DotNetCoreTest(
        testProject,
        new DotNetCoreTestSettings {
            NoRestore = false,
            NoBuild = false,
            Configuration = configuration,
            OutputDirectory = output
        });
});

Task("Publish")
    .IsDependentOn("Test")
    .Does( () => {
    DotNetCorePublish(
        project,
        new DotNetCorePublishSettings {
            NoRestore = false,
            Configuration = configuration,
            OutputDirectory = output
        });
});

Task("Deploy")
    .IsDependentOn("Publish")
    .WithCriteria(
        !string.IsNullOrEmpty(baseUri) &&
        !string.IsNullOrEmpty(userName) &&
        !string.IsNullOrEmpty(password))
    .Does( () => {
    IKuduClient kuduClient = KuduClient(
        baseUri,
        userName,
        password);

    kuduClient.ZipDeployDirectory(output);
});

Task("Default")
    .IsDependentOn("Deploy");

RunTarget(target);