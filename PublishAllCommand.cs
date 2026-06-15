using System.Diagnostics;

namespace Tools;

public class DotnetMakePackages
{
    public List<string> Files { get; } = [];
    private readonly string rootPath ;
    private readonly string pfxPath;
    private readonly string pfxPassword=null!;
    private readonly string version;
    private readonly string packtoolfolder;
    private readonly string makeappx;
    private readonly string signtool;
    private readonly string msixbundleFIle;
    private readonly string publishversionfolder;
    private readonly string slnPath;
    public DotnetMakePackages (string _version,string slnFolder)
    {
        rootPath = slnFolder;
        slnPath = Directory.GetFiles(rootPath , "*.slnx").First();
        pfxPassword = Environment.GetEnvironmentVariable("MADAO_PASSWORD") ?? throw new ArgumentException("环境变量：证书密码未配置");

        // ... 其他原有代码保持不变
        version = _version;
        msixbundleFIle = Path.Combine(rootPath , "publish" , $"{version}.msixbundle");

        packtoolfolder = Directory.GetDirectories("C:\\Program Files (x86)\\Microsoft Visual Studio\\Shared\\NuGetPackages\\microsoft.windows.sdk.buildtools\\" , "x64" , new EnumerationOptions() { RecurseSubdirectories = true }).First();
         pfxPath = Path.Combine(rootPath, "DJDQfff_new.pfx");

        makeappx = Directory.GetFiles(packtoolfolder).Single(x => x.EndsWith("makeappx.exe"));
        signtool = Directory.GetFiles(packtoolfolder).Single(x => x.EndsWith("signtool.exe"));

         publishversionfolder = Path.Combine(slnFolder,"publish" , version);
        Directory.CreateDirectory(publishversionfolder);

    }
    public void BuildMsix ()
    {
        var targetframwork = "net10.0-windows10.0.26100";

        string winappcsproj = Path.Combine(rootPath , "WinApp/WinApp.csproj");
        string WinApp_bin = Path.Combine(rootPath , "WinApp/bin");
        // 👇 1. 在构建任何项目之前，先对整个解决方案执行还原
        if (!string.IsNullOrEmpty(slnPath))
        {
            if (!string.IsNullOrEmpty(slnPath))
            {
                Run("dotnet" , $"restore \"{slnPath}\"" , rootPath);
            }
        }
        string[]? platforms = ["x64" , "x86" , "ARM64"];
        var rids = new Dictionary<string , string>
        {
            ["x64"] = "win-x64" ,
            ["x86"] = "win-x86" ,
            ["ARM64"] = "win-arm64"
        };
        foreach (var platform in platforms)
        {
            //var tempDirectory = Directory.CreateTempSubdirectory().FullName;
            //Process.Start("dotnet" ,
            //    $" restore {winappcsproj}" +
            //    $" -p:TargetFramework={targetframwork}")
            //    .WaitForExit();
            //Process.Start("dotnet" ,
            //    $" build {winappcsproj}" +
            //    $" --no-restore" +
            //    $" --no-dependencies" +
            //    $" -c Release" +
            //    $" -f {targetframwork} " +
            //    $" -p:Platform={platform} " +
            //    $" -p:RuntimeIdentifier={rids[platform]}" +
            //    $" -p:AppxBundle=Never " +
            //    $" /p:GenerateAppxPackageOnBuild=true " +
            //    $" /p:PackageCertificateKeyFile={pfxPath} " +
            //    $" /p:PackageCertificatePassword={pfxPassword}")
            //    .WaitForExit();

            var msbuild = @"C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe";
            string msbuildArgs = $"{winappcsproj} /t:Publish" +
                                             $" /p:Configuration=Release" +
                                             $" /p:Platform={platform}" +
                                             $" /p:RuntimeIdentifier={rids[platform]}" +
                                             $" /p:TargetFramework={targetframwork}" +
                                             $" /p:AppxBundle=Never" +
                                             $" /p:GenerateAppxPackageOnBuild=true" +
                                             $" /p:PackageCertificateKeyFile={pfxPath}" +
                                             $" /p:PackageCertificatePassword={pfxPassword}";

            Run(msbuild , msbuildArgs , rootPath);
            var folder = Path.Combine(WinApp_bin , platform , "Release" , targetframwork , rids[platform]);
            var files = Directory.EnumerateFiles(folder , $"*_{platform}.msix" , new EnumerationOptions() { RecurseSubdirectories = true });
            var msix = files.Single();
            var target = Path.Combine(publishversionfolder , $"{Path.GetFileName(msix)}");
            File.Move(msix , target , true);

            Files.Add(msix);
      
        }

        // 👇 3. 打包和签名也换成 Run 方法
        Run(makeappx , $"bundle /o /d \"{publishversionfolder}\" /p \"{msixbundleFIle}\"" , rootPath);
        Run(signtool , $"sign /fd SHA256 /a /f \"{pfxPath}\" /p {pfxPassword} \"{msixbundleFIle}\"" , rootPath);
    }

    
    public void PubllishAPK ()
    {
        var targetframework = "net10.0-android";

        string unoappcsproj = Path.Combine(rootPath , "UnoApp/UnoApp.csproj");

        string unorelease = Path.Combine(rootPath , $"UnoApp/bin/Release/{targetframework}");
        //// 👇 1. 使用 Run 方法对整个解决方案执行还原
        //if (!string.IsNullOrEmpty(slnPath))
        //{
        //    Run("dotnet" , $"restore \"{slnPath}\" -p:TargetFramework={targetframework}" , rootPath);
        //}


        var runtimeidentifiers = new string[] { "android-arm64" , "android-arm" };
        Files.Add(Path.Combine(rootPath , "DJDQfff_certificate.cer"));


        foreach (var runtime in runtimeidentifiers)
        {
            //var tempDirectory = Directory.CreateTempSubdirectory().FullName;
            // 发布 APK
            Run("dotnet" , $"publish \"{unoappcsproj}\" -f {targetframework} -r {runtime} -c Release" , rootPath);

            var folder = Path.Combine(unorelease , runtime , "publish");
            var apks = Directory.EnumerateFiles(folder , $"*-Signed.apk" , new EnumerationOptions() { RecurseSubdirectories = true });
            var apk = apks.Single();
            var newapk = Path.Combine(publishversionfolder,Path.GetFileNameWithoutExtension(apk) +$"-{runtime}.apk");
            File.Move(apk , newapk , true);
            Files.Add(newapk);
        }
    }
    public void CleanBuildDirectories ()
    {
        string[] bin_obj = ["bin" , "obj"];
        string[] folders = ["WinApp" , "Database" , "Server" , "Core" , "UnoLibrary" , "UnoApp"];
        foreach(var folder in folders)
        {
            var directoryinfo=new DirectoryInfo(Path.Combine(rootPath,folder));
        
        foreach (var b_o in bin_obj)
            {
            var objfolder = directoryinfo. GetDirectories(b_o).SingleOrDefault();
            objfolder?.Delete(true);

            }

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
            CreateNoWindow = false
        };

        using var proc = Process.Start(psi);
        proc?.WaitForExit();
        if (proc?.ExitCode != 0)
        {
            throw new Exception($"命令执行失败 (ExitCode: {proc.ExitCode}): {fileName} {arguments}");
        }
    }
}