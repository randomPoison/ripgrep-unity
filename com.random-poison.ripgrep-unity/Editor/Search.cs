using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;

namespace Ripgrep.Editor
{
    /// <summary>
    /// 
    /// </summary>
    public class Search
    {
        /// <summary>
        /// The raw arguments passed to the <c>rg</c> executable.
        /// </summary>
        ///
        /// <remarks>
        /// Arguments are represented as they would be when using <c>rg</c>
        /// </remarks>
        public string Args { get; set; }

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

        // TODO: Make this 
        public List<string> Run()
        {
            if (!File.Exists(Installer.WindowsBinPath))
            {
                Debug.LogWarning($"ripgrep not installed, please run installer before doing an asset search");
                return null;
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = Path.GetFullPath(Installer.WindowsBinPath),
                CreateNoWindow = true,
                Arguments = Args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };

            var references = new List<string>();
            var process = new Process { StartInfo = processStartInfo };

            process.OutputDataReceived += (sender, args) =>
            {
                var path = args.Data;

                if (string.IsNullOrWhiteSpace(path)) return;

                // NOTE: Unity follows the convention that paths always use '/' as the separator
                // character, even on Windows where '\' is the convention. To keep things
                // consistent for Unity users, we normalize the paths returned by Ripgrep to always
                // use '/'.
                references.Add(path.Replace("\\", "/"));
            };

            // Run ripgrep.
            process.Start();
            process.BeginOutputReadLine();

            // Wait for ripgrep to finish.
            //
            // TODO: Offer an async version that doesn't block.
            while (!process.HasExited)
            {
            }

            return references;
        }
    }
}

