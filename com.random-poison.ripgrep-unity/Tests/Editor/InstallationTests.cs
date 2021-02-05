using System.Collections;
using System.IO;
using NUnit.Framework;
using Ripgrep.Editor;
using UnityEngine.TestTools;

public class InstallationTests
{
    private const string TestDir = "Library/com.random-poison.ripgrep-unity/Test";

    public static (string, string, ArchiveType)[] PlatformInstallations => new (string, string, ArchiveType)[]
    {
        (Installer.WindowsDownloadUrl, Installer.WindowsBinPath, ArchiveType.Zip),
        (Installer.MacosDownloadUrl, Installer.MacosBinPath, ArchiveType.Tgz),
        (Installer.LinuxDownloadUrl, Installer.LinuxBinPath, ArchiveType.Tgz),
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
        [ValueSource(nameof(PlatformInstallations))]
        (string url, string binPath, ArchiveType archiveType) installParams)
    {
        var expectedBinPath = Path.Combine(TestDir, installParams.binPath);

        var installOp = new InstallOperation(installParams.url, TestDir, installParams.archiveType);
        Assert.IsFalse(installOp.IsDone, $"{nameof(InstallOperation.IsDone)} should be false when operation is first created");

        installOp.Start();

        yield return installOp;

        Assert.IsTrue(installOp.IsDone, $"{nameof(InstallOperation.IsDone)} should be true when operation finishes");
        Assert.IsTrue(installOp.Succeeded, $"Install operation failed: {installOp?.OperationException}");
        Assert.IsTrue(File.Exists(expectedBinPath), $"No executable found at expected bin path: {expectedBinPath}");
    }

    [UnityTest]
    public IEnumerator InstallForCurrentPlatform()
    {
        // If there's already an existing install, clear it out so that we can be sure
        // we're actually testing the installation process.
        if (Installer.IsInstalled)
        {
            Directory.Delete(Installer.InstallRoot, true);
        }

        Assert.IsFalse(Installer.IsInstalled, "Ripgrep is still installed after deleting root directory");

        var installOp = Installer.Install();
        yield return installOp;

        Assert.IsTrue(installOp.IsDone, $"{nameof(InstallOperation.IsDone)} should be true when operation finishes");
        Assert.IsTrue(installOp.Succeeded, $"Install operation failed: {installOp?.OperationException}");
        Assert.IsTrue(Installer.IsInstalled, "Ripgrep is not installed after install operation finished");
    }
}
