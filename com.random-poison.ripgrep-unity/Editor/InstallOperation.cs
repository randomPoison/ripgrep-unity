using System;
using System.Collections;
using System.IO;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;
using UnityEngine.Networking;

namespace Ripgrep.Editor
{
    public class InstallOperation : IEnumerator
    {
        private string _downloadUrl;
        private string _installRoot;
        private ArchiveType _archiveType;
        private string _archiveExtension;

        public bool IsDone { get; private set; } = false;
        public bool Succeeded { get; private set; } = false;
        public Exception OperationException { get; private set; }

        public event Action Completed;

        public InstallOperation(string downloadUrl, string installRoot, ArchiveType archiveType)
        {
            _downloadUrl = downloadUrl;
            _installRoot = installRoot;
            _archiveType = archiveType;

            switch (_archiveType)
            {
                case ArchiveType.Zip:
                    _archiveExtension = "zip";
                    break;

                case ArchiveType.Tgz:
                    _archiveExtension = "tar.gz";
                    break;

                default:
                    throw new ArgumentException(
                        $"Unknown archive type {archiveType}",
                        nameof(archiveType));
            }
        }

        public void Start()
        {
            var request = UnityWebRequest.Get(_downloadUrl);
            request.SendWebRequest().completed += _ =>
            {
                try
                {
                    if (request.isNetworkError || request.isHttpError)
                    {
                        Debug.LogError($"Failed to download archive: {request.error}");
                        Succeeded = false;
                        return;
                    }

                    var downloadPath = Path.Combine(_installRoot, $"ripgrep.{_archiveExtension}");

                    // Create the directory for the file if it doesn't already exist.
                    Directory.CreateDirectory(Path.GetDirectoryName(downloadPath));

                    // TODO: Use a more efficient method for transferring the download bytes.
                    // Ideally this would mean streaming the downloaded bytes directly into
                    // the unzipper, but if we have to unzip from a file then at least
                    // streaming the bytes into a file without buffering them in-memory would
                    // be an improvement.
                    File.WriteAllBytes(downloadPath, request.downloadHandler.data);

                    switch (_archiveType)
                    {
                        case ArchiveType.Zip:
                            var unzipper = new FastZip();
                            unzipper.ExtractZip(downloadPath, _installRoot, null);
                            break;

                        case ArchiveType.Tgz:
                            using (var inStream = File.OpenRead(downloadPath))
                            using (var gzipStream = new GZipInputStream(inStream))
                            using (var tarArchive = TarArchive.CreateInputTarArchive(gzipStream, Encoding.UTF8))
                            {
                                tarArchive.ExtractContents(_installRoot);
                            }
                            break;

                        default:
                            throw new NotSupportedException($"Unknown archive type {_archiveType}");
                    }


                    Succeeded = true;
                }
                catch (Exception exception)
                {
                    OperationException = exception;
                    Succeeded = false;
                }
                finally
                {
                    IsDone = true;
                    Completed?.Invoke();
                }
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
