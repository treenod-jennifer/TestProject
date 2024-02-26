using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Tilemaps;

public class AreaTilemapEditorWindow : EditorWindow
{
    private AreaTilemapSettings settings;

    private AreaTilemapSettings Settings
    {
        get
        {
            if(settings == null)
            {
                settings = AssetDatabase.LoadAssetAtPath<AreaTilemapSettings>("Assets/4_InResource/Tile/Settings/Area Tilemap Settings.asset");
            }

            return settings;
        }
    }

    private int toggle = 0;

    private int Toggle
    {
        get
        {
            return toggle;
        }
        set
        {
            if(toggle != value)
            {
                ModeChange(toggle);
                toggle = value;
            }
        }
    }

    private Tilemap loadTilemap;
    private Tilemap editTilemap;

    private Tilemap testTilemap;
    private Tilemap TestTilemap
    {
        get
        {
            return testTilemap;
        }
        set
        {
            if (value == null && makeTestTilemap != null)
            {
                Manager.DeleteTilemap_EditorMode(makeTestTilemap.name);
                makeTestTilemap = null;
            }
            else if(testTilemap != value)
            {
                if(makeTestTilemap != null)
                {
                    Manager.DeleteTilemap_EditorMode(makeTestTilemap.name);
                }

                makeTestTilemap = Manager.MakeTilemap(value.name, value);
            }

            testTilemap = value;
        }
    }
    private Tilemap makeTestTilemap;
    private List<TextAsset> testHoles = new List<TextAsset>();

    private ManagerTilemap manager;
    private ManagerTilemap Manager
    {
        get
        {
            if(manager == null)
            {
                manager = Instantiate(Settings.Manager);
            }

            return manager;
        }
    }

    [MenuItem("Tools/AreaTilemapEditor", priority = 1800)]
    static void Open()
    {
        GetWindow<AreaTilemapEditorWindow>();
    }

    private void OnDestroy()
    {
        if(manager != null)
        {
            DestroyImmediate(manager.gameObject);
        }
    }

    void OnGUI()
    {
        if (GUILayout.Button("카메라 자동 세팅"))
        {
            var view = SceneView.lastActiveSceneView;
            view.AlignViewToObject(Camera.main.transform);
            view.orthographic = true;
        }

        Toggle = GUILayout.Toolbar(Toggle, new string[] { "Edit", "View" });

        if (Toggle == 0)
        {
            EditMode();
        }
        else if(Toggle == 1)
        {
            ViewMode();
        }
    }

    private void EditMode()
    {
        GUI.enabled = true;

        GUILayout.Space(10.0f);

        EditorGUILayout.LabelField("Tilemap Editor"); 

        bool isEditMode = editTilemap != null;
        string message = isEditMode ? $"{editTilemap.name}을 편집중 입니다." : "현재 편집중인 Tile이 없습니다.";
        EditorGUILayout.HelpBox(message, isEditMode ? MessageType.Info : MessageType.Warning);

        GUI.enabled = !isEditMode;
        if (GUILayout.Button("Blind Tile 새로 만들기"))
        {
            editTilemap = MakeTile(Settings.BlindTile);
        }

        if (GUILayout.Button("Cloud Tile 새로 만들기"))
        {
            editTilemap = MakeTile(Settings.CloudTile);
        }

        GUILayout.Space(10.0f);

        GUI.enabled = true;
        loadTilemap = EditorGUILayout.ObjectField("Load Data", loadTilemap, typeof(Tilemap), false) as Tilemap;

        GUI.enabled = loadTilemap != null;
        if (GUILayout.Button("Tile 불러오기"))
        {
            if (editTilemap != null)
            {
                DestroyImmediate(editTilemap.gameObject);
            }

            editTilemap = MakeTile_Edit(loadTilemap);
            Selection.activeObject = editTilemap;
        }

        GUILayout.Space(10.0f);

        GUI.enabled = isEditMode;

        if (GUILayout.Button("닫기"))
        {
            TilemapEditEnd();
        }
    }

    private void TilemapEditEnd()
    {
        if(editTilemap != null)
        {
            DestroyImmediate(editTilemap.gameObject);
            editTilemap = null;
        }
    }

    private void ViewMode()
    {
        GUI.enabled = true;

        GUILayout.Space(10.0f);

        TestTilemap = EditorGUILayout.ObjectField("Tilemap", TestTilemap, typeof(Tilemap), false) as Tilemap;

        GUI.enabled = testTilemap != null;
        if (GUILayout.Button("Tile 리셋"))
        {
            TileReset();
        }

        if (GUILayout.Button("Hole 세팅"))
        {
            TileReset();

            for (int i = 0; i < testHoles.Count; i++)
            {
                if (testHoles[i] != null)
                {
                    Manager.MakeHole(makeTestTilemap.name, testHoles[i].text);
                }
            }
        }


        GUI.enabled = true;

        int holeCount = EditorGUILayout.IntField("Hole 갯수", testHoles.Count);

        holeCount = Mathf.Clamp(holeCount, 0, 30);

        int subCount = holeCount - testHoles.Count;

        if (subCount > 0)
        {
            testHoles.AddRange(new TextAsset[subCount]);
        }
        else if (subCount < 0)
        {
            testHoles.RemoveRange(testHoles.Count + subCount, subCount * -1);
        }

        for (int i = 0; i < testHoles.Count; i++)
        {
            testHoles[i] = EditorGUILayout.ObjectField($"hole[{i + 1:D2}]", testHoles[i], typeof(TextAsset), false) as TextAsset;
        }

        void TileReset()
        {
            Manager.DeleteTilemap_EditorMode(testTilemap.name);
            makeTestTilemap = Manager.MakeTilemap(testTilemap.name, testTilemap);
        }
    }

    private void ModeChange(int toggle)
    {
        switch (toggle)
        {
            case 0:
                TilemapEditEnd();
                break;
            case 1:
                TestTilemap = null;
                break;
            default:
                break;
        }
    }

    private Tilemap MakeTile(Tilemap tilemap)
    {
        var makeTilemap = Instantiate(tilemap, Manager.Root);

        makeTilemap.name = tilemap.name;

        return makeTilemap;
    }

    private Tilemap MakeTile_Edit(Tilemap tilemap)
    {
        var makeTilemap = PrefabUtility.InstantiatePrefab(tilemap, Manager.Root);

        makeTilemap.name = tilemap.name;

        return makeTilemap as Tilemap;
    }
}
