using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class FindReferenceTool
{
    public static void DrawLine()
    {
        EditorGUILayout.Space();

        var rect = EditorGUILayout.BeginHorizontal();
        Handles.color = Color.gray;
        Handles.DrawLine(new Vector2(rect.x + 5, rect.y), new Vector2(rect.width - 5, rect.y));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }
    
    public static void DrawSearchType(ref FindReferenceEditor.SearchType searchType)
    {
        GUILayout.Label("Search type", EditorStyles.boldLabel);
        searchType = (FindReferenceEditor.SearchType)EditorGUILayout.EnumPopup(searchType);
        EditorGUILayout.Space();
    }
    
    public static void StartProgress(string title, int value, int max)
    {
        float progress = (float) value / max;
        EditorUtility.DisplayProgressBar(
            title,
            value.ToString() + "(" + (progress * 100).ToString("F2") + "%)",
            progress
        );
    }

    public static void CloseProgress()
    {
        EditorUtility.ClearProgressBar();
    }

    private static T[] GetAllActiveInProject<T>()
    {
        List<T> result = new List<T>();
        string[] arr = Directory.GetFiles(Directory.GetCurrentDirectory() + "/Assets/", "*.prefab",
            SearchOption.AllDirectories);
        for (var i = 0; i < arr.Length; i++)
        {
            var FullPath = arr[i];
            if (!FullPath.Contains(".meta"))
            {
                int index1 = FullPath.IndexOf("Assets/", StringComparison.Ordinal);
                string path = FullPath.Substring(index1);
                path = path.Replace("\\", "/");

                GameObject prefab = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                if (prefab != null)
                {
                    result.AddRange(prefab.GetComponentsInChildren<T>(true));
                }
            }
        }

        return result.ToArray();
    }

    #region Searching In Prefab
    
    public static void BacktraceSpriteSelection(string spriteName, string atlasName, List<UISprite> referencingSelectionSprite)
    {
        if (string.IsNullOrEmpty(spriteName))
            return;

        var prefabs = GetAllActiveInProject<UISprite>();
        if (prefabs == null)
            return;

        referencingSelectionSprite.Clear();

        foreach (UISprite sprite in prefabs)
        {
            if (sprite != null && string.Equals(sprite.spriteName, spriteName) &&
                !string.IsNullOrEmpty(atlasName) && sprite.atlas != null && string.Equals(sprite.atlas.texture.name, atlasName))
            {
                if (!referencingSelectionSprite.Contains(sprite))
                {
                    referencingSelectionSprite.Add(sprite);
                }
            }
        }
    }
    
    public static Vector2 DrawReferenceList(string title, List<UISprite> referenceList, Vector2 scrollPosition)
    {
        GUILayout.Label(title, EditorStyles.boldLabel);
        if (referenceList == null || referenceList.Count == 0)
        {
            GUILayout.Label("is not referenced");
        }
        else
        {
            EditorGUILayout.BeginVertical(new GUIStyle("RL Background"));
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (var item in referenceList)
            {
                EditorGUILayout.ObjectField(item.GetType().ToString(), item, typeof(GameObject),
                    allowSceneObjects: true);
                EditorGUILayout.TextField(AssetDatabase.GetAssetPath(item));
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        return scrollPosition;
    }
    
    #endregion

    #region Searching In Scene
    
    public static void BacktraceSpriteSelection(string spriteName, List<string> referencingSelectionPath)
    {
        string[] arr = Directory.GetFiles(Directory.GetCurrentDirectory() + "/Assets/", "*.unity",
            SearchOption.AllDirectories);
        for (var i = 0; i < arr.Length; i++)
        {
            var fullPath = arr[i];
            StreamReader reader = new StreamReader(fullPath);
            string result = reader.ReadToEnd();

            if (result.Contains("mSpriteName: " + spriteName))
            {
                int index1 = fullPath.IndexOf("Assets/", StringComparison.Ordinal);
                string path = fullPath.Substring(index1);
                path = path.Replace("\\", "/");
                referencingSelectionPath.Add(path);
            }
        }
    }
    
    public static Vector2 DrawReferenceList(string title, List<string> referenceList, Vector2 scrollPosition)
    {
        GUILayout.Label(title, EditorStyles.boldLabel);

        if ((referenceList == null || referenceList.Count == 0))
        {
            GUILayout.Label("is not referenced by any scenes");
        }
        else
        {
            EditorGUILayout.BeginVertical(new GUIStyle("RL Background"));
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            foreach (var path in referenceList)
            {
                EditorGUILayout.TextField(path);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        return scrollPosition;
    }

    #endregion

    #region Searching Object
    
    public static void FindAllReferences(Dictionary<string, List<string>> referenceCacheList, string folders = "")
    {
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        string[] guids;
        if (!string.IsNullOrEmpty(folders))
        {
            guids = AssetDatabase.FindAssets("", new []{folders});
        }
        else
        {
            guids = AssetDatabase.FindAssets("");   
        }

        for (var i = 0; i < guids.Length; i++)
        {
            string guid = guids[i];
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string[] dependencies = AssetDatabase.GetDependencies(assetPath, false);

            foreach (var dependency in dependencies)
            {
                if (referenceCacheList.ContainsKey(dependency))
                {
                    if (!referenceCacheList[dependency].Contains(assetPath))
                    {
                        referenceCacheList[dependency].Add(assetPath);
                    }
                }
                else
                {
                    referenceCacheList[dependency] = new List<string>() {assetPath};
                }
            }
            
            StartProgress("Find All References", i, guids.Length);
        }
        
        CloseProgress();
    }

    public static Vector2 DrawReferenceList(string title, Dictionary<string, List<string>> referenceCacheList, Vector2 scrollPosition, UnityEngine.Object selectObject)
    {
        GUILayout.Label(title, EditorStyles.boldLabel);

        if ((referenceCacheList == null || referenceCacheList.Count == 0 || selectObject == null))
        {
            GUILayout.Label("is not referenced");
        }
        else
        {
            string path = AssetDatabase.GetAssetPath(selectObject);
            if (!referenceCacheList.ContainsKey(path))
            {
                GUILayout.Label("is not referenced");
            }
            else
            {
                EditorGUILayout.BeginVertical(new GUIStyle("RL Background"));
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                foreach (var reference in referenceCacheList[path])
                {
                    var obj = AssetDatabase.LoadMainAssetAtPath(reference);
                    EditorGUILayout.ObjectField(obj.GetType().ToString(), obj, typeof(GameObject), allowSceneObjects: true);
                    EditorGUILayout.TextField(AssetDatabase.GetAssetPath(obj));
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
        }

        return scrollPosition;
    }

    #endregion
}
