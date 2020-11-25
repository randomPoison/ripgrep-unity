using System.IO;
using UnityEditor;

namespace Ripgrep.Editor
{
    public class Installer
    {
        public const string InstallRoot = "Library/com.random-poison.ripgrep-unity";
        public const string RipgrepVersion = "12.1.1";

        public static readonly string WindowsDownloadUrl = $"https://github.com/BurntSushi/ripgrep/releases/download/{RipgrepVersion}/ripgrep-{RipgrepVersion}-x86_64-pc-windows-msvc.zip";
        public static readonly string WindowsBinPath = $"Library/com.random-poison.ripgrep-unity/ripgrep-{RipgrepVersion}-x86_64-pc-windows-msvc/rg.exe";

        /// <summary>
        /// Checks if the Ripgrep binary has been installed.
        /// </summary>
        public static bool IsInstalled => File.Exists(WindowsBinPath);

        // TODO: Add a more script-friendly way to perform the install, that way other
        // scripts can trigger the install if they need ripgrep.
        [MenuItem("Tools/Ripgrep/Install Ripgrep")]
        public static void InstallMenuItem()
        {
            // TODO: Probably log any errors since there's no user code to handle them.
            Install();
        }

        public static InstallOperation Install()
        {
            // TODO: Check to see if it's already installed and skip the installation
            // process if it is.
            //
            // TODO: Provide an option to force-install, which would mean deleting any
            // existing installation before doing the install.
            //
            // TODO: Determine the correct download URL for the current platform.
            var installOp = new InstallOperation(WindowsDownloadUrl, InstallRoot);
            installOp.Start();
            return installOp;
        }
    }
}
