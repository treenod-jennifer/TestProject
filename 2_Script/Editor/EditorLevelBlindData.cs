using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public class EditorLevelBlindData : EditorWindow
{
    static EditorLevelBlindData window;

    static Vector2 scrollValue = Vector2.zero;

    private static int _nDataSize = 250;
    private static int _nDataWidthSize = 50;
    static GUIStyle guiStyle;

    private static Texture2D[] _tex2ds = new Texture2D[(int)eAreaDataTextureType.Count];
    private Texture2D _tex2dCur;
    private int _nTextureID = 0;
    public int _nCurrentArea = 0;

    string _strAssetPath = "Bundle/Texture/Blind/";

    private bool _bEdit = false;
    //    private RectOffset _editRect;
    public MeshDataInfo _meshDataInfo = new MeshDataInfo();

    private static ManagerLevelBlindStatic _managerLevelBlindStatic;
    private static TileMap _tileMap;



    private void Awake()
    {
        _nTextureID = (int)eAreaDataTextureType.None;
    }

    [MenuItem("EditorUtility/LevelBlindDataEditor")]
    static void OpenLevelBlindDataEditor()
    {
        window = (EditorLevelBlindData)EditorWindow.GetWindow(typeof(EditorLevelBlindData));
        string strFileName = "";
        byte[] data;
        for (int i = 0; i < (int)eAreaDataTextureType.Count; i++)
        {
            //            strFileName = _strAssetPath + "blind_01_" + (i+1).ToString("00");
            strFileName = Application.dataPath + "/7_Tool/Texture/Blind/" + "blind_01_" + (i + 1).ToString("00") + ".png";
            data = File.ReadAllBytes(strFileName);
            _tex2ds[i] = new Texture2D(32, 32);
            _tex2ds[i].LoadImage(data);
            //            _tex2ds[i] = Resources.Load(strFileName) as Texture2D;
        }
        //        strFileName = _strAssetPath + "blind_01_08";
        /*
         strFileName = Application.dataPath + "/7_Tool/Texture/Blind/" + "blind_01_" + eAreaDataTextureType.None.ToString("00");
         data = File.ReadAllBytes(strFileName);
         _tex2dCur = new Texture2D(32, 32);
         _tex2dCur.LoadImage(data);
        */
        Assembly _assembly = Assembly.Load("Assembly-CSharp");

        foreach (Type type in _assembly.GetTypes())
        {
            if (type.IsClass)
            {
                if (type.BaseType.FullName.Contains("MonoBehaviour"))
                {
                    if (type.Name.Contains("ManagerLevelBlindStatic"))
                    {
                        UnityEngine.Object[] allObjects = Resources.FindObjectsOfTypeAll(type.UnderlyingSystemType);
                        foreach (UnityEngine.Object _obj in allObjects)
                        {
//                            Debug.Log(_obj.ToString());
                            if ("ManagerLevelBlindStatic" == _obj.name)
                            {
                                GameObject g = _obj as GameObject;
                                _managerLevelBlindStatic = _obj as ManagerLevelBlindStatic;
                                if (_managerLevelBlindStatic != null)
                                    break;
                            }
                        }
                    }
                }
            }
        }

        /*        
                guiStyle = new GUIStyle();
                guiStyle.margin.left = 10;
                guiStyle.padding.bottom = 5;
                guiStyle.normal.textColor = Color.white;
                */
    }


    private void OnSelectionChange()
    {
        if (Application.isPlaying == true || _managerLevelBlindStatic == null || _bEdit == false)
            return;
        GameObject go = Selection.activeGameObject;
        if (go == null)
            return;
        TileMap tileMap = go.GetComponent<TileMap>();
        if (tileMap != null && tileMap.name == "TileMap_0" && _tileMap != null && _tileMap != tileMap)
        {
            _tileMap = tileMap;
            _meshDataInfo._fPosX = _tileMap._transform.localPosition.x;
            _meshDataInfo._fPosZ = _tileMap._transform.localPosition.z;
            _meshDataInfo._nColumns = _tileMap._nColumns;
            _meshDataInfo._nRows = _tileMap._nRows;
            _meshDataInfo._fTileWidthSize = _tileMap._fTileWidthSize;
            _meshDataInfo._fTileHeightSize = _tileMap._fTileHeightSize;
            _meshDataInfo._nPosIndex_X = _tileMap._nPosIndex_X;
            _meshDataInfo._nPosIndex_Z = _tileMap._nPosIndex_Z;
            _nCurrentArea = _tileMap.transform.parent.GetComponent<BlindArea>()._nIndex;
        }
    }

    bool onceInit = false;
    void OnGUI()
    {
        if (Application.isPlaying == true)
            return;

        if (_managerLevelBlindStatic == null)
            onceInit = false;

        if (onceInit == false)
        {
            onceInit = true;
            OpenLevelBlindDataEditor();
        }
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.BeginHorizontal();
        _bEdit = EditorGUILayout.Toggle("Edit Mode", _bEdit, GUILayout.Width(180));
        _nCurrentArea = EditorGUILayout.IntField("Current Area Index", _nCurrentArea, GUILayout.Width(180));
        EditorGUILayout.EndHorizontal();
        _managerLevelBlindStatic._bEditMode = _bEdit;
        if (_bEdit == true)
            _managerLevelBlindStatic.MakeColliderMesh(_nCurrentArea);
        else
            _managerLevelBlindStatic.DeleteColliderMesh(_nCurrentArea);
        if (_tileMap != null)
        {
            if (_bEdit == true)
            {
                if (_tileMap.gameObject.activeInHierarchy == false)
                    _tileMap.gameObject.SetActive(true);
//                Selection.activeObject = _tileMap;
            }
            else
            {
                if (_tileMap.gameObject.activeInHierarchy == true)
                    _tileMap.gameObject.SetActive(false);
            }
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Pos Info");
        if (GUILayout.Button("Make", GUILayout.Width(60)))
        {
            if (_managerLevelBlindStatic != null && _bEdit == true )
            {
//                _tileMap.transform.position = new Vector3(_meshDataInfo._fPosX, 3.17f, _meshDataInfo._fPosZ);
                _managerLevelBlindStatic.Init(_meshDataInfo, _nCurrentArea, _nTextureID);
                _tileMap = _managerLevelBlindStatic.GetTileMap(_nCurrentArea, 0);
                Selection.activeObject = _tileMap;
            }
            else
                onceInit = false;
        }
        if (GUILayout.Button("Save", GUILayout.Width(60)))
        {
            if (_managerLevelBlindStatic == null)
            {
                EditorUtility.DisplayDialog("Notice", "인스턴스 얻어오기 실패, 다시시도해주세요", "Ok");
                onceInit = false;
            }
            else if (_bEdit == true)
            {
                string path = EditorUtility.SaveFilePanel("Save Tile Data", "", "", "bytes");
                if (path != null && path != "")
                {
                    if (_managerLevelBlindStatic.SaveTileChunkDataInfo(path, _meshDataInfo, _nCurrentArea))
                        EditorUtility.DisplayDialog("TileMap", "저장하기 완료", "Ok");
                    else
                        EditorUtility.DisplayDialog("TileMap", "저장하기 실패", "Ok");
                }
            }

            /*
            if (strFileName != null && strFileName != "")
            {
                if (_managerLevelBlindStatic != null)
                {
                    if (_managerLevelBlindStatic.SaveTileChunkDataInfo(strFileName, _meshDataInfo))
                        EditorUtility.DisplayDialog("TileMap", "저장하기 완료", "Ok");
                    else
                        EditorUtility.DisplayDialog("TileMap", "저장하기 실패", "Ok");
                }
                else
                    onceInit = false;
            }
            */
        }
        if (GUILayout.Button("Load", GUILayout.Width(60)))
        {
            if (_managerLevelBlindStatic == null)
            {
                EditorUtility.DisplayDialog("Notice", "인스턴스 얻어오기 실패, 다시시도해주세요", "Ok");
                onceInit = false;
            }
            else if (_bEdit == true)
            {
                string path = EditorUtility.OpenFilePanel("Load Tile Data", "", "bytes");
                if (path != null && path != "")
                {
                    if (_managerLevelBlindStatic.LoadTileChunkDataInfo(path, _nCurrentArea))
                    {
                        _tileMap = _managerLevelBlindStatic.GetTileMap(_nCurrentArea, 0);
                        _meshDataInfo._fPosX = _tileMap._transform.position.x;
                        _meshDataInfo._fPosZ = _tileMap._transform.position.z;
                        _meshDataInfo._nColumns = _tileMap._nColumns;
                        _meshDataInfo._nRows = _tileMap._nRows;
                        _meshDataInfo._fTileWidthSize = _tileMap._fTileWidthSize;
                        _meshDataInfo._fTileHeightSize = _tileMap._fTileHeightSize;
                        _meshDataInfo._nPosIndex_X = _tileMap._nPosIndex_X;
                        _meshDataInfo._nPosIndex_Z = _tileMap._nPosIndex_Z;
                        _nCurrentArea = _tileMap.transform.parent.GetComponent<BlindArea>()._nIndex;
                        EditorUtility.DisplayDialog("TileMap", "불러오기 완료", "Ok");
                        Selection.activeObject = _tileMap;
                    }
                    else
                        EditorUtility.DisplayDialog("TileMap", "불러오기 실패", "Ok");
                }
            }
            /*
           if (strFileName != null && strFileName != "")
           {
               if (_managerLevelBlindStatic != null)
               {
                   if (_managerLevelBlindStatic.LoadTileChunkDataInfo(strFileName))
                       EditorUtility.DisplayDialog("TileMap", "불러오기 완료", "Ok");
                   else
                       EditorUtility.DisplayDialog("TileMap", "불러오기 실패", "Ok");
               }
               else
                   onceInit = false;
           }
            * */
        }
        if (GUILayout.Button("Clear", GUILayout.Width(60)))
        {
            _managerLevelBlindStatic.AllClearArea();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel += 1;
        EditorGUILayout.BeginHorizontal();
        _meshDataInfo._nPosIndex_X = EditorGUILayout.IntField("Pos Index X", _meshDataInfo._nPosIndex_X);
        _meshDataInfo._nPosIndex_Z = EditorGUILayout.IntField("Pos Index Z", _meshDataInfo._nPosIndex_Z);
        _meshDataInfo._fPosX = _meshDataInfo._nPosIndex_X*_meshDataInfo._fTileWidthSize;
        _meshDataInfo._fPosZ = _meshDataInfo._nPosIndex_Z * _meshDataInfo._fTileHeightSize;
        EditorGUILayout.EndHorizontal();
        /*
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.FloatField("Pos X", _meshDataInfo._fPosX);
        EditorGUILayout.FloatField("Pos Z", _meshDataInfo._fPosZ);
        EditorGUILayout.EndHorizontal();
         * */
        EditorGUILayout.BeginHorizontal();
        _meshDataInfo._nColumns = EditorGUILayout.IntField("Columns", _meshDataInfo._nColumns);
        _meshDataInfo._nRows = EditorGUILayout.IntField("Rows", _meshDataInfo._nRows);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        _meshDataInfo._fTileWidthSize = EditorGUILayout.FloatField("TileWidthSize", _meshDataInfo._fTileWidthSize);
        _meshDataInfo._fTileHeightSize = EditorGUILayout.FloatField("TileHeightSize", _meshDataInfo._fTileHeightSize);
        EditorGUILayout.EndHorizontal();
        EditorGUI.indentLevel -= 1;

        /*
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Clear", GUILayout.Width(60)))
        {
            
        }
        EditorGUILayout.EndHorizontal();
         * * */

        
        EditorGUILayout.BeginHorizontal();
//        if (_managerLevelBlindStatic != null)
        {
            EditorGUILayout.IntField("Vertices Count", _managerLevelBlindStatic.GetVerticesCount(_nCurrentArea));
            EditorGUILayout.IntField("Triangles Count", _managerLevelBlindStatic.GetTrianglesCount(_nCurrentArea));
        }
        EditorGUILayout.EndHorizontal();

        int k = 0;
        for (int i = 0; i < 3; i++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < (int)8; j++)
            {
                if (_tex2ds[k] != null)
                {
                    if (GUILayout.Button(_tex2ds[k], GUILayout.Width(30)))
                    {
                        _tex2dCur = _tex2ds[k];
                        _nTextureID = k;
                        _managerLevelBlindStatic.ChangeTextureID_All(_nTextureID);
                    }
                    k++;
                }
            }
            if (i == 2)
            {
                EditorGUILayout.LabelField("", GUILayout.Width(30));
                if (GUILayout.Button(_tex2dCur, GUILayout.Width(30)))
                {

                }
            }
            EditorGUILayout.EndHorizontal();
        }

        /*
        EditorGUILayout.BeginHorizontal();
        int k = 0;
        for (int i = 0; i < (int)eAreaDataTextureType.Count; i++)
        {
            if (_tex2ds[i] != null)
            {
                if (GUILayout.Button(_tex2ds[i], GUILayout.Width(30)))
                {
                    _tex2dCur = _tex2ds[i];
                    _nTextureID = i;
                    _levelBlindStaticManager.ChangeTextureID(_nTextureID);
                }
            }
        }

        EditorGUILayout.LabelField("", GUILayout.Width(30));
        if (GUILayout.Button(_tex2dCur, GUILayout.Width(30)))
        {

        }

        EditorGUILayout.EndHorizontal();
        */
        if (onceInit == false)
        {
            onceInit = true;
            OpenLevelBlindDataEditor();
        }
    }
}
#endif