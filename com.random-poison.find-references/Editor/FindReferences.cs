using System.Collections.Generic;
using System.IO;
using Ripgrep.Editor;
using UnityEditor;
using UnityEngine;

namespace FindReferences.Editor
{
    public class FindReferences
    {
        [MenuItem("Assets/Find All References")]
        public static void ForCurrentSelection()
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
            var name = Path.GetFileName(AssetDatabase.GUIDToAssetPath(guid));

            var startTime = Time.realtimeSinceStartup;
            var references = ForGuid(guid);

            var elapsedTime = Time.realtimeSinceStartup - startTime;
            if (references != null)
            {
                Debug.Log($"Found {references.Count} reference(s) to {name} (took {elapsedTime:0.##} secs):\n" + string.Join("\n", references));
            }
        }

        public static List<string> ForGuid(string guid)
        {
            var search = new Search();
            search.Args = $@"--files-with-matches --no-text --glob !**/*.meta ""{guid}"" Assets/";
            return search.Run();
        }
    }
}
