using Microsoft.EntityFrameworkCore.Metadata.Internal;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
public class DotnetMakePackages 
{
    public   List<string> Files { get; } = [];

    public  void Run ()
    {
        var packtool = Directory.GetDirectories("C:\\Program Files (x86)\\Microsoft Visual Studio\\Shared\\NuGetPackages\\microsoft.windows.sdk.buildtools\\" , "x64" , new EnumerationOptions() { RecurseSubdirectories = true }).First();
        var makeappx = Directory.GetFiles(packtool).Single( x => x.EndsWith("makeappx.exe"));
        var signtool = Directory.GetFiles(packtool).Single(x => x.EndsWith("signtool.exe"));

        string rootPath = "E:\\Projects\\EroMangaManager";
        string winappcsproj = "E:\\Projects\\EroMangaManager\\WinApp\\WinApp.csproj";
        string unoappcsproj = "E:\\Projects\\EroMangaManager\\UnoApp\\UnoApp\\UnoApp.csproj";
        string binPath_WinApp= "E:\\Projects\\EroMangaManager/WinApp/bin";
        string unorelease = "E:\\Projects\\EroMangaManager\\UnoApp\\UnoApp\\bin\\Release\\net10.0-android";
        var publishDirectory = new  DirectoryInfo("E:\\Projects\\EroMangaManager\\publish");
        string pfxPath = "E:\\Projects\\EroMangaManager\\DJDQfff_new.pfx";
        string pfxPassword = Environment.GetEnvironmentVariable("MADAO_PASSWORD");
        string msixbundleFIle = "E:\\Projects\\EroMangaManager\\publish\\upload.msixbundle";
        var platforms = new string[] { "x64" , "x86" , "ARM64" };
        var runtimeidentifiers_android = new string[] { "android-arm64" , "android-arm" };
        var rids = new Dictionary<string , string>
        {
            ["x64"] = "win-x64" ,
            ["x86"] = "win-x86" ,
            ["ARM64"] = "win-arm64"
        };
        Files.Add("E:\\Projects\\EroMangaManager\\DJDQfff_certificate.cer");
        foreach(var platform in platforms)
        {

            var tempDirectory = Directory.CreateTempSubdirectory().FullName;

            Process.Start("dotnet" , $" build {winappcsproj} -c Release -p:Platform={platform} -p:RuntimeIdentifier={rids[platform]} -p:AppxBundle=Never /p:GenerateAppxPackageOnBuild=true /p:PackageCertificateKeyFile={pfxPath} /p:PackageCertificatePassword={pfxPassword}").WaitForExit();

            var folder = Path.Combine(binPath_WinApp , platform);
            folder = Path.Combine(folder , "Release");
            var files = Directory.EnumerateFiles(folder , $"*{platform}.msix" , new EnumerationOptions() { RecurseSubdirectories = true });
            var msix = files.Single();
            var target = Path.Combine(publishDirectory.FullName , $"{Path.GetFileName(msix)}");
            File.Copy(msix , target,true);

            Files.Add(msix);
        }

        Process.Start(makeappx, $"bundle /o /d \"{publishDirectory.FullName}\" /p \"{msixbundleFIle}\"").WaitForExit();

        
        Process
            .Start(signtool , $"sign /fd SHA256 /a /f \"{pfxPath}\" /p {pfxPassword}  \"{msixbundleFIle}\"")
            .WaitForExit();


        foreach (var runtime in runtimeidentifiers_android)
        {
            var tempDirectory = Directory.CreateTempSubdirectory().FullName;

            Process.Start("dotnet" , $" publish {unoappcsproj} -f net10.0-android -r {runtime}  -c Release ").WaitForExit();

            var folder = Path.Combine(unorelease , runtime);
            folder = Path.Combine(folder , "publish");
            var apks = Directory.EnumerateFiles(folder , $"*-Signed.apk" , new EnumerationOptions() { RecurseSubdirectories = true });
            var apk = apks.Single();
            var newapk = apk.Replace(".apk" , $"-{runtime}.apk");
            File.Move(apk , newapk,true);
            Files.Add(newapk );
        }
    }
    static void Run (string fileName , string arguments , string workingDir)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName ,
            Arguments = arguments ,
            WorkingDirectory = workingDir ,
            UseShellExecute = false ,
            CreateNoWindow = true
        };

        using var proc = Process.Start(psi);
        proc?.WaitForExit();
    }
}