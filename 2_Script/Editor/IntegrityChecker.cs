using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class IntegrityChecker
{
    private struct ErrorData
    {
        public GameObject rootObj;
        public GameObject errorObj;
        public string message;

        public string GetFilePath()
        {
            return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(errorObj);
        }

        public string GetPath()
        {
            List<string> pathData = new List<string>();
            Transform t = errorObj.transform;

            while (t != null)
            {
                pathData.Add(t.name);
                t = t.parent;
            }

            System.Text.StringBuilder path = new System.Text.StringBuilder();

            for (int i = pathData.Count - 1; i >= 0; i--)
            {
                path.Append(pathData[i] + "/");
            }

            return path.Remove(path.Length - 1, 1).ToString();
        }
    }

    private interface Integrity
    {
        GameObject[] Check(GameObject target);
        string GetMessage();
    }

    private class TransformModelChecker : Integrity
    {
        public GameObject[] Check(GameObject target)
        {
            List<GameObject> errorObjects = new List<GameObject>();

            var objBases = target.GetComponentsInChildren<ObjectBase>(true);

            foreach (var obj in objBases)
            {
                if (obj._transformModel == null)
                {
                    errorObjects.Add(obj.gameObject);
                }
            }

            return errorObjects.ToArray();
        }

        public string GetMessage()
        {
            return "_transformModel Error : _transformModel is null";
        }
    }

    private class TransformColliderChecker : Integrity
    {
        public GameObject[] Check(GameObject target)
        {
            List<GameObject> errorObjects = new List<GameObject>();

            var objBases = target.GetComponentsInChildren<ObjectBase>(true);

            foreach (var obj in objBases)
            {
                if (!(obj is ObjectStatic) && obj._transformCollider == null)
                {
                    errorObjects.Add(obj.gameObject);
                }
            }

            return errorObjects.ToArray();
        }

        public string GetMessage()
        {
            return "_transformCollider Error : _transformCollider is null";
        }
    }

    private class StampTextureChecker : Integrity
    {
        public GameObject[] Check(GameObject target)
        {
            List<GameObject> errorObjects = new List<GameObject>();

            var objBases = target.GetComponentsInChildren<Stamp>(true);

            foreach (var obj in objBases)
            {
                if (obj.stampTexture == null)
                {
                    errorObjects.Add(obj.gameObject);
                }
            }

            return errorObjects.ToArray();
        }

        public string GetMessage()
        {
            return "Stamp Error : stampTexture is null";
        }
    }

    private class UILabelChecker : Integrity
    {
        private string pattern;

        public UILabelChecker(string pattern)
        {
            this.pattern = pattern;
        }

        public GameObject[] Check(GameObject target)
        {
            List<GameObject> errorObjects = new List<GameObject>();

            var objBases = target.GetComponentsInChildren<UILabel>(true);

            foreach (var obj in objBases)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(obj.text, pattern))
                {
                    errorObjects.Add(obj.gameObject);
                }
            }

            return errorObjects.ToArray();
        }

        public string GetMessage()
        {
            return "Find Label String";
        }
    }

    private class UIGlobalStringChecker : Integrity
    {
        private string pattern;

        public UIGlobalStringChecker(string pattern)
        {
            this.pattern = pattern;
        }

        public GameObject[] Check(GameObject target)
        {
            List<GameObject> errorObjects = new List<GameObject>();

            var objBases = target.GetComponentsInChildren<UIItemGlobalString>(true);

            foreach (var obj in objBases)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(obj.Text, pattern))
                {
                    errorObjects.Add(obj.gameObject);
                }
            }

            return errorObjects.ToArray();
        }

        public string GetMessage()
        {
            return "Find Global String";
        }
    }

    private class BoniActionCharacterAnimationCheck : Integrity
    {
        private List<string> checkString = new List<string>
        {
            "anger",
            "clean_ready",
            "dissatisfaction",
            "fighting",
            "frustration",
            "grass",
            "grass_ready",
            "indifference_02",
            "joy",
            "longPaint_ready",
            "worry_01",
            "worry_02",
            "longShovel",
            "longShovel_ready",
            "look",
            "paint",
            "paint_ready",
            "pickup",
            "question",
            "rub",
            "run_bag",
            "shock",
            "shout",
            "shovel",
            "sleep",
            "stretching",
            "wateringCan",
            "wateringCan_ready",
            "wavehand",
            "wish",
            "idle_bag",
            "sit_loop",
            "liquid"
        };

        public GameObject[] Check(GameObject target)
        {
            List<GameObject> errorObjects = new List<GameObject>();

            var objBases = target.GetComponentsInChildren<ActionCharacterAnimation>(true);

            foreach (var obj in objBases)
            {
                if (obj._type == TypeCharacterType.Boni)
                {
                    foreach(var ani in obj._animation)
                    {
                        if (checkString.Contains(ani._animation))
                        {
                            errorObjects.Add(obj.gameObject);
                        }
                    }
                }
            }

            return errorObjects.ToArray();
        }

        public string GetMessage()
        {
            return "Boni Animation Check";
        }
    }


    [MenuItem("Tools/IntegrityCheck", priority = 1900)]
    public static bool Check()
    {
        ClearLog();

        GameObject[] prefabs = GetAllPrefabs("Assets/5_OutResource");
        int checkCount = BaseCheck(prefabs, new TransformModelChecker(), new TransformColliderChecker(), new StampTextureChecker());

        if (checkCount == 0)
        {
            Debug.Log("<color=green><size=22><b>Check Complete. There is no error.</b></size></color>");
            return false;
        }
        else
        {
            Debug.Log($"<color=red><size=22><b>Check Complete. found {checkCount} errors.</b></size></color>");
            return true;
        }
    }

    [MenuItem("Tools/BoniAnimationCheck", priority = 1901)]
    public static bool BoniAnimationCheck()
    {
        ClearLog();

        GameObject[] prefabs = GetAllPrefabs(
            "Assets/5_OutResource/area_0",
            "Assets/5_OutResource/area_1",
            "Assets/5_OutResource/area_2",
            "Assets/5_OutResource/area_3",
            "Assets/5_OutResource/area_4",
            "Assets/5_OutResource/area_5",
            "Assets/5_OutResource/area_6",
            "Assets/5_OutResource/area_7",
            "Assets/5_OutResource/area_8",
            "Assets/5_OutResource/area_9",
            "Assets/5_OutResource/area_10",
            "Assets/5_OutResource/area_11",
            "Assets/5_OutResource/area_12",
            "Assets/5_OutResource/area_13",
            "Assets/5_OutResource/global");
        int checkCount = BaseCheck(prefabs, new BoniActionCharacterAnimationCheck());

        if (checkCount == 0)
        {
            Debug.Log("<color=green><size=22><b>Check Complete. There is no error.</b></size></color>");
            return false;
        }
        else
        {
            Debug.Log($"<color=red><size=22><b>Check Complete. found {checkCount} errors.</b></size></color>");
            return true;
        }
    }

    private static int BaseCheck(GameObject[] prefabs, params Integrity[] integrities)
    {
        List<ErrorData[]> checkList = new List<ErrorData[]>();

        foreach (var item in integrities)
        {
            checkList.Add(AllCheck(prefabs, item));
        }

        int checkCount = 0;

        foreach (var check in checkList)
        {
            checkCount += check.Length;
            PrintError(check);
        }

        return checkCount;
    }

    private static GameObject[] GetAllPrefabs(params string[] foldersToSearch)
    {
        List<GameObject> prefabs = new List<GameObject>();

        var fileList = AssetDatabase.FindAssets("t:GameObject", foldersToSearch);

        int count = 0;
        ProgressBarWindow.Init(fileList.Length);
        ProgressBarWindow.Show(count);

        foreach (var guid in fileList)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            ProgressBarWindow.Show(++count);

            if (obj != null)
                prefabs.Add(obj);
        }

        ProgressBarWindow.Exit();

        return prefabs.ToArray();
    }

    private static ErrorData[] Check(GameObject target, Integrity checker)
    {
        List<ErrorData> errors = new List<ErrorData>();

        GameObject[] errorObjs = checker.Check(target);

        foreach (var errorObj in errorObjs)
        {
            var errorData = new ErrorData();
            errorData.rootObj = target;
            errorData.errorObj = errorObj;
            errorData.message = checker.GetMessage();

            errors.Add(errorData);
        }

        return errors.ToArray();
    }

    private static ErrorData[] AllCheck(GameObject[] targets, Integrity checker)
    {
        List<ErrorData> errors = new List<ErrorData>();

        foreach (var target in targets)
        {
            var errorsObj = Check(target, checker);

            errors.AddRange(errorsObj);
        }

        return errors.ToArray();
    }

    private static void PrintError(ErrorData[] errors)
    {
        foreach (var error in errors)
        {
            Debug.Log
            (
                "<color=orange>" + error.message + "</color>\n" +
                "<color=yellow>" + error.GetPath() + "</color>\n" +
                "<color=white>File : (" + error.GetFilePath() + ")</color>\n",
                error.rootObj
            );
        }
    }

    private static void ClearLog()
    {
        var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }


    private static class ProgressBarWindow
    {
        private const string TITLE = "AssetBundle Integrity Checke";
        private static int maxCount;

        public static void Init(int total)
        {
            maxCount = total;
        }

        public static void Show(int count)
        {
            Show(count, string.Empty);
        }

        public static void Show(int count, string info)
        {
            info = string.IsNullOrEmpty(info) ? $"({count}/{maxCount})" : $"({count}/{maxCount})\n{info}";
            EditorUtility.DisplayProgressBar(TITLE, info, (float)count / maxCount);
        }

        public static void Exit()
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private class FindStringWindow : EditorWindow
    {
        private string pattern;

        [MenuItem("Tools/StringChecke", priority = 1901)]
        public static void Open()
        {
            GetWindow<FindStringWindow>(title: "Find String");
        }

        void OnGUI()
        {
            pattern = GUILayout.TextArea(pattern);

            if (GUILayout.Button("레이블 찾기"))
            {
                ClearLog();

                GameObject[] prefabs = GetAllPrefabs("Assets/5_OutResource", "Assets/Resources", "Assets/3_Prefab");
                BaseCheck(prefabs, new UILabelChecker(pattern));
            }

            if (GUILayout.Button("Global String 찾기"))
            {
                ClearLog();

                GameObject[] prefabs = GetAllPrefabs("Assets/5_OutResource", "Assets/Resources", "Assets/3_Prefab");
                BaseCheck(prefabs, new UIGlobalStringChecker(pattern));
            }
        }
    }
}
