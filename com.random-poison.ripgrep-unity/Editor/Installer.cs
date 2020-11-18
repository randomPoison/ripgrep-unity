using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using UnityEditor;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

namespace Ripgrep.Editor
{
    public class Installer
    {
        internal const string DownloadUrl = "https://github.com/BurntSushi/ripgrep/releases/download/12.1.1/ripgrep-12.1.1-x86_64-pc-windows-msvc.zip";
        internal const string InstallRoot = "Library/com.random-poison.ripgrep-unity";
        internal const string BinPath = "Library/com.random-poison.ripgrep-unity/ripgrep-12.1.1-x86_64-pc-windows-msvc/rg.exe";

        /// <summary>
        /// Checks if the Ripgrep binary has been installed.
        /// </summary>
        public static bool IsInstalled => File.Exists(BinPath);

        // TODO: Add a more script-friendly way to perform the install, that way other
        // scripts can trigger the install if they need ripgrep.
        [MenuItem("Tools/Ripgrep/Install Ripgrep")]
        public static void TryToDoAnInstall()
        {
            // TODO: Check to see if it's already installed and skip the installation
            // process if it is.
            //
            // TODO: Provide an option to force-install, which would mean deleting any
            // existing installation before doing the install.

            // TODO: Determine the correct download URL for the current platform.
            var request = UnityWebRequest.Get(DownloadUrl);
            request.SendWebRequest().completed += _ =>
            {
                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.LogError($"Failed to download archive: {request.error}");
                    return;
                }

                var downloadPath = Path.Combine(InstallRoot, "ripgrep.zip");

                // Create the directory for the file if it doesn't already exist.
                Directory.CreateDirectory(Path.GetDirectoryName(downloadPath));

                // TODO: Use a more efficient method for transferring the download bytes.
                // Ideally this would mean streaming the downloaded bytes directly into
                // the unzipper, but if we have to unzip from a file then at least
                // streaming the bytes into a file without buffering them in-memory would
                // be an improvement.
                File.WriteAllBytes(downloadPath, request.downloadHandler.data);

                using (var fileInput = File.OpenRead(downloadPath))
                using (var zipFile = new ZipFile(fileInput))
                {
                    foreach (ZipEntry zipEntry in zipFile)
                    {
                        // Ignore directories, since we handle recreating the directory
                        // hierarchy based on the path described in the entry name.
                        if (!zipEntry.IsFile)
                        {
                            continue;
                        }

                        var entryFileName = zipEntry.Name;

                        // Determine the output path for the file.
                        var fullZipToPath = Path.Combine(InstallRoot, entryFileName);

                        // Create the directory for the file if it doesn't already exist.
                        Directory.CreateDirectory(Path.GetDirectoryName(fullZipToPath));

                        // 4K is optimum (according to the SharpZipLib examples).
                        var buffer = new byte[4096];

                        // Unzip file in buffered chunks. This is just as fast as unpacking
                        // to a buffer the full size of the file, but does not waste memory.
                        // The "using" will close the stream even if an exception occurs.
                        using (var zipStream = zipFile.GetInputStream(zipEntry))
                        using (Stream fsOutput = File.Create(fullZipToPath))
                        {
                            StreamUtils.Copy(zipStream, fsOutput, buffer);
                        }
                    }
                }
            };
        }
    }
}