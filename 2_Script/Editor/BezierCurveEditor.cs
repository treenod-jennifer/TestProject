using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(BezierCurve))]
public class BezierCurveEditor : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        bool windowActive = BezierCurveEditorWindow.IsOpen(property);

        if (windowActive)
        {
            var color = BezierCurveEditorWindow.GetDebugColor(property);
            GUI.color = color.Value;
        }

        EditorGUI.LabelField(position, property.displayName);

        GUI.color = Color.white;

        Rect toggleRect = new Rect(position.width - 100.0f, position.y, position.width - 100.0f, position.height);

        bool cyrrentActive = EditorGUI.ToggleLeft(toggleRect, new GUIContent("Curve Editor"), windowActive);

        if (windowActive != cyrrentActive)
        {
            SetWindow(cyrrentActive, property);
        }

        EditorGUI.EndProperty();
    }

    //private const int DEFAULT_HEIGHT = 16;
    //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    //{
    //    return DEFAULT_HEIGHT * 1;
    //}

    private void SetWindow(bool trigger, SerializedProperty curve)
    {
        if (trigger)
        {
            BezierCurve bezierCurve = GetObject(curve) as BezierCurve;

            if (bezierCurve != null)
            {
                BezierCurveEditorWindow.EditorOpen(curve, bezierCurve);
            }
        }
        else
        {
            BezierCurveEditorWindow.EditorClose(curve);
        }
    }

    private object GetObject(SerializedProperty property)
    {
        object target = property.serializedObject.targetObject;
        string[] paths = property.propertyPath.Split('.');

        for(int i=0; i<paths.Length; i++)
        {
            if (paths[i] == "Array")
            {
                #region 자료구조속에 데이터가 있는 경우
                var array = target as IEnumerable<object>;
                IEnumerator<object> arrayEnumerator = array.GetEnumerator();

                string indexData = paths[i + 1];
                indexData = indexData.Remove(0, 5);
                indexData = indexData.Remove(indexData.Length - 1, 1);
                int index = int.Parse(indexData);

                for (int j=0; j<=index; j++)
                {
                    arrayEnumerator.MoveNext();
                }

                target = arrayEnumerator.Current;
                i++;
                #endregion
            }
            else
            {
                System.Type type = target.GetType();
                FieldInfo field = type.GetField(paths[i], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                target = field.GetValue(target);
            }
        }

        return target;
    }
}

public class BezierCurveEditorWindow : EditorWindow
{
    private class CurveData
    {
        public BezierCurve Curve { get; }
        public SerializedProperty CurveProperty { get; }

        public Color debugColor;

        private bool[] connectInfo;
        public bool[] ConnectInfo
        {
            get
            {
                if (connectInfo == null || connectInfo.Length != Curve.CurveLength - 1)
                {
                    var tempArray = new bool[Mathf.Max(0, Curve.CurveLength - 1)];

                    System.Array.Copy(connectInfo, tempArray, Mathf.Min(tempArray.Length, connectInfo.Length));

                    connectInfo = tempArray;
                }
                
                return connectInfo;
            }

            set
            {
                connectInfo = value;
            }
        }

        public CurveData(SerializedProperty curveProperty, BezierCurve curve)
        {
            Curve = curve;
            CurveProperty = curveProperty;
            debugColor = DebugColorUtility.GetColor();
            connectInfo = new bool[Mathf.Max(0, Curve.CurveLength - 1)];
        }

        private const int DEBUG_SAMPLING_LEVEL = 100;

        public Vector3[] GetCurve()
        {
            int drawCount = (Curve.PointLength - 1) * DEBUG_SAMPLING_LEVEL;

            if (drawCount <= 0) return new Vector3[0];

            List<Vector3> curves = new List<Vector3>(drawCount);

            for(int i=0; i<drawCount; i++)
            {
                curves.Add(Curve.Evaluate((float)i / (drawCount - 1)));
            }

            return curves.ToArray();
        }
    }

    private class CurveDataList
    {
        private List<CurveData> curveDatas = new List<CurveData>();

        public CurveData this[int i]
        {
            get
            {
                return curveDatas[i];
            }
        }

        public int Count { get { return curveDatas.Count; } }

        public void Add(SerializedProperty property, BezierCurve curve)
        {
            curveDatas.Add(new CurveData(property, curve));
        }

        public void RemoveAt(int index)
        {
            curveDatas.RemoveAt(index);
        }

        public void Clear()
        {
            curveDatas.Clear();
        }
    }

    private static class DebugColorUtility
    {
        private static Color[] colors = new Color[] { Color.red, Color.blue, Color.green, Color.yellow, Color.white, Color.black };
        private static int index = -1;

        public static Color GetColor()
        {
            index++;

            if (index >= colors.Length)
            {
                return new Color(Random.value, Random.value, Random.value);
            }
            else
            {
                return colors[index];
            }
        }

