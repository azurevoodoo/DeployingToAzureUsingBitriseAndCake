#addin nuget:?package=Cake.Kudu.Client&version=0.4.0

var target = Argument("target", "Deploy");
var configuration = Argument("configuration", "Release");
var solution = "./src/ShippedFromBitrise.sln";
var project = "./src/ShippedFromBitrise/ShippedFromBitrise.csproj";
var output = "./output";

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

Task("Publish")
    .IsDependentOn("Build")
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
    .Does( () => {
    string  baseUri     = EnvironmentVariable("KUDU_CLIENT_BASEURI"),
            userName    = EnvironmentVariable("KUDU_CLIENT_USERNAME"),
            password    = EnvironmentVariable("KUDU_CLIENT_PASSWORD");

    IKuduClient kuduClient = KuduClient(
        baseUri,
        userName,
        password);

    kuduClient.ZipDeployDirectory(output);
});

RunTarget(target);