using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Ripgrep.Editor
{
    /// <summary>
    /// 
    /// </summary>
    public class Search : IEnumerator
    {
        /// <summary>
        /// The raw arguments passed to the <c>rg</c> executable.
        /// </summary>
        ///
        /// <remarks>
        /// Arguments are represented as they would be when using <c>rg</c>
        /// </remarks>
        public string Args { get; set; }

        public event Action<string> MatchFound;
        public event Action<List<string>> Completed;

        public bool IsDone { get; private set; } = false;
        public List<string> Result { get; private set; } = null;

        private Process _searchProcess;
        private HashSet<string> _matches = new HashSet<string>();

        public Search()
        {
            // TODO: Provide an option to automatically install Ripgrep before running
            // the search if it's not already installed.
            if (!Installer.IsInstalled)
            {
                throw new InvalidOperationException(
                    "Ripgrep executable is not installed, please run the installer");
            }
        }

        public void Run()
        {
            if (!Installer.IsInstalled)
            {
                Debug.LogWarning($"ripgrep not installed, please run installer before doing an asset search");

                IsDone = true;
                return;
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = Path.GetFullPath(Installer.BinPath),
                CreateNoWindow = true,
                Arguments = Args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };

            _searchProcess = new Process
            {
                StartInfo = processStartInfo,
                EnableRaisingEvents = true,
            };

            _searchProcess.OutputDataReceived += (sender, args) =>
            {
                var path = args.Data;

                if (string.IsNullOrWhiteSpace(path)) return;

                // NOTE: Unity follows the convention that paths always use '/' as the separator
                // character, even on Windows where '\' is the convention. To keep things
                // consistent for Unity users, we normalize the paths returned by Ripgrep to always
                // use '/'.
                path = path.Replace("\\", "/");

                InvokeOnMainThread(() =>
                {
                    _matches.Add(path);
                    MatchFound?.Invoke(path);
                });
            };

            _searchProcess.Exited += (sender, args) =>
            {
                // TODO: Check the process to see if it exited successfully.
                InvokeOnMainThread(() =>
                {
                    // Sort the list of results so that we can produce deterministic output.
                    Result = new List<string>(_matches);
                    Result.Sort();

                    // Broadcast the result of the search.
                    IsDone = true;
                    Completed?.Invoke(Result);
                });
            };

            // Run ripgrep.
            _searchProcess.Start();
            _searchProcess.BeginOutputReadLine();
        }

        /// <summary>
        /// Helper function that defers some action until the next editor update.
        /// </summary>
        ///
        /// <param name="action">The action to invoke on the main editor thread.</param>
        private static void InvokeOnMainThread(Action action)
        {
            EditorApplication.update += InvokeOnce;

            void InvokeOnce()
            {
                try
                {
                    action?.Invoke();
                }
                finally
                {
                    EditorApplication.update -= InvokeOnce;
                }
            }
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
