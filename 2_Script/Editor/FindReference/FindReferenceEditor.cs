using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class FindReferenceEditor : EditorWindow
{
    public enum SearchType
    {
        Project,
        Scene
    }

    public enum Tab
    {
        Object,
        Sprite_New,
        Sprite_Old
    }

    #region private

    private static readonly string[] tabName = new string[] { "Object", "Sprite(New)", "Sprite(Old)" };
    private static SearchType searchType = SearchType.Project;
    private static Tab tab = Tab.Object;
    
    #endregion
    
    [MenuItem("Tools/FindReferenceEditor")]
    public static void ShowWindow()
    {
        GetWindow(typeof(FindReferenceEditor));
    }

    public void OnGUI()
    {
        tab = (Tab)GUILayout.Toolbar((int)tab, tabName);
        
        switch (tab)
        {
            case Tab.Object:
                ObjectGUI();
                break;
            case Tab.Sprite_New:
                NewSpriteGUI();
                break;
            case Tab.Sprite_Old:
                OldSpriteGUI();
                break;
        }
        
        Repaint();
    }

    #region NewSprite
    
    private List<UISprite> referencingSelectionSprite = new List<UISprite>();
    private List<string> referencingSelectionScene = new List<string>();
    private Vector2 newSpriteScrollPosition_1 = Vector2.zero;
    private Vector2 newSpriteScrollPosition_2 = Vector2.zero;
    
    private void NewSpriteGUI()
    {
        
        GUILayout.Label("※Prefab에 연결되어 사용중인 Sprite를 검색합니다※");
        GUILayout.Label("Sprite Setting", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        {
            ComponentSelector.Draw<UIAtlas>("Atlas", atlas, OnSelectAtlas, true, GUILayout.MinWidth(80f));
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        {
            EditorGUI.BeginDisabledGroup(atlas == null);
            UISpriteData sprite = GetSpriteData();
            GUILayout.BeginHorizontal();
            {
                if (sprite != null)
                {
                    NGUIEditorTools.DrawAdvancedSpriteField(atlas, sprite.name, OnSelectSprite, true);
                    if (GUILayout.Button("Search", GUILayout.Width(60f)))
                    {
                        referencingSelectionSprite.Clear();
                        FindReferenceTool.BacktraceSpriteSelection(sprite.name, atlas.name, referencingSelectionSprite);
                        referencingSelectionScene.Clear();
                        FindReferenceTool.BacktraceSpriteSelection(sprite.name, referencingSelectionScene );
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndHorizontal();
        FindReferenceTool.DrawLine();


        GUILayout.Label("Find Result", EditorStyles.boldLabel);
        newSpriteScrollPosition_1 = FindReferenceTool.DrawReferenceList(
            "Select object/s of use the sprite",
            referencingSelectionSprite,
            newSpriteScrollPosition_1);
        newSpriteScrollPosition_2 =
            FindReferenceTool.DrawReferenceList(
                "Select scene/s of use the sprite",
                referencingSelectionScene,
                newSpriteScrollPosition_2);
    }
    
    private UIAtlas atlas
    {
        get { return NGUISettings.Get<UIAtlas>("NGUI Atlas", null); }
        set { NGUISettings.Set("NGUI Atlas", value); }
    }

    private string selectedSprite
    {
        get { return NGUISettings.GetString("NGUI Sprite", null); }
        set { NGUISettings.SetString("NGUI Sprite", value); }
    }
    
    private void OnSelectAtlas(Object obj)
    {
        if (atlas != obj)
        {
            atlas = obj as UIAtlas;
            Repaint();
        }
    }

    private void OnSelectSprite(string spriteName)
    {
        if (selectedSprite != spriteName)
        {
            selectedSprite = spriteName;
            Repaint();
        }
    }
    
    private UISpriteData GetSpriteData()
    {
        string spriteName = selectedSprite;
        UISpriteData sprite;
        if (atlas != null)
        {
            sprite = atlas.GetSprite(selectedSprite);
        }
        else
        {
            sprite = null;
            return null;
        }

        if (!string.IsNullOrEmpty(spriteName))
        {
            sprite = atlas.GetSprite(spriteName);
        }
        
        return sprite ?? (atlas.spriteList[0]);
    }

    #endregion

    #region Object

    private Dictionary<string, List<string>> referenceCacheList = new Dictionary<string, List<string>>();
    private Vector2 objectScrollPosition = Vector2.zero;
    private bool isLoadAllReferences = false;
    private string folderName = "";
    private string searchingFolderName = "";
    private Object selectObject;
    
    private void ObjectGUI()
    {
        GUILayout.Label("Select objects to search", EditorStyles.boldLabel);
        var selections = Selection.activeObject;
        EditorGUI.BeginDisabledGroup(selections == null);
        EditorGUILayout.ObjectField(selections, typeof(Component), allowSceneObjects: true);
        EditorGUILayout.BeginHorizontal();
        if (selections == null)
        {
            GUILayout.Label("Select source object/s from scene Hierarchy panel.");
        }
        EditorGUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();
        
        FindReferenceTool.DrawLine();
        
        GUILayout.Label("Find all references (검색 시간이 오래 걸림)", EditorStyles.boldLabel);
        GUILayout.Label("*검색 후 확인하고 싶은 파일을 클릭해서 사용");
        string buttonText = "Find all references";
        if (isLoadAllReferences && referenceCacheList.Count > 0)
        {
            buttonText = "Find all references again";
        }
        if (GUILayout.Button(buttonText))
        {
            isLoadAllReferences = true;
            selectObject = null;
            searchingFolderName = "";
            referenceCacheList.Clear();
            FindReferenceTool.FindAllReferences(referenceCacheList);
        }
        FindReferenceTool.DrawLine();

        GUILayout.Label("특정 폴더 경로 검색 ex)Assets/Resources/");
        folderName = EditorGUILayout.TextField(folderName);
        EditorGUI.BeginDisabledGroup(folderName == searchingFolderName && selectObject == selections);
        if (GUILayout.Button("Find select object references"))
        {
            isLoadAllReferences = false;
            searchingFolderName = folderName;
            selectObject = selections;
            referenceCacheList.Clear();
            FindReferenceTool.FindAllReferences(referenceCacheList, searchingFolderName);
        }
        EditorGUI.EndDisabledGroup();
        FindReferenceTool.DrawLine();
        

        // 검색 결과 UI;
        objectScrollPosition = FindReferenceTool.DrawReferenceList(
            "Select object/s of use",
            referenceCacheList,
            objectScrollPosition,
            isLoadAllReferences ? Selection.activeObject : selectObject);
    }

    #endregion
    
    #region OldSprite
    
    private bool searchAllAtlas = true;
    private static string atlasNameSearch;
    private static string spriteNameSearch;
    private List<UISprite> referencingSelectionSpriteOld = new List<UISprite>();

    private Vector2 oldSpriteScrollPosition_1 = Vector2.zero;
    
    private void OldSpriteGUI()
    {
        FindReferenceTool.DrawSearchType(ref searchType);
        
        GUILayout.Label("Atlas option", EditorStyles.boldLabel);
        searchAllAtlas = EditorGUILayout.Toggle("Search all atlas", searchAllAtlas);
        
        EditorGUI.BeginDisabledGroup(searchAllAtlas);
        atlasNameSearch = EditorGUILayout.TextField("Atlas name ", atlasNameSearch);
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        GUILayout.Label("Search by sprite name", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        {
            spriteNameSearch = EditorGUILayout.TextField(spriteNameSearch);
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(spriteNameSearch));
            if (GUILayout.Button("Search", GUILayout.Width(60f)))
            {
                referencingSelectionSpriteOld.Clear();
                FindReferenceTool.BacktraceSpriteSelection(spriteNameSearch, searchAllAtlas ? null : atlasNameSearch, referencingSelectionSpriteOld);
            }
            EditorGUI.EndDisabledGroup();
        }
        EditorGUILayout.EndHorizontal();

        FindReferenceTool.DrawLine();

        // 검색 결과 UI
        newSpriteScrollPosition_1 = FindReferenceTool.DrawReferenceList(
            "Select object/s of use the sprite",
            referencingSelectionSpriteOld,
            oldSpriteScrollPosition_1);
    }

    #endregion
}
