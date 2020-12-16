using System.Collections.Generic;
using System.IO;
using Ripgrep.Editor;
using Unity.QuickSearch;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace FindReferences.Editor
{
    public class FindReferences
    {
        [MenuItem("Assets/Find All References (debug)")]
        public static void ForCurrentSelection()
        {
            if (Selection.assetGUIDs.Length == 0)
            {
                Debug.LogWarning("Select an asset first in order to find references");
            }

            foreach (var guid in Selection.assetGUIDs)
            {
                var name = Path.GetFileName(AssetDatabase.GUIDToAssetPath(guid));

                var startTime = Time.realtimeSinceStartup;
                var references = ForGuid(guid);

                var elapsedTime = Time.realtimeSinceStartup - startTime;
                if (references != null)
                {
                    Debug.Log($"Found {references.Count} reference(s) to {name} (took {elapsedTime:0.##} secs):\n" + string.Join("\n", references));
                }
            }
        }

        [Shortcut("Quick Search/Find References")]
        [MenuItem("Assets/Find References")]
        public static void PopQuickSearch()
        {
            QuickSearch.OpenWithContextualProvider("references");
        }

        [SearchItemProvider]
        internal static SearchProvider CreateProvider()
        {
            return new SearchProvider("references", "Find References")
            {
                filterId = "ref:",
                fetchItems = (context, items, provider) =>
                {
                    return null;
                },
            };
        }

        public static List<string> ForGuid(string guid)
        {
            var search = new Search();
            search.Args = $@"--files-with-matches --no-text --glob !**/*.meta ""{guid}"" Assets/";
            return search.Run();
        }
    }
}
