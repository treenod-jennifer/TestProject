using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EditorAreaAnimalData : EditorWindow
{
    private int areaIndex = -1;

    //에디터
    private EditorAreaAnimal_ViewRoamingData roamingAreaEditor;
    private EditorAreaAnimal_ViewCharacterData characterEditor;
    private EditorAreaAnimal_ViewAreaPrefabData areaPrefabEditor;

    #region 에디터 UI 설정
    //에디터 GUI 스타일
    private GUIStyle editorStyle;

    //상단 메뉴바
    private float menuBarHeight = 20f;
    private Rect menuBar;

    private Vector2 posScroll;

    //좌측 뷰
    private Vector2 posLeft;

    //우측 뷰
    private Vector2 posRight;

    //스플리터_공통
    private float splitterWidth = 5;

    //스플리터_좌측
    private Rect splitterRect_left;
    private float splitterPos_left;
    private Vector2 dragStartPos_left;
    private bool dragging_left;

    //스플리터_우측
    private Rect splitterRect_right;
    private float splitterPos_right;
    private Vector2 dragStartPos_right;
    private bool dragging_right;
    #endregion

    [MenuItem("EditorUtility/AreaAnimalData Editor")]
    private static void OpenWindow()
    {
        EditorAreaAnimalData window = GetWindow<EditorAreaAnimalData>();
        window.titleContent = new GUIContent("로비 동물 데이터 편집기");
    }

    private void OnEnable()
    {
        hideFlags = HideFlags.HideAndDontSave;
        if (roamingAreaEditor == null)
        {
            roamingAreaEditor = new EditorAreaAnimal_ViewRoamingData();
            roamingAreaEditor.InitView();
        }

        if (characterEditor == null)
        {
            characterEditor = new EditorAreaAnimal_ViewCharacterData();
            characterEditor.InitView();
        }

        if (areaPrefabEditor == null)
        {
            areaPrefabEditor = new EditorAreaAnimal_ViewAreaPrefabData();
            areaPrefabEditor.InitView();
        }

        editorStyle = new GUIStyle();
        editorStyle.normal.background = EditorGUIUtility.whiteTexture;
        editorStyle.border = new RectOffset(12, 12, 12, 12);
    }

    private void OnGUI()
    {
        //메뉴바 그리기
        DrawMenuBar();

        Vector2 pos = new Vector2(10f, 30f);

        //편집하고자 하는 에리어 인덱스
        areaIndex = EditorGUI.IntField(new Rect(pos.x, pos.y, 250, 22), "에리어 번호 :", areaIndex);
        GUILayout.Space(70f);

        //뷰 그리기
        posScroll = EditorGUILayout.BeginScrollView(posScroll);
        {
            float posX = 10f;

            #region 좌측 뷰
            roamingAreaEditor.Draw(new Vector2(10f, 0f));
            int viewHeight = roamingAreaEditor.GetViewSize().y + 50;
            #endregion

            posX += roamingAreaEditor.GetViewSize().x + 20;

            #region 좌측 스플리터
            GUI.Box(new Rect(posX, 0, splitterWidth, this.maxSize.y - 30), "");
            #endregion

            posX += splitterWidth + 20;

            #region 중앙 뷰
            characterEditor.Draw(new Vector2(posX, 0f));
            if (viewHeight < characterEditor.GetViewSize().y + 50)
                viewHeight = characterEditor.GetViewSize().y + 50;
            #endregion

            posX += characterEditor.GetViewSize().x + 20;

            #region 우측 스플리터
            GUI.Box(new Rect(posX, 0, splitterWidth, this.maxSize.y - 30), "");
            #endregion

            posX += splitterWidth + 20;

            #region 우측 뷰
            areaPrefabEditor.Draw(new Vector2(posX, 0f));
            if (viewHeight < areaPrefabEditor.GetViewSize().y + 50)
                viewHeight = areaPrefabEditor.GetViewSize().y + 50;
            #endregion

            GUILayout.Space(viewHeight);
        }
        EditorGUILayout.EndScrollView();

        #region 이벤트 처리
        #endregion
    }

    private void DrawMenuBar()
    {
        menuBar = new Rect(0, 0, position.width, menuBarHeight);

        GUILayout.BeginArea(menuBar, EditorStyles.toolbar);
        GUILayout.BeginHorizontal();

        //로드 버튼.
        if (GUILayout.Button(new GUIContent("Load"), EditorStyles.toolbarButton, GUILayout.Width(50)))
        {
            OnClickButton_Load();
        }

        //세이브 버튼.
        if (GUI.Button(new Rect(position.width - 55, 0, 50, menuBarHeight), "Save", EditorStyles.toolbarButton))
        {
            OnClickButton_Save();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    private AssetGlobal GetAssetGlobal()
    {
        string path = "Assets/5_OutResource/global_1/g_1.prefab";
        GameObject BundleObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        return BundleObject.GetComponent<AssetGlobal>();
    }

    #region 저장
    public void OnClickButton_Save()
    {
        if (areaIndex == -1)
            return;

        AssetGlobal assetGlobal = GetAssetGlobal();
        if (assetGlobal == null)
            return;

        roamingAreaEditor.SaveData(areaIndex, assetGlobal.listRoamingAreaData);
        characterEditor.SaveData(areaIndex, assetGlobal.listAreaAnimalData);
        areaPrefabEditor.SaveData(areaIndex);

        PrefabUtility.SavePrefabAsset(assetGlobal.gameObject);
        Debug.Log(" === 저장 완료 === ");
    }
    #endregion

    #region 로드
    public void OnClickButton_Load()
    {
        if (areaIndex == -1)
            return;

        AssetGlobal assetGlobal = GetAssetGlobal();
        if (assetGlobal == null)
            return;

        roamingAreaEditor.LoadData(areaIndex, assetGlobal.listRoamingAreaData);
        characterEditor.LoadData(areaIndex, assetGlobal.listAreaAnimalData);
        areaPrefabEditor.LoadData(areaIndex);
    }
    #endregion
}
