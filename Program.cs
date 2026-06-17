
global using static System.Console;
using Tools;

Console.WriteLine("Hello, World!");

var version = "2026.6.17";
var slnfolder = "E:\\Projects\\EroMangaManager";

var publisher = new GitHubReleasePublisher("DJDQfff" , "EroMangaManager");

DotnetMakePackages maker = new(version ,slnfolder);

//maker. CleanThenRestoreSlnx();

maker.BuildMsix();
maker.PubllishAPK();


await publisher.PublishAsync(version , maker.Files);




