using Tools;

using static System.Console;
Console.WriteLine("Hello, World!");

var version = "2026.6.11";
var slnfolder = "E:\\Projects\\EroMangaManager";
var publishversion = Path.Combine(slnfolder , "publish" , slnfolder);
var publisher = new GitHubReleasePublisher("DJDQfff" , "EroMangaManager");
var files = Directory.EnumerateFiles(publishversion);

await publisher.PublishAsync(version , [.. files]);
return;

DotnetMakePackages maker = new(version ,slnfolder);
maker.CleanBuildDirectories();
//maker.BuildMsix();
maker.PubllishAPK();




