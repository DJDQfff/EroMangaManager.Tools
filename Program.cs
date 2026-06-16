
global using static System.Console;
using Tools;

Console.WriteLine("Hello, World!");

var version = "2026.6.15";
var slnfolder = "E:\\Projects\\EroMangaManager";
var publishversion = Path.Combine(slnfolder , "publish" , slnfolder);
var files = Directory.EnumerateFiles(publishversion);


DotnetMakePackages maker = new(version ,slnfolder);

maker.CleanBuildDirectories();
maker.BuildMsix();
maker.PubllishAPK();

var publisher = new GitHubReleasePublisher("DJDQfff" , "EroMangaManager");

await publisher.PublishAsync(version , [.. files]);