        public static void ResetColor()
        {
            index = -1;
        }
    }

    private static BezierCurveEditorWindow instance;

    private CurveDataList curveList = new CurveDataList();

    private Tool toolState;

    private void OnEnable()
    {
        toolState = Tools.current;

#if UNITY_2018
        SceneView.onSceneGUIDelegate += OnSceneGUI;
#else
        SceneView.duringSceneGui += OnSceneGUI;
#endif
    }

    private void OnDisable()
    {
        Tools.current = toolState;

        instance = null;

        SceneView.RepaintAll();

#if UNITY_2018
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
#else
        SceneView.duringSceneGui -= OnSceneGUI;
#endif
    }

    /// <summary>
    /// OnSceneGUI 내부에서 사용하는 플래그
    /// </summary>
    private bool sceneDirty = false;

    /// <summary>
    /// Window의 상태변경 채크에 사용되는 플래그
    /// </summary>
    private bool windowDirty = false;

    private bool showMode = true;

    private void OnSceneGUI(SceneView sceneView)
    {
        Tools.current = Tool.None;

        sceneDirty = false;

        Handles.BeginGUI();
        if (GUILayout.Button("Control " + (showMode ? "Off" : "On"), GUILayout.Width(85)))
        {
            showMode = !showMode;
        }
        Handles.EndGUI();

        for (int i=0; i<curveList.Count; i++)
        {
            Handles.color = curveList[i].debugColor;
            Handles.DrawAAPolyLine(curveList[i].GetCurve());

            if (showMode)
            {
                PointHandles(curveList[i]);
                ControlHandles(curveList[i]);
            }
        }

        if (sceneDirty)
        {
            windowDirty = true;
            HandleUtility.Repaint();
            RepaintWindow();
        }
    }

    private void PointHandles(CurveData curveData)
    {
        for (int i=0; i<curveData.Curve.PointLength; i++)
        {
            Vector3 pos1 = curveData.Curve.GetPoint(i).Value;
            Vector3 pos2 = Handles.PositionHandle(pos1, Quaternion.identity);

            if(pos1 != pos2)
            {
                curveData.Curve.MovePoint(i, pos2);
                sceneDirty = true;
            }
        }
    }

    private void ControlHandles(CurveData curveData)
    {
        Handles.color = Color.white;

        for (int i=0; i<curveData.Curve.CurveLength; i++)
        {
            #region Start Control Handle
            Vector3 startPos = curveData.Curve.GetPoint(i).Value;
            Vector3 startControlPos = curveData.Curve.GetStartControl(i).Value;

            Handles.DrawLine(startPos, startControlPos);
            Vector3 startControlPos2 = Handles.PositionHandle(startControlPos, Quaternion.identity);

            if (startControlPos != startControlPos2)
            {
                curveData.Curve.MoveStartControl(i, startControlPos2);
                sceneDirty = true;
            }
            #endregion

            #region End Control Handle
            Vector3 endPos = curveData.Curve.GetPoint(i + 1).Value;
            Vector3 endControlPos = curveData.Curve.GetEndControl(i).Value;

            Handles.DrawLine(endPos, endControlPos);

            if (i == curveData.Curve.CurveLength - 1 || !curveData.ConnectInfo[i])
            {
                Vector3 endControlPos2 = Handles.PositionHandle(endControlPos, Quaternion.identity);

                if (endControlPos != endControlPos2)
                {
                    curveData.Curve.MoveEndControl(i, endControlPos2);
                    sceneDirty = true;
                }
            }
            #endregion

            if (i < curveData.Curve.CurveLength - 1)
            {
                Handles.BeginGUI();
                curveData.ConnectInfo[i] = EditorGUI.ToggleLeft(new Rect(HandleUtility.WorldToGUIPoint(endPos), new Vector2(65, 16)), "Connect", curveData.ConnectInfo[i]);
                Handles.EndGUI();

                if (curveData.ConnectInfo[i])
                {
                    Vector3 localEndControlPos = curveData.Curve.GetLocalEndControl(i).Value;
                    Vector3 localStartControlPos = curveData.Curve.GetLocalStartControl(i + 1).Value;

                    if (localEndControlPos != localStartControlPos * -1.0f)
                    {
                        curveData.Curve.MoveLocalEndControl(i, localStartControlPos * -1.0f);
                        sceneDirty = true;
                    }
                }
            }

            //test
            //Handles.DrawBezier(startPos, endPos, startControlPos, endControlPos, new Color(1.0f, 1.0f, 1.0f, 0.1f), null, 10.0f);
        }
    }

    private void OnGUI()
    {
        for (int i=0; i<curveList.Count; i++)
        {
            ShowProperty(curveList[i]);
        }
    }

