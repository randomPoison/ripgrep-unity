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
                ForGuid(guid).Completed += references =>
                {
                    var elapsedTime = Time.realtimeSinceStartup - startTime;
                    Debug.Log($"Found {references.Count} reference(s) to {name} (took {elapsedTime:0.##} secs):\n" + string.Join("\n", references));
                };
            }
        }

        [Shortcut("Quick Search/Find References")]
        [MenuItem("Assets/Find References")]
        public static void PopQuickSearch()
        {
            var search = QuickSearch.OpenWithContextualProvider("references");

            if (Selection.assetGUIDs.Length > 0)
            {
                search.SetSearchText(AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]));
            }
        }

        [SearchItemProvider]
        internal static SearchProvider CreateProvider()
        {
            return new SearchProvider("references", "Find References")
            {
                filterId = "ref:",
                isExplicitProvider = true,
                fetchItems = (context, items, provider) => FetchItems(context, provider),
            };
        }

        private static IEnumerable<SearchItem> FetchItems(SearchContext context, SearchProvider provider)
        {
            var guid = AssetDatabase.AssetPathToGUID(context.searchText);
            if (guid == null)
            {
                yield break;
            }

            var search = ForGuid(guid);
            while (!search.IsDone)
            {
                yield return null;
            }

            foreach (var asset in search.Result)
            {
                yield return new SearchItem(asset)
                {
                    label = asset,
                    provider = provider,
                };
            }
        }

        public static Search ForGuid(string guid)
        {
            var search = new Search();
            search.Args = $@"--files-with-matches --no-text --glob !**/*.meta ""{guid}"" Assets/";
            search.Run();
            return search;
        }
    }
}
