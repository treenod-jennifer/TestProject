using UnityEngine;
using UnityEditor;
using Spine.Unity;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class SpineMaker : EditorWindow
{
    private string folderName = "spc_";

    Vector3 emoticonOffset = new Vector3(0, 0, 0);
    private float _scale = 0;

    private GUIStyle editorStyle;
    int tabIndex = 0;

    #region makerUI
    [MenuItem("EditorUtility/SpineMaker")]
    private static void OpenWindow()
    {
        SpineMaker window = GetWindow<SpineMaker>();
        window.titleContent = new GUIContent("Spine Prefab Maker");
        window.maxSize = new Vector2(400f, 240f);
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
        float line1 = 30f;
        float line2 = 60f;
        float line2_row = 190f;
        float line3 = 90f;

        folderName = EditorGUI.TextArea(new Rect(10f, line1, 120, 22), folderName);
        EditorGUI.LabelField(new Rect(10f, line2, 200, 22), "(ImportData) Default Scale :");
        _scale = EditorGUI.FloatField(new Rect(line2_row + 10f, line2, 44, 22), _scale);
        EditorGUI.LabelField(new Rect(10f, line3, 200, 22), "(ImportData) Emoticon Offset :");
        emoticonOffset = EditorGUI.Vector3Field(new Rect(190, 93, 160, 160), "", emoticonOffset);

        GUILayout.Space(135f);
        GUILayout.BeginHorizontal();
        GUILayout.Space(70f);
        if (GUILayout.Button("Spine Object 만들기", GUILayout.Width(130), GUILayout.Height(40)))
            OnMakeButtonClicked(folderName);
        if (GUILayout.Button("ImportData 만들기", GUILayout.Width(130), GUILayout.Height(40)))
            OnMakeImportDataButtonClicked();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Space(70f);
        if (GUILayout.Button("Spine Object/ImportData 만들기", GUILayout.Width(263), GUILayout.Height(40)))
            OnMakeSpineImportDataButtonClicked();
        GUILayout.EndHorizontal();
    }
#endregion

    #region Spine Make
    public void OnMakeButtonClicked(string folderName)
    {
        string prefabPath = $"Assets/5_OutResource/SpineChars/{folderName}/{folderName}.prefab";
        if (!Directory.Exists($"Assets/5_OutResource/SpineChars/{folderName}"))
        {
            Debug.Log("<b><size=16><color=red>Error : 폴더가 없습니다.</color></size></b>");
            return;
        }
        if (File.Exists(prefabPath))
        {
            Debug.Log("<b><size=16><color=red>Error : Spine Object가 이미 존재합니다.</color></size></b>");
            return;
        }

        List<string> assetList = Directory.GetFiles($"Assets/5_OutResource/SpineChars/{folderName}/", "*.asset").ToList();
        SkeletonDataAsset skeletonAsset = null;
        foreach (string _path in assetList)
        {
            var asset = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(_path);
            if (asset != null)
            {
                skeletonAsset = asset;
                break;
            }
        }

        if (skeletonAsset == null)
        {
            Debug.Log("<b><size=16><color=red>Error : Skeleton Object가 없습니다.</color></size></b>");
            return;
        }
        MakeSpinePrefab(skeletonAsset, folderName);
    }

    private void MakeSpinePrefab(SkeletonDataAsset skeletonAsset, string folderName)
    {
        string prefabPath = $"Assets/5_OutResource/SpineChars/{folderName}/{folderName}.prefab";
        GameObject newGameObject = new GameObject(folderName);
        newGameObject.AddComponent<LAppModelProxy>();
        newGameObject.layer = 5;
        SkeletonAnimation skeletonObj = SkeletonAnimation.NewSkeletonAnimationGameObject(skeletonAsset);
        skeletonObj.transform.parent = newGameObject.transform;
        skeletonObj.transform.localScale = new Vector3(0.057f, 0.057f, 1f);
        skeletonObj.gameObject.layer = 5;
        newGameObject.GetComponent<LAppModelProxy>()._skeletonAnim = skeletonObj;
        PrefabUtility.SaveAsPrefabAsset(newGameObject, prefabPath);
        DestroyImmediate(newGameObject);
        Debug.Log($"<b><size=16><color=lime>{folderName} 생성 완료</color></size></b>");
    }

    public void OnMakeImportDataButtonClicked()
    {
        string dirPath = $"Assets/5_OutResource/SpineChars/{folderName}";
        string prefabPath = $"Assets/5_OutResource/SpineChars/{folderName}/{folderName}.prefab";
        string savePath = $"Assets/5_OutResource/SpineChars/{folderName}/ImportData.prefab";
        if (folderName == "" || !Directory.Exists(dirPath))
        {
            Debug.Log("<b><size=16><color=red>Error : 폴더가 없습니다.</color></size></b>");
            return;
        }
        if (!File.Exists(prefabPath))
        {
            Debug.Log("<b><size=16><color=red>Error : Spine Object가 없습니다.</color></size></b>");
            return;
        }
        if (File.Exists(savePath))
        {
            Debug.Log("<b><size=16><color=orange>Warning : ImportData를 덮어 씁니다.</color></size></b>");
        }

        MakeImportData(folderName);
    }

    public void OnMakeSpineImportDataButtonClicked()
    {
        string dirPath = $"Assets/5_OutResource/SpineChars/{folderName}";
        string prefabPath = $"Assets/5_OutResource/SpineChars/{folderName}/{folderName}.prefab";
        string savePath = $"Assets/5_OutResource/SpineChars/{folderName}/ImportData.prefab";
        if (folderName == "" || !Directory.Exists(dirPath))
        {
            Debug.Log("<b><size=16><color=red>Error : 폴더가 없습니다.</color></size></b>");
            return;
        }

        //스파인 파일 생성
        if (File.Exists(prefabPath))
            Debug.Log("<b><size=16><color=red>Error : Spine Object가 이미 존재합니다.</color></size></b>");
        else
            OnMakeButtonClicked(folderName);

        //ImportData 생성
        if (File.Exists(savePath))
            Debug.Log("<b><size=16><color=orange>Warning : ImportData를 덮어 씁니다.</color></size></b>");
        MakeImportData(folderName);
    }

    private void MakeImportData(string folderName)
    {
        string prefabPath = $"Assets/5_OutResource/SpineChars/{folderName}/{folderName}.prefab";
        string savePath = $"Assets/5_OutResource/SpineChars/{folderName}/ImportData.prefab";

        List<string> soundList = Directory.GetFiles($"Assets/5_OutResource/SpineChars/{folderName}/", "*.wav").ToList();
        List<AudioClip> clipList = new List<AudioClip>();
        foreach (string path in soundList)
        {
            AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (audioClip != null)
                clipList.Add(audioClip);
        }

        ImportData_Live2D importData = new GameObject().AddComponent<ImportData_Live2D>();
        GameObject live2DObj = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        importData.data = new ImportData_Live2D.ImportData()
        {
            obj = live2DObj,
            defaultScale = _scale,
            emoticonOffset = emoticonOffset,
            chatSound = clipList
        };
        PrefabUtility.SaveAsPrefabAsset(importData.gameObject, savePath);
        DestroyImmediate(importData.gameObject);
        Debug.Log($"<b><size=16><color=lime>{folderName} ImportData 생성, 사운드 파일 {clipList.Count}개 적용 완료.</color></size></b>");
    }

    #endregion
}
