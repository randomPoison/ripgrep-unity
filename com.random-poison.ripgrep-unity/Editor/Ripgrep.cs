using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Ripgrep.Editor
{
    public class Ripgrep
    {
        // TODO: Split this into a separate package.
        [MenuItem("Assets/Find All References")]
        public static void FindReferences()
        {
            if (Selection.assetGUIDs.Length == 0)
            {
                Debug.LogWarning("Select an asset first in order to find references");
            }

            if (Selection.assetGUIDs.Length > 1)
            {
                Debug.LogWarning(
                    "More than one asset selected, can only find references for one asset at a " +
                    "time");
            }

            var guid = Selection.assetGUIDs[0];
            var references = SearchProject(guid);

            if (references != null)
            {
                Debug.Log($"{references.Count} references:\n" + string.Join("\n", references));
            }
        }

        public static List<string> SearchProject(string pattern)
        {
            if (!File.Exists(Installer.BinPath))
            {
                Debug.LogWarning($"ripgrep not installed, please run installer before doing an asset search");
                return null;
            }

            var arguments = $@"--files-with-matches --no-text ""{pattern}"" Assets/";
            var processStartInfo = new ProcessStartInfo
            {
                FileName = Path.GetFullPath(Installer.BinPath),
                CreateNoWindow = true,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };

            var references = new List<string>();
            var process = new Process { StartInfo = processStartInfo };

            process.OutputDataReceived += (sender, args) =>
            {
                var path = args.Data;

                if (string.IsNullOrWhiteSpace(path)) return;

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

