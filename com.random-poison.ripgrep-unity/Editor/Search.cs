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

        public bool IsDone => _searchProcess?.HasExited ?? false;
        public List<string> Result => IsDone ? _matches : null;

        private Process _searchProcess;
        private List<string> _matches = new List<string>();

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
            if (!File.Exists(Installer.WindowsBinPath))
            {
                Debug.LogWarning($"ripgrep not installed, please run installer before doing an asset search");

                // TODO: `IsDone` won't return a correct result in this case, since `_searchProcess`
                // will be null. Determine how to handle this case correctly. We likely need a way
                // to return error information.
                return;
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = Path.GetFullPath(Installer.WindowsBinPath),
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
                InvokeOnMainThread(() => { Completed?.Invoke(_matches); });
            };

            // Run ripgrep.
            _searchProcess.Start();
            _searchProcess.BeginOutputReadLine();
        }

        public List<string> RunAndWaitForResults()
        {
            Run();
            while (!IsDone) { }
            return _matches;
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
