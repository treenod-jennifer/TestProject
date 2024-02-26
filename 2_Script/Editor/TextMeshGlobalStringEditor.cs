using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TextMeshGlobalString))]
public class TextMeshGlobalStringEditor : Editor
{
    private TextMeshGlobalString TextMeshGlobalStringTarget = null;

    private GameObject TestTMP
    {
        get
        {
            var tmp = TextMeshGlobalStringTarget.GetComponentInChildren<TMPro.TMP_Text>();

            if (tmp == null)
            {
                return null;
            }
            else
            {
                return tmp.gameObject;
            }
        }
    }

    private void OnEnable()
    {
        TextMeshGlobalStringTarget = target as TextMeshGlobalString;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Test"))
        {
            try
            {
                var oldTestTMP = TestTMP;
                if (oldTestTMP != null)
                {
                    DestroyImmediate(oldTestTMP);
                }

                var path = serializedObject.FindProperty("tmpPath");

                var tmpTemplate = Resources.Load(path.stringValue);

                if (tmpTemplate == null)
                {
                    throw new System.Exception("경로에 TMP 오브젝트가 없습니다.");
                }

                var tmpObject = Instantiate(tmpTemplate, TextMeshGlobalStringTarget.transform) as GameObject;
                tmpObject.hideFlags = HideFlags.DontSave;
                tmpObject.name = tmpObject.name.Replace("(Clone)", "(Dont Save Mode)");

                var targetTexture = serializedObject.FindProperty("texture");
                var uiTexture = targetTexture.objectReferenceValue as UITexture;
                var tmpText = tmpObject.GetComponent<TMPro.TMP_Text>();

                tmpText.rectTransform.sizeDelta = new Vector2(uiTexture.width - 20, uiTexture.height - 30);
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("경고", e.Message, "확인");
            }
        }
    }
}
