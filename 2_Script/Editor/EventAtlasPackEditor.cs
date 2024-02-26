using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EventAtlasPack))]
public class EventAtlasPackEditor : Editor
{
    private SerializedProperty blockAtlasObjProperty;
    private SerializedProperty block2AtlasObjProperty;
    private SerializedProperty effectAtlasObjProperty;
    private SerializedProperty ingameUIAtlasObjProperty;

    private void OnEnable()
    {
        blockAtlasObjProperty    = serializedObject.FindProperty("blockAtlasObj");
        block2AtlasObjProperty   = serializedObject.FindProperty("block2AtlasObj");
        effectAtlasObjProperty   = serializedObject.FindProperty("effectAtlasObj");
        ingameUIAtlasObjProperty = serializedObject.FindProperty("ingameUIAtlasObj");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUI.changed)
        {
            CheckData();

            EditorUtility.SetDirty(target);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            serializedObject.ApplyModifiedProperties();
        }
    }

    private void CheckData()
    {
        bool blockAtlasCheck    = CheckINGUIAtlas(blockAtlasObjProperty.objectReferenceValue);
        bool block2AtlasCheck   = CheckINGUIAtlas(block2AtlasObjProperty.objectReferenceValue);
        bool effectAtlasCheck   = CheckINGUIAtlas(effectAtlasObjProperty.objectReferenceValue);
        bool ingameUIAtlasCheck = CheckINGUIAtlas(ingameUIAtlasObjProperty.objectReferenceValue);

        if (!blockAtlasCheck)    blockAtlasObjProperty.objectReferenceValue     = TryGetAtlas(blockAtlasObjProperty.objectReferenceValue);
        if (!block2AtlasCheck)   block2AtlasObjProperty.objectReferenceValue    = TryGetAtlas(block2AtlasObjProperty.objectReferenceValue);
        if (!effectAtlasCheck)   effectAtlasObjProperty.objectReferenceValue    = TryGetAtlas(effectAtlasObjProperty.objectReferenceValue);
        if (!ingameUIAtlasCheck) ingameUIAtlasObjProperty.objectReferenceValue  = TryGetAtlas(ingameUIAtlasObjProperty.objectReferenceValue);
    }

    private bool CheckINGUIAtlas(Object obj)
    {
        if (obj == null)
            return true;

        INGUIAtlas atlas = obj as INGUIAtlas;
        return atlas != null;
    }

    private UIAtlas TryGetUIAtlas(Object obj)
    {
        GameObject gameObject = obj as GameObject;

        if (gameObject == null)
            return null;

        UIAtlas atlas = gameObject.GetComponent<UIAtlas>();
        return atlas;
    }

    private NGUIAtlas TryGetNGUIAtlas(Object obj)
    {
        NGUIAtlas atlas = obj as NGUIAtlas;
        return atlas;
    }

    private Object TryGetAtlas(Object obj)
    {
        Object atlas = TryGetUIAtlas(obj);
        if (atlas == null)
            atlas = TryGetNGUIAtlas(obj);

        return atlas;
    }
}