    private void ShowProperty(CurveData cData)
    {
        try
        {
            cData.CurveProperty.serializedObject.Update();
        }
        catch
        {
            if (instance != null) instance.Close();
            
            return;
        }

        EditorGUI.BeginChangeCheck();


        EditorGUILayout.BeginHorizontal();

        GUILayout.Label($"[{cData.CurveProperty.displayName}]");
        cData.debugColor = EditorGUILayout.ColorField(cData.debugColor);

        EditorGUILayout.EndHorizontal();


        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField($"Points [{cData.Curve.PointLength}]");
        if (GUILayout.Button("-", EditorStyles.miniButtonLeft))
        {
            cData.Curve.RemovePoint();
            
        }
        if (GUILayout.Button("+", EditorStyles.miniButtonRight))
        {
            Vector3 nextPos;

            switch (cData.Curve.PointLength)
            {
                case 0:
                    nextPos = Vector3.zero;
                    break;
                case 1:
                    Vector3 lastPos = cData.Curve.GetPoint(Mathf.Max(0, cData.Curve.PointLength - 1)) ?? Vector3.zero;
                    nextPos = lastPos + Vector3.right;
                    break;
                default:
                    Vector3 lastPos1 = cData.Curve.GetPoint(Mathf.Max(0, cData.Curve.PointLength - 1)) ?? Vector3.zero;
                    Vector3 lastPos2 = cData.Curve.GetPoint(Mathf.Max(0, cData.Curve.PointLength - 2)) ?? Vector3.zero;
                    nextPos = lastPos1 + (lastPos1 - lastPos2);
                    break;
            }

            Vector3 endControl = cData.Curve.GetLocalEndControl(Mathf.Max(0, cData.Curve.CurveLength - 1)) ?? Vector3.down;
            endControl *= -1.0f;

            cData.Curve.AddPoint(nextPos, endControl, endControl);
        }

        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < cData.Curve.PointLength; i++)
        {
            Vector3? pos = cData.Curve.GetPoint(i);
            if (pos == null) break;

            cData.Curve.MovePoint(i, EditorGUILayout.Vector3Field($"Point [{i}]", pos.Value));
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField($"Control [{cData.Curve.CurveLength}]");

        for (int i=0; i<cData.Curve.CurveLength; i++)
        {
            Vector3? startPos = cData.Curve.GetLocalStartControl(i);
            if (startPos == null) break;

            cData.Curve.MoveLocalStartControl(i, EditorGUILayout.Vector3Field($"Start Control [{i}-{i + 1}]", startPos.Value));

            Vector3? endPos = cData.Curve.GetLocalEndControl(i);
            if (endPos == null) break;

            cData.Curve.MoveLocalEndControl(i, EditorGUILayout.Vector3Field($"End Control [{i}-{i + 1}]", endPos.Value));
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        if (EditorGUI.EndChangeCheck() || windowDirty)
        {
            windowDirty = false;

            EditorUtility.SetDirty(cData.CurveProperty.serializedObject.targetObject);

            var target = cData.CurveProperty.serializedObject.targetObject as MonoBehaviour;

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(target.gameObject.scene);

            cData.CurveProperty.serializedObject.ApplyModifiedProperties();
        }
    }

    public static void EditorOpen(SerializedProperty curveProperty, BezierCurve curve)
    {
        if(instance == null)
        {
            instance = CreateInstance<BezierCurveEditorWindow>();
            instance.ShowUtility();

            DebugColorUtility.ResetColor();
        }

        if (!IsOpen(curveProperty))
        {
            instance.curveList.Add(curveProperty, curve);
        }

        RepaintWindow();
        SceneView.RepaintAll();
    }

    public static void EditorClose(SerializedProperty curve)
    {
        int index = GetIndex(curve);

        if (index == -1) return;

        instance.curveList.RemoveAt(index);

        if(instance.curveList.Count == 0)
        {
            instance.Close();
        }
        else
        {
            RepaintWindow();
            SceneView.RepaintAll();
        }
    }

    public static bool IsOpen(SerializedProperty curve)
    {
        if (instance == null) return false;

        return GetIndex(curve) != -1;
    }

    public static Color? GetDebugColor(SerializedProperty curve)
    {
        int index = GetIndex(curve);

        if (index == -1)
        {
            return null;
        }

        return instance.curveList[index].debugColor;
    }

    public static void RepaintWindow()
    {
        instance?.Repaint();
    }

    private static int GetIndex(SerializedProperty curve)
    {
        int index = -1;

        for (int i = 0; i < instance.curveList.Count; i++)
        {
            if (SerializedProperty.EqualContents(instance.curveList[i].CurveProperty, curve))
            {
                index = i;
                break;
            }
        }

        return index;
    }
}
