using System.Collections;
using System.IO;
using NUnit.Framework;
using Ripgrep.Editor;
using UnityEngine.TestTools;

public class InstallationTests
{
    private const string TestDir = "Library/com.random-poison.ripgrep-unity/Test";

    public static (string, string)[] InstallParams => new (string, string)[]
    {
        (Installer.WindowsDownloadUrl, Installer.WindowsBinPath),
        (Installer.MacosDownloadUrl, Installer.MacosBinPath),
        (Installer.LinuxDownloadUrl, Installer.LinuxBinPath),
    };

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(TestDir))
        {
            Directory.Delete(TestDir, true);
        }
    }

    [UnityTest]
    public IEnumerator InstallForPlatform(
        [ValueSource(nameof(InstallParams))]
        (string url, string binPath) installParams)
    {
        var expectedBinPath = Path.Combine(TestDir, installParams.binPath);

        var installOp = new InstallOperation(installParams.url, TestDir);
        Assert.IsFalse(installOp.IsDone, $"{nameof(InstallOperation.IsDone)} should be false when operation is first created");

        installOp.Start();

        yield return installOp;

        Assert.IsTrue(installOp.IsDone, $"{nameof(InstallOperation.IsDone)} should be true when operation finishes");
        Assert.IsTrue(installOp.Succeeded, $"Install operation failed: {installOp?.OperationException}");
        Assert.IsTrue(File.Exists(expectedBinPath), $"No executable found at expected bin path: {expectedBinPath}");
    }

    public struct InstallTestParams
    {
        public string DownloadUrl;
        public string BinPath;
    }
}
