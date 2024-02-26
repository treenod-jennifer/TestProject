using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

[CustomEditor(typeof(AreaTilemapHoleUtility))]
public class AreaTilemapHoleUtilityEditor : Editor
{
    private AreaTilemapHoleUtility holeUtility;

    private void OnEnable()
    {
        holeUtility = target as AreaTilemapHoleUtility;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!PrefabUtility.IsPartOfPrefabAsset(target))
        {
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Load"))
            {
                if (!holeUtility.LoadHoleData())
                {
                    EditorUtility.DisplayDialog("경고", "파일이 없거나, 올바르지 않습니다.", "확인");
                }
            }

            if (GUILayout.Button("Save"))
            {
                var path = EditorUtility.SaveFilePanel(
                    "Hole Data Save",
                    "Assets",
                    $"{holeUtility.name}_Hole",
                    "json");

                if (path.Length != 0)
                {
                    string jsonHoleData = holeUtility.GetHoleData();
                    SaveJsonFile(path, jsonHoleData);
                    AssetDatabase.Refresh();
                }
            }

            GUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.HelpBox("정상적인 접근 방법이 아닙니다", MessageType.Warning);
        }
    }

    private void SaveJsonFile(string path, string jsonText)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(jsonText);

        path = Path.Combine(Application.persistentDataPath, path);

        if (bytes == null)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            return;
        }

        try
        {
            using (FileStream file = File.Create(path))
            {
                file.Write(bytes, 0, bytes.Length);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }
}
