global using static System.Console;
using System.Text;
using CommonLibrary;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SharpCompress.Archives.Zip;
using SharpCompress.Common;
using SharpCompress.Providers.Default;
using SharpCompress.Readers;
using Tools;

var version = "2026.7.27";
var slnfolder = "E:\\Projects\\EroMangaManager";

var publisher = new GitHubReleasePublisher("DJDQfff", "EroMangaManager");

DotnetMakePackages maker = new(version, slnfolder);

//maker. CleanThenRestoreSlnx();

maker.BuildMsix();
maker.PubllishAPK();

await publisher.PublishAsync(version, maker.Files);
