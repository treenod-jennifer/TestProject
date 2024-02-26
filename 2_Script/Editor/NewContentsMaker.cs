using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class NewContentsMaker : EditorWindow
{
    private string existName = "";
    private string newName = "";
    private string existWord = "";
    private string newWord = "";
    private string descriptionName = "";

    private GUIStyle editorStyle;

    #region makerUI
    [MenuItem("EditorUtility/New Contents Maker")]
    private static void OpenWindow()
    {
        NewContentsMaker window = GetWindow<NewContentsMaker>();
        window.titleContent = new GUIContent("New Contents Maker");
        window.maxSize = new Vector2(440f, 380f);
        window.minSize = window.maxSize;
    }

    private void OnEnable()
    {
        hideFlags = HideFlags.HideAndDontSave;

        editorStyle = new GUIStyle();
        editorStyle.normal.background = EditorGUIUtility.whiteTexture;
        editorStyle.border = new RectOffset(12, 12, 12, 12);
    }

    private void OnGUI()
    {
        float textRow = 70f;
        float textBoxRow = 240f;
        float textWidth = 200f;
        float textBoxWidth = 120f;
        float height = 22f;
        
        EditorGUI.LabelField(new Rect(10f, 20f, 420, 22), "카피 후 다른 창을 클릭했다가 다시 유니티를 클릭하면 스크립트가 적용됩니다.");
        EditorGUI.LabelField(new Rect(textRow, 60f, textWidth, height), "[제목] 기존 컨텐츠 네이밍 :");
        existName = EditorGUI.TextArea(new Rect(textBoxRow, 60f, textBoxWidth, height), existName);
        EditorGUI.LabelField(new Rect(textRow, 90f, textWidth, height), "[제목] 신규 컨텐츠 네이밍 :");
        newName = EditorGUI.TextArea(new Rect(textBoxRow, 90f, textBoxWidth, height), newName);
        EditorGUI.LabelField(new Rect(textRow, 120f, textWidth, height), "[제목] 부가 네이밍 : ");
        descriptionName = EditorGUI.TextArea(new Rect(textBoxRow, 120f, textBoxWidth, height), descriptionName);
        EditorGUI.LabelField(new Rect(textRow, 150f, textWidth, height), "[내용] 변경할 단어 전 (선택) : ");
        existWord = EditorGUI.TextArea(new Rect(textBoxRow, 150f, textBoxWidth, height), existWord);
        EditorGUI.LabelField(new Rect(textRow, 180f, textWidth, height), "[내용] 변경할 단어 후 (선택) : ");
        newWord = EditorGUI.TextArea(new Rect(textBoxRow, 180f, textBoxWidth, height), newWord);
        GUILayout.Space(220f);
        GUILayout.BeginHorizontal();
        GUILayout.Space(70f);
        if (GUILayout.Button($"Manager Script 복제\n( Manager{newName}{descriptionName}.cs )", GUILayout.Width(300), GUILayout.Height(40)))
            OnMakeManagerButtonClicked();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Space(70f);
        if (GUILayout.Button($"UI 관련 Script 복제\n( UIPopUp{newName}{descriptionName}.cs )\n( UIItem{newName}{descriptionName}.cs )", GUILayout.Width(300), GUILayout.Height(50)))
            OnMakeUIButtonClicked();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Space(70f);
        if (GUILayout.Button($"UI 관련 프리팹 복제\n( UIPopUp{newName}{descriptionName}.prefab )\n( UIItem{newName}{descriptionName}.prefab )", GUILayout.Width(300), GUILayout.Height(50)))
            OnMakePrefabButtonClicked();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
#endregion

    #region Contents Make

    private void OnMakeManagerButtonClicked()
    {
        CopyScript("2_Script/EventManager/Manager", "cs");
    }
    
    private void OnMakeUIButtonClicked()
    {
        CopyScript("2_Script/UI/UIPopUp", "cs");
        CopyScript("2_Script/UI/UIItem", "cs");
    }
    
    private void OnMakePrefabButtonClicked()
    {
        CopyScript("Resources/UIPrefab/UIPopUp", "prefab");
        CopyScript("Resources/UIPrefab/UIItem", "prefab");
    }

    private void CopyScript(string fileName, string extension)
    {
        string path = $"Assets/{fileName}{existName}{descriptionName}.{extension}";
        string copyPath = $"Assets/{fileName}{newName}{descriptionName}.{extension}";
        bool isCopy = false;
        bool isChange = false;
        if (!File.Exists(path))
        {
            Debug.Log($"<b><size=16><color=yellow>[원본 파일이 없어 복사되지 않았습니다]\n{path}</color></size></b>");
            return;
        }

        if (!File.Exists(copyPath))
        {
            FileInfo newFile = new FileInfo(path);
            newFile.CopyTo(copyPath);
            isCopy = true;
        }

        string str = File.ReadAllText(copyPath);
        while (str.Contains(existName))
            str = Regex.Replace(str,existName, newName);
        
        if (!String.IsNullOrEmpty(existWord))
        {
            if (str.Contains(existWord))
                isChange = true;
            while (str.Contains(existWord))
                str = Regex.Replace(str,existWord, newWord);
        }
        
        File.WriteAllText(copyPath, str);
        AssetDatabase.ImportAsset(copyPath, ImportAssetOptions.Default);
        
        if (isCopy)
            Debug.Log($"<b><size=16><color=green>[Copy Complete]\n{copyPath}.</color></size></b>");
        else if (isChange)
            Debug.Log($"<b><size=16><color=blue>[Change Complete]\n{copyPath}.</color></size></b>");
        else
            Debug.Log($"<b><size=16><color=yellow>[File No Change]\n{copyPath}.</color></size></b>");
    }
    #endregion
}
