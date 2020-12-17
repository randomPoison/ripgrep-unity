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
            if (Selection.assetGUIDs.Length > 0)
            {
                var search = QuickSearch.OpenWithContextualProvider("references");
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
                toObject = (item, type) => AssetDatabase.LoadAssetAtPath(item.id, type),
                openContextual = (selection, rect) => OpenContextualMenu(selection, rect),

                showDetails = true,
                showDetailsOptions = ShowDetailsOptions.Default | ShowDetailsOptions.Inspector,
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

            foreach (var assetPath in search.Result)
            {
                yield return provider.CreateItem(context, assetPath, assetPath, null, null, null);
            }
        }

        public static Search ForGuid(string guid)
        {
            var search = new Search();
            search.Args = $@"--files-with-matches --no-text --glob !**/*.meta ""{guid}"" Assets/";
            search.Run();
            return search;
        }

        private static bool OpenContextualMenu(SearchSelection selection, Rect contextRect)
        {
            var old = Selection.instanceIDs;
            SearchUtils.SelectMultipleItems(selection);
            EditorUtility.DisplayPopupMenu(contextRect, "Assets/", null);
            EditorApplication.delayCall += () => EditorApplication.delayCall += () => Selection.instanceIDs = old;
            return true;
        }
    }
}
