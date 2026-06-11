using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Tools;

internal class Class1
{
    public static void Run ()
    {

        var workfolder = @"E:\Projects\EroMangaManager\WinApp";

        var cmdfolder =
            @"C:\Program Files (x86)\Microsoft Visual Studio\Shared\NuGetPackages\microsoft.windows.sdk.buildtools\10.0.26100.7705\bin\10.0.26100.0\x64";

        if (!Directory.Exists(cmdfolder))
        {
            Console.WriteLine("bin目录不存在");
            return;
        }

        var makeappx = Path.Combine(cmdfolder , "makeappx.exe");
        var signtool = Path.Combine(cmdfolder , "signtool.exe");

        var tempfolder = Directory.CreateTempSubdirectory();

        var file = Path.Combine(tempfolder.FullName , "result.msixbundle");
        var pfx = @"../../DJDQfff_new.pfx";
        var password = "512131415zcb";

        var appxmanifest = Directory.GetFiles(workfolder , "Package.appxmanifest")[0];
        var lines = File.ReadAllText(appxmanifest);
        string regexpattern = @"Version=""\S*""";
        var version = Regex.Match(lines , regexpattern).Value.Replace("Version=" , "").Trim('\"');
        var index = 0;

        var msixs = Directory.GetFiles(
            workfolder ,
            $"*_{version}_*.msix" ,
            SearchOption.AllDirectories
        );
        foreach (var m in msixs)
        {
            var target = Path.Combine(tempfolder.FullName , $"{index++}.msix");
            File.Copy(m , target);
        }

        Process.Start(makeappx , $"bundle /d \"{tempfolder}\" /p \"{file}\"").WaitForExit();

        Process
            .Start(signtool , $"sign /fd SHA256 /a /f \"{pfx}\" /p {password}  \"{file}\"")
            .WaitForExit();

        File.Move(file , Path.Combine(workfolder , $"{version}.msixbundle"));
    }
}
