using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Ripgrep.Editor
{
    public class Installer
    {
        public const string InstallRoot = "Library/com.random-poison.ripgrep-unity";
        public const string RipgrepVersion = "12.1.1";

        public static readonly string WindowsDownloadUrl = $"https://github.com/BurntSushi/ripgrep/releases/download/{RipgrepVersion}/ripgrep-{RipgrepVersion}-x86_64-pc-windows-msvc.zip";
        public static readonly string MacosDownloadUrl = $"https://github.com/BurntSushi/ripgrep/releases/download/{RipgrepVersion}/ripgrep-{RipgrepVersion}-x86_64-apple-darwin.tar.gz";
        public static readonly string LinuxDownloadUrl = $"https://github.com/BurntSushi/ripgrep/releases/download/{RipgrepVersion}/ripgrep-{RipgrepVersion}-x86_64-unknown-linux-musl.tar.gz";

        public static readonly string WindowsBinPath = $"ripgrep-{RipgrepVersion}-x86_64-pc-windows-msvc/rg.exe";
        public static readonly string MacosBinPath = $"ripgrep-{RipgrepVersion}-x86_64-apple-darwin/rg";
        public static readonly string LinuxBinPath = $"ripgrep-{RipgrepVersion}-x86_64-unknown-linux-musl/rg";

        /// <summary>
        /// Path to the installed Ripgrep binary.
        /// </summary>
        ///
        /// <remarks>
        /// The path is platform-specific, and will point to the correct path for the current
        /// editor platform.
        /// </remarks>
        public static string BinPath
        {
            get
            {
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                        return Path.Combine(InstallRoot, WindowsBinPath);

                    case RuntimePlatform.OSXEditor:
                        return Path.Combine(InstallRoot, MacosBinPath);

                    case RuntimePlatform.LinuxEditor:
                        return Path.Combine(InstallRoot, LinuxBinPath);

                    default:
                        throw new InvalidOperationException(
                            $"Invalid install platform {Application.platform}");
                }
            }
        }

        /// <summary>
        /// Checks if the Ripgrep binary has been installed.
        /// </summary>
        public static bool IsInstalled => File.Exists(BinPath);

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
            var installOp = new InstallOperation(WindowsDownloadUrl, InstallRoot, ArchiveType.Zip);
            installOp.Start();
            return installOp;
        }
    }
}
