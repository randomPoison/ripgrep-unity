using System.Collections;
using System.IO;
using NUnit.Framework;
using Ripgrep.Editor;
using UnityEngine.TestTools;

public class InstallationTests
{
    private const string TestDir = "Library/com.random-poison.ripgrep-unity/Test";

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(TestDir))
        {
            Directory.Delete(TestDir, true);
        }
    }

    [UnityTest]
    public IEnumerator Install_Windows()
    {
        var installDir = Path.Combine(TestDir, "Windows");
        var expectedBinPath = Path.Combine(
            installDir,
            $"ripgrep-{Installer.RipgrepVersion}-x86_64-pc-windows-msvc/rg.exe"
        );

        var installOp = new InstallOperation(Installer.WindowsDownloadUrl, installDir);
        Assert.IsFalse(installOp.IsDone, $"{nameof(InstallOperation.IsDone)} should be false when operation is first created");

        installOp.Start();

        yield return installOp;

        Assert.IsTrue(installOp.IsDone, $"{nameof(InstallOperation.IsDone)} should be true when operation finishes");
        Assert.IsTrue(installOp.Succeeded, "Install operation failed");
        Assert.IsTrue(File.Exists(expectedBinPath), $"No executable found at expected bin path: {expectedBinPath}");
    }

    [Test]
    public void FailingTest()
    {
        Assert.IsTrue(false, "Verifying that test failures are reported");
    }
}
