using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;




// 잡다
public static class PokoUtil
{
    static public float _PI90 = Mathf.PI / 2f;

    static public Transform FindChild(Transform t, string startsWith)
    {
        if (t.name.StartsWith(startsWith)) return t;

        for (int i = 0, imax = t.childCount; i < imax; ++i)
        {
            Transform ch = FindChild(t.GetChild(i), startsWith);
            if (ch != null) return ch;
        }
        return null;
    }

    public static void RemoveString_AssetsResources(ref string strPath, bool bRemoveExtention = true)
    {
        int i_count = strPath.LastIndexOf("Assets/Resources/");
        if (i_count >= 0)
        {
            strPath = strPath.Remove(0, i_count + 17);
        }
        if (bRemoveExtention)
        {
            RemoveString_Extention(ref strPath);
        }
    }

    public static void RemoveString_Assets(ref string strPath, bool bRemoveExtention = true)
    {
        int i_count = strPath.LastIndexOf("Assets/");
        if (i_count >= 0)
        {
            strPath = strPath.Remove(0, i_count + 7);
        }
        if (bRemoveExtention)
        {
            RemoveString_Extention(ref strPath);
        }
    }

    public static void RemoveString_Extention(ref string strPath)
    {
        int i_count = strPath.LastIndexOf(".");
        if (i_count > -1)
        {
            strPath = strPath.Remove(i_count, strPath.Length - i_count);
        }
    }

    public static string[] GetFolderList(string strDir)
    {
        DirectoryInfo dir = new DirectoryInfo(strDir);

        // CheckDir
        if (dir.Exists == false)
        {
            Debug.LogError("Directory not found - " + strDir);
            return null;
        }

        DirectoryInfo[] info = dir.GetDirectories();
        List<string> strList = new List<string>();

        foreach (DirectoryInfo dirInfo in info)
        {
            strList.Add(dirInfo.Name);
        }
        string[] folderStrings = new string[strList.Count];
        folderStrings = strList.ToArray();
        return folderStrings;
    }

    public static string[] GetFileList(string strDir, string searchPattern)
    {
        DirectoryInfo dir = new DirectoryInfo(strDir);

        // CheckDir
        if (dir.Exists == false)
        {
            Debug.LogError("Directory not found - " + strDir);
            return null;
        }

        FileInfo[] info = dir.GetFiles(searchPattern);
        string[] fileStrings = new string[info.Length];
        int nCount = 0;
        foreach (FileInfo fileInfo in info)
        {
            fileStrings[nCount++] = Path.GetFileNameWithoutExtension(fileInfo.Name);
        }
        return fileStrings;
    }

    static public void SetChildLayer(this Transform t, int layer)
    {
        for (int i = 0; i < t.childCount; ++i)
        {
            Transform child = t.GetChild(i);
            child.gameObject.layer = layer;
            SetChildLayer(child, layer);
        }
    }

    public static string pathForDocumentsFile(string filename)
    {
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            string path = Application.dataPath.Substring(0, Application.dataPath.Length - 5);
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(Path.Combine(path, "Documents"), filename);
        }

        else if (Application.platform == RuntimePlatform.Android)
        {
            string path = Application.persistentDataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        }
        else
        {
            string path = Application.dataPath;
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        }
    }

#if UNITY_EDITOR
    static public void drawString(string text, Vector3 worldPos, float oX = 0, float oY = 0, Color? colour = null)
    {
        


        if (UnityEditor.SceneView.currentDrawingSceneView == null)
            return;

        UnityEditor.Handles.BeginGUI();

        var restoreColor = GUI.color;

        if (colour.HasValue) GUI.color = colour.Value;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

        if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
        {
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
            return;
        }

        GUIStyle style = new GUIStyle();
        style.normal.textColor = colour.Value;

        UnityEditor.Handles.Label(TransformByPixel(worldPos, oX, oY), text, style);

        GUI.color = restoreColor;
        UnityEditor.Handles.EndGUI();


    }
    static Vector3 TransformByPixel(Vector3 position, float x, float y)
    {
        return TransformByPixel(position, new Vector3(x, y));
    }

    static Vector3 TransformByPixel(Vector3 position, Vector3 translateBy)
    {
        Camera cam = UnityEditor.SceneView.currentDrawingSceneView.camera;
        if (cam)
            return cam.ScreenToWorldPoint(cam.WorldToScreenPoint(position) + translateBy);
        else
            return position;
    }
#else
    static public void drawString(string text, Vector3 worldPos, float oX = 0, float oY = 0, Color? colour = null)
    {

    }
#endif
}

//
public static class PokoMath
{
    static public Vector3 ChangeTouchPosNGUI(Vector3 in_touchPos)
    {
        return new Vector3((in_touchPos.x - Screen.width / 2f), in_touchPos.y - Screen.height / 2f, 0) * 800f / Screen.width;
    }
    static public Vector3 ChangeTouchPosNGUIInvert(Vector3 in_touchPos)
    {
        return new Vector3((in_touchPos.x * Screen.width / 800f + Screen.width / 2f), in_touchPos.y * Screen.width / 800f + Screen.height / 2f, 0);
    }

    /// <summary>
    /// 월드 포인트에서 스크린 상의 포인트 위치를 얻어오기 위함
    /// </summary>
    /// <param name="world"> 얻어올 월드 벡터 </param>
    /// <returns></returns>
    static public Vector2 WorldToGUIPoint ( Vector2 world , Camera targetCamera )
    {
        Vector2 screenPoint = targetCamera.WorldToScreenPoint( world );
        screenPoint.y = (float)Screen.height - screenPoint.y;
        return screenPoint;
    }

    /// <summary>
    /// 스크린상의 2D 텍스처 Rect범위를 얻어오기 위함
    /// </summary>
    /// <param name="objectCollider"></param>
    /// <returns></returns>
    static public Rect GUIRectWithObject2D ( BoxCollider2D objectCollider, Camera targetCamera )
    {
        Vector3 cen = objectCollider.bounds.center;
        Vector3 ext = objectCollider.bounds.extents;

        Vector2[] extentPoints = new Vector2[4]
        {
            WorldToGUIPoint ( new Vector3 (cen.x-ext.x, cen.y-ext.y, cen.z-ext.z), targetCamera),
            WorldToGUIPoint ( new Vector3 (cen.x+ext.x, cen.y-ext.y, cen.z-ext.z), targetCamera),
            WorldToGUIPoint ( new Vector3 (cen.x-ext.x, cen.y+ext.y, cen.z-ext.z), targetCamera),
            WorldToGUIPoint ( new Vector3 (cen.x+ext.x, cen.y+ext.y, cen.z-ext.z), targetCamera)
        };

        Vector2 min = extentPoints [0];
        Vector2 max = extentPoints [0];

        for( int i = 0; i < 4; i++ )
        {
            min = Vector2.Min ( min, extentPoints[i] );
            max = Vector2.Max ( max, extentPoints[i] );
        }

        return new Rect ( min.x, min.y, max.x - min.x, max.y - min.y );
    }
}
//델리게이트 관련
public static class Method
{
    public delegate void FunctionVoid();
    public delegate void FunctionVoidCallback(Method.FunctionVoid callback);
    public delegate void FunctionBool(bool arg);
    public delegate void FunctionInt(int arg);
    public delegate void FunctionFloat(float arg);
    public delegate void FunctionString(string arg);
    public delegate void FunctionObject(object arg);
    public delegate void FunctionTemplate<T>(T arg);
    public delegate void FunctionVector3(Vector3 arg);
    public delegate void FunctionGameObject(GameObject arg);
    public delegate IEnumerator FunctionCoroutine();
}