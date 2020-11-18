using UnityEditor;
using UnityEngine;
using Ripgrep.Editor;
using System.Collections.Generic;

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
            var references = ForGuid(guid);

            if (references != null)
            {
                Debug.Log($"{references.Count} references:\n" + string.Join("\n", references));
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
