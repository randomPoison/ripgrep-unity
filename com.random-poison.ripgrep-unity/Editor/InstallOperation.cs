using System;
using System.Collections;
using System.IO;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;
using UnityEngine.Networking;

namespace Ripgrep.Editor
{
    public class InstallOperation : IEnumerator
    {
        private string _downloadUrl;
        private string _installRoot;

        public bool IsDone { get; private set; } = false;
        public bool Succeeded { get; private set; } = false;

        public event Action Completed;

        public InstallOperation(string downloadUrl, string installRoot)
        {
            _downloadUrl = downloadUrl;
            _installRoot = installRoot;
        }

        public void Start()
        {
            var request = UnityWebRequest.Get(_downloadUrl);
            request.SendWebRequest().completed += _ =>
            {
                if (request.isNetworkError || request.isHttpError)
                {
                    Debug.LogError($"Failed to download archive: {request.error}");
                    Completed?.Invoke();
                    IsDone = true;
                    Succeeded = false;
                    return;
                }

                var downloadPath = Path.Combine(_installRoot, "ripgrep.zip");

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
                        var fullZipToPath = Path.Combine(_installRoot, entryFileName);

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

                IsDone = true;
                Succeeded = true;
                Completed?.Invoke();
            };
        }

        #region IEnumerator

        object IEnumerator.Current => null;

        bool IEnumerator.MoveNext()
        {
            return !IsDone;
        }

        void IEnumerator.Reset() { }

        #endregion
    }
}
