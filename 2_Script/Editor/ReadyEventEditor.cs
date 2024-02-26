using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ReadyEvent))]
public class ReadyEventEditor : Editor
{
    private SerializedProperty property_Live2D_Character;
    private SerializedProperty property_Live2D_Size;
    private SerializedProperty property_Live2D_Offset;

    private SerializedProperty property_Voice_Ready;
    private SerializedProperty property_Voice_Move;
    private SerializedProperty property_Voice_Fail;

    private ReadyEvent eventTarget;
    private GameObject stepTemplate;
    private GameObject railTemplate;
    private GameObject scoreLineTemplate;

    private Vector3 snap;

    private static GameObject dummyReadyPopup;

    private bool isAutomaticMode = false;

    //스코어모드 라인 설정.
    private bool isAutomaticMakeLine_ScoreMode = false;
    private int scoreModeLineCount = 1;

    private Texture BackGround
    {
        get
        {
            return eventTarget.transform.Find("back").GetComponent<UITexture>().mainTexture;
        }
        set
        {
            eventTarget.transform.Find("back").GetComponent<UITexture>().mainTexture = value;
        }
    }

    private Texture Star
    {
        get
        {
            return eventTarget._star[0].mainTexture;
        }
        set
        {
            foreach (var star in eventTarget._star)
            {
                star.mainTexture = value;
            }
        }
    }

    private Texture Free
    {
        get
        {
            return eventTarget._free[0].GetComponent<UITexture>().mainTexture;
        }
        set
        {
            foreach (var free in eventTarget._free)
            {
                free.GetComponent<UITexture>().mainTexture = value;
            }
        }
    }

    private Texture CollectBack
    {
        get
        {
            return eventTarget.collectRoot.transform.Find("Texture").GetComponent<UITexture>().mainTexture;
        }
        set
        {
            eventTarget.collectRoot.transform.Find("Texture").GetComponent<UITexture>().mainTexture = value;
        }
    }

    private int StepToggle
    {
        get
        {
            if (eventTarget._step[0].mainTexture.name == eventTarget._textureStepOn.name)
                return 0;
            else
                return 1;
        }
        set
        {
            if (value == 0)
                SetStepTexture(true);
            else
                SetStepTexture(false);
        }
    }

    private bool GiftRoot
    {
        get
        {
            return eventTarget.giftRoot.gameObject.activeSelf;
        }
        set
        {
            eventTarget.giftRoot.gameObject.SetActive(value);
        }
    }
    private bool RailEditor
    {
        get
        {
            if (railTemplate == null)
            {
                Transform rail = eventTarget.transform.Find("back").Find("rail");
                if (rail == null)
                    return false;
                else
                    return rail.gameObject.activeSelf;
            }

            return railTemplate.transform.parent.gameObject.activeSelf;
        }
        set
        {
            if (value)
            {
                #region railTemplate Setting
                Transform tempBack = eventTarget.transform.Find("back");
                Transform tempRail = tempBack.Find("rail");
                if (tempRail == null)
                {
                    GameObject rail = new GameObject();
                    rail.name = "rail";
                    rail.layer = LayerMask.NameToLayer("UI");
                    rail.transform.SetParent(tempBack);
                    rail.transform.localPosition = Vector3.down * tempBack.localPosition.y;
                    rail.transform.localScale = Vector3.one;
                    tempRail = rail.transform;
                }
                Transform tempRail1 = tempRail.Find("rail1");
                if (tempRail1 == null)
                {
                    GameObject rail1 = new GameObject();
                    rail1.name = "rail1";
                    rail1.layer = LayerMask.NameToLayer("UI");
                    rail1.transform.SetParent(tempRail);
                    rail1.transform.localPosition = Vector3.zero;
                    rail1.transform.localScale = Vector3.one;
                    UITexture railTexture = rail1.AddComponent<UITexture>();
                    railTexture.depth = 11;
                    tempRail1 = rail1.transform;
                }
                railTemplate = tempRail1.gameObject;
                #endregion
            }
            else
            {
                GiftRoot = true;
                Transform rail = eventTarget.transform.Find("back").Find("rail");
                if (rail != null)
                    DestroyImmediate(rail.gameObject);
            }

            if (railTemplate != null)
                railTemplate.transform.parent.gameObject.SetActive(value);


        }
    }
    private Texture RailTexture
    {
        get
        {
            return GetRailsTexture()[0].mainTexture;
        }
        set
        {
            UITexture[] textures = GetRailsTexture();
            for (int i = 0; i < textures.Length; i++)
            {
                textures[i].mainTexture = value;
            }
        }
    }
    private int RailWeight
    {
        get
        {
            return GetRailsTexture()[0].height;
        }
        set
        {
            UITexture[] rails = GetRailsTexture();
            foreach (var rail in rails)
            {
                rail.height = value;
            }
        }
    }
    private float RailOffsetHeight
    {
        get
        {
            return eventTarget.transform.Find("back").Find("rail").localPosition.y;
        }
        set
        {
            Transform root = eventTarget.transform.Find("back").Find("rail");
            Vector3 pos = root.localPosition;
            pos.y = value;
            root.localPosition = pos;
        }
    }

    private bool StarEditor
    {
        get
        {
            return eventTarget._star[0].gameObject.activeSelf;
        }
        set
        {
            foreach (var star in eventTarget._star)
            {
                star.gameObject.SetActive(value);
            }
        }
    }
    private Vector2 StarPosition
    {
        get
        {
            return eventTarget._star[0].transform.localPosition;
        }
        set
        {
            foreach (var star in eventTarget._star)
            {
                star.transform.localPosition = new Vector3(value.x, value.y, star.transform.localPosition.z);
            }
        }
    }

    private bool FreeEditor
    {
        get
        {
            return eventTarget._free[0].gameObject.activeSelf;
        }
        set
        {
            foreach (var free in eventTarget._free)
            {
                free.gameObject.SetActive(value);
                if (eventTarget.waveRoot != null)
                    eventTarget.waveRoot.SetActive(value);
            }
        }
    }
    private Vector2 FreePosition
    {
        get
        {
            return eventTarget._free[0].transform.localPosition;
        }
        set
        {
            foreach (var free in eventTarget._free)
            {
                free.transform.localPosition = new Vector3(value.x, value.y, free.transform.localPosition.z);
            }
        }
    }
    
    private Vector2 WaveRootPosition
    {
        get
        {
            if (eventTarget.waveRoot != null)
                return eventTarget.waveRoot.transform.localPosition;
            return Vector3.zero;
        }
        set
        {
            if (eventTarget.waveRoot != null)
                eventTarget.waveRoot.transform.localPosition = new Vector3(value.x, value.y, eventTarget.waveRoot.transform.localPosition.z);
        }
    }

    private bool MaterialEditor
    {
        get
        {
            return eventTarget.collectRoot.activeSelf;
        }
        set
        {
            eventTarget.collectRoot.SetActive(value);

            foreach (var step in eventTarget._step)
            {
                step.transform.Find("MaterialRoot").gameObject.SetActive(value);
            }
        }
    }
    private Vector2 MaterialRootPosition
    {
        get
        {
            return eventTarget.collectRoot.transform.localPosition;
        }
        set
        {
            eventTarget.collectRoot.transform.localPosition = new Vector3(value.x, value.y, eventTarget.collectRoot.transform.localPosition.z);
        }
    }
    private Vector2 MaterialPosition
    {
        get
        {
            return eventTarget._step[0].transform.Find("MaterialRoot").localPosition;
        }
        set
        {
            foreach (var step in eventTarget._step)
            {
                Transform materialTransform = step.transform.Find("MaterialRoot");
                materialTransform.localPosition = new Vector3(value.x, value.y, materialTransform.localPosition.z);
            }
        }
    }

    #region 스코어 모드
    private bool ScoreEditor
    {
        get
        {
            if (eventTarget.scoreRoot == null)
                return false;
            else
                return eventTarget.scoreRoot.activeSelf;
        }
        set
        {
            if (eventTarget.scoreRoot != null)
                eventTarget.scoreRoot.SetActive(value);

            foreach (var step in eventTarget._step)
            {
                Transform scoreTransform = step.transform.Find("ScoreRoot");
                if (scoreTransform != null)
                    scoreTransform.gameObject.SetActive(value);
            }
        }
    }

    //자동으로 보상 라인이 생성되게 할지에 대한 설정.
    private bool AutoMakeRewardLIne_ScoreMode
    {
        get
        {
            return isAutomaticMakeLine_ScoreMode;
        }
        set
        {
            if (isAutomaticMakeLine_ScoreMode == value)
                return;

            isAutomaticMakeLine_ScoreMode = value;

            if (isAutomaticMakeLine_ScoreMode == true)
            {
                SetScoreModeRewardLine();
            }
        }
    }

    //스코어 모드 보상 라인 및 텍스트 설정
    private void SetScoreModeRewardLine()
    {
        //스코어 모드 보상 라인들의 루트 설정
        Transform rewardLineRootTr = eventTarget.scoreProgressBar.transform.parent.Find("RewardLine");
        if (rewardLineRootTr == null)
            return;
        
        //생성할 라인 오브젝트 찾기
        if (scoreLineTemplate == null)
        {
            Transform rewardLineTr = rewardLineRootTr.Find("Sprite_Line_1");
            if (rewardLineTr == null)
                return;

            scoreLineTemplate = rewardLineTr.gameObject;
        }

        //라인 리스트 재 정리
        InitListScoreRewardLine();

        //라인이 더 필요하거나 너무 많을 경우, 라인 재 조정.
        if (IsMakeOrRemoveRewardLine_ScoreMode() == true)
        {
            //뱃지 카운트 만큼 라인 생성.
            MakeOrRemoveRewardLine_ScoreMode(rewardLineRootTr);

            //라인 위치 재 설정
            SetRewardLinePosition_ScoreMode();
        }
    }

    //오브젝트 삭제 여부나, 이름을 확인해서 스코어 모드에서 사용하는 라인 리스트 재 정리.
    private void InitListScoreRewardLine()
    {
        for (int i = eventTarget.listScoreRewardLine.Count - 1; i >= 0; i--)
        {
            UISprite spriteLine = eventTarget.listScoreRewardLine[i].spriteRewardLine;
            if (spriteLine == null || spriteLine.gameObject == null)
            {
                eventTarget.listScoreRewardLine.RemoveAt(i);
            }
            else
            {
                string name = string.Format("Sprite_Line_{0}", (i + 1));
                spriteLine.gameObject.name = name;
            }
        }

        //기본이 되는 라인이 리스트에 등록되어 있지 않은 경우, 리스트에 추가시켜줌.
        if (eventTarget.listScoreRewardLine.Count == 0)
        {
            UISprite spriteScoreLine = scoreLineTemplate.GetComponent<UISprite>();
            UILabel textScoreLine = scoreLineTemplate.GetComponentInChildren<UILabel>(true);
            ReadyEvent.ScoreModeRewardLine scoreModeRewardLine = new ReadyEvent.ScoreModeRewardLine()
            {
                spriteRewardLine = spriteScoreLine,
                textRewardCount = textScoreLine,
            };
            eventTarget.listScoreRewardLine.Add(scoreModeRewardLine);
        }
    }

    private bool IsMakeOrRemoveRewardLine_ScoreMode()
    {
        //생성할 라인 수 설정(맨 마지막 값에는 라인이 필요없기 때문에 -1 해줌)
        if (isAutomaticMakeLine_ScoreMode == true)
            scoreModeLineCount = (eventTarget._step.Count * 3) - 1;

        //가지고 있는 라인 수가 생성할 뱃지 수와 동일하다면 라인을 새로 만들지 않음.
        if (eventTarget.listScoreRewardLine.Count == scoreModeLineCount)
            return false;

        return true;
    }

    private void MakeOrRemoveRewardLine_ScoreMode(Transform parentTr)
    {
        if (eventTarget.listScoreRewardLine.Count > scoreModeLineCount)
        {
            for (int i = eventTarget.listScoreRewardLine.Count - 1; i >= scoreModeLineCount; i--)
            {
                UISprite spriteLine = eventTarget.listScoreRewardLine[i].spriteRewardLine;
                if (spriteLine != null || spriteLine.gameObject != null)
                {
                    if (i == 0)
                        spriteLine.gameObject.SetActive(false);
                    else
                        DestroyImmediate(spriteLine.gameObject);
                }
                eventTarget.listScoreRewardLine.RemoveAt(i);
            }
        }
        else
        {
            int lineObjCount = eventTarget.listScoreRewardLine.Count;

            //처음의 라인이 비활성 상태이면, 활성상태로 변경시켜줌.
            if (lineObjCount > 0 && eventTarget.listScoreRewardLine[0].spriteRewardLine.gameObject.activeInHierarchy == false)
                eventTarget.listScoreRewardLine[0].spriteRewardLine.gameObject.SetActive(true);

            //원하는 갯수만큼 라인을 생성.
            for (int i = lineObjCount; i < scoreModeLineCount; i++)
            {
                GameObject temp = Instantiate(scoreLineTemplate);
                temp.name = "Sprite_Line_" + (i + 1);
                temp.transform.SetParent(parentTr);

                //현재 생성한 라인 오브젝트에서 필요한 정보를 리스트에 추가.
                UISprite spriteScoreLine = temp.GetComponent<UISprite>();
                UILabel textScoreLine = temp.GetComponentInChildren<UILabel>(true);
                ReadyEvent.ScoreModeRewardLine scoreModeRewardLine = new ReadyEvent.ScoreModeRewardLine
                {
                    spriteRewardLine = spriteScoreLine,
                    textRewardCount = textScoreLine
                };
                eventTarget.listScoreRewardLine.Add(scoreModeRewardLine);
            }
        }
    }

    private void SetRewardLinePosition_ScoreMode()
    {
        int width = eventTarget.scoreProgressBar.foregroundWidget.width;

        //칸 간격.
        float step = ((float)width / (scoreModeLineCount + 1));

        //라인 설정
        for (int i = 0; i < eventTarget.listScoreRewardLine.Count; i++)
        {
            //라인 위치 설정
            float xPos = (width * -0.5f) + (step * (i + 1)) + 0.5f;
            Transform lineTr = eventTarget.listScoreRewardLine[i].spriteRewardLine.transform;
            lineTr.localPosition = new Vector3(xPos, 0f, 0f);
            lineTr.localScale = new Vector3(0.8f, 1f, 1f);

            //라인 두께 및 알파 설정
            eventTarget.listScoreRewardLine[i].spriteRewardLine.width = 1;
            eventTarget.listScoreRewardLine[i].spriteRewardLine.alpha = 0.5f;

            //텍스트 비활성화
            eventTarget.listScoreRewardLine[i].textRewardCount.gameObject.SetActive(false);
        }
    }

    private Vector2 ScoreRootPosition
    {
        get
        {
            if (eventTarget.scoreRoot == null)
                return Vector2.zero;
            else
                return eventTarget.scoreRoot.transform.localPosition;
        }
        set
        {
            if (eventTarget.scoreRoot != null)
                eventTarget.scoreRoot.transform.localPosition = new Vector3(value.x, value.y, eventTarget.scoreRoot.transform.localPosition.z);
        }
    }

    private Vector2 ScorePosition
    {
        get
        {
            Transform scoreTransform = eventTarget._step[0].transform.Find("ScoreRoot");
            if (scoreTransform == null)
                return Vector3.zero;
            else
                return scoreTransform.localPosition;
        }
        set
        {
            foreach (var step in eventTarget._step)
            {
                Transform scoreTransform = step.transform.Find("ScoreRoot");
                if (scoreTransform != null)
                    scoreTransform.localPosition = new Vector3(value.x, value.y, scoreTransform.localPosition.z);
            }
        }
    }

    private NGUIAtlas ScoreAtlas
    {
        get
        {
            if (eventTarget.scoreModeAtlas == null)
                return null;
            else
                return eventTarget.scoreModeAtlas;
        }
        set
        {
            eventTarget.scoreModeAtlas = value;
        }
    }

    private Vector2 PointOffset
    {
        get
        {
            return eventTarget._offsetPoint;
        }
        set
        {
            eventTarget._offsetPoint = new Vector3(value.x, value.y, 0.0f);

            eventTarget._texturePointShadow.transform.localPosition = GetCurrentStepPosition() + (Vector2)eventTarget._offsetPoint;
        }
    }
    #endregion

    private bool DummyActive
    {
        get
        {
            return (dummyReadyPopup != null);
        }
        set
        {
            if (value)
            {
                if(dummyReadyPopup == null)
                {
                    dummyReadyPopup = Instantiate(Resources.Load("UIPrefab/UIPopupReady"), eventTarget.transform.parent) as GameObject;
                    dummyReadyPopup.hideFlags = HideFlags.DontSave | HideFlags.NotEditable | HideFlags.HideInHierarchy;
                }
            }
            else
            {
                if (dummyReadyPopup != null)
                {
                    DestroyImmediate(dummyReadyPopup);
                    dummyReadyPopup = null;
                }
            }
        }
    }

    private ReadyEvent CopyTarget;

    private static int currentStepIndex = -1;
    private int CurrentStepIndex
    {
        get
        {
            if (currentStepIndex == -1)
                currentStepIndex = GetCurrentStep();

            return currentStepIndex;
        }
        set
        {
            if (value >= 0 && value <= eventTarget._step.Count)
                currentStepIndex = value;
        }
    }
    private const float ERROR_RANGE = 5.0f;

    private static ReadyEventEditor _instance;

    private void OnEnable()
    {
        if(_instance == null)
            _instance = this;

        eventTarget = target as ReadyEvent;

        stepTemplate = eventTarget.transform.GetComponentInChildren<ReadyEventStage>().gameObject;

        property_Live2D_Character = serializedObject.FindProperty("live2dCharacter");
        property_Live2D_Size = serializedObject.FindProperty("live2dSize");
        property_Live2D_Offset = serializedObject.FindProperty("live2dOffset");

        property_Voice_Ready = serializedObject.FindProperty("readyVoice");
        property_Voice_Move = serializedObject.FindProperty("moveVoice");
        property_Voice_Fail = serializedObject.FindProperty("failVoice");
        //serializedBG = new SerializedObject(eventTarget.transform.Find("back").GetComponent<UITexture>());
        //property_BackGround = serializedBG.FindProperty("mTexture");

        #region OnSceneGUI
        var snapX = EditorPrefs.GetFloat("MoveSnapX", 1f);
        var snapY = EditorPrefs.GetFloat("MoveSnapY", 1f);
        var snapZ = EditorPrefs.GetFloat("MoveSnapZ", 1f);
        snap = new Vector3(snapX, snapY, snapZ);
        #endregion
    }

    void OnDisable()
    {
        if(_instance == this)
            _instance = null;

        DummyActive = false;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Resource Editor");
        EditorStyles.label.fontStyle = FontStyle.Normal;

        #region BG Texture Setting
        EditorGUI.BeginChangeCheck();
        BackGround = EditorGUILayout.ObjectField("BackGround", BackGround, typeof(Texture), false) as Texture;
        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
        #endregion

        #region Step Texture Setting
        EditorGUI.BeginChangeCheck();
        eventTarget._textureStepOn = EditorGUILayout.ObjectField("Step_On", eventTarget._textureStepOn, typeof(Texture), false) as Texture;
        if (EditorGUI.EndChangeCheck())
        {
            SetStepTexture(true);
        }

        EditorGUI.BeginChangeCheck();
        eventTarget._textureStepOff = EditorGUILayout.ObjectField("Step_Off", eventTarget._textureStepOff, typeof(Texture), false) as Texture;
        if (EditorGUI.EndChangeCheck())
        {
            SetStepTexture(false);
        }
        #endregion

        #region Etc Texture Setting
        Star = EditorGUILayout.ObjectField("Star", Star, typeof(Texture), false) as Texture;

        Free = EditorGUILayout.ObjectField("Free", Free, typeof(Texture), false) as Texture;

        CollectBack = EditorGUILayout.ObjectField("CollectBack", CollectBack, typeof(Texture), false) as Texture;

        eventTarget._texturePoint.mainTexture = EditorGUILayout.ObjectField("Point", eventTarget._texturePoint.mainTexture, typeof(Texture), false) as Texture;
        eventTarget._texturePoint.MakePixelPerfect();

        eventTarget._texturePointShadow.mainTexture = EditorGUILayout.ObjectField("PointShadow", eventTarget._texturePointShadow.mainTexture, typeof(Texture), false) as Texture;
        eventTarget._texturePointShadow.MakePixelPerfect();
        #endregion

        GUILayout.Space(20.0f);
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Step Editor");
        EditorStyles.label.fontStyle = FontStyle.Normal;

        StepToggle = (GUILayout.Toolbar(StepToggle, new string[] { "Step_On", "Step_Off" }));

        #region Step Count Setting
        using (new EditorGUILayout.HorizontalScope())
        {
            //EditorGUILayout.IntSlider(property_Step.arraySize, 3, 5, GUILayout.ExpandWidth);
            EditorGUILayout.LabelField("StepCount [" + eventTarget._step.Count + "]");
            if (eventTarget._step.Count == 1)
                GUI.enabled = false;
            if (GUILayout.Button("-", EditorStyles.miniButtonLeft))
            {
                SubStep();
            }
            GUI.enabled = true;

            if (GUILayout.Button("+", EditorStyles.miniButtonRight))
            {
                AddStep();
            }
        }
        #endregion

        #region Automatic Sorting
        if (isAutomaticMode)
        {
            GUI.color = Color.green;
            if (GUILayout.Button("Sort"))
            {
                isAutomaticMode = false;
                AutomaticPositioning();
            }
            GUI.color = Color.white;
        }
        else
        {
            if (GUILayout.Button("Automatic Sorting Mode"))
            {
                SceneView.RepaintAll();
                isAutomaticMode = true;
            }
        }
        #endregion

        GUI.enabled = PrefabUtility.GetPrefabType(target) == PrefabType.Prefab ? false : true;
        GUILayout.Space(20.0f);

        #region Rail Setting
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Rail Editor" + (PrefabUtility.GetPrefabType(target) == PrefabType.Prefab ? " (PrefabInstance Only)" : ""));
        EditorStyles.label.fontStyle = FontStyle.Normal;
        RailEditor = EditorGUILayout.ToggleLeft("Rail On/Off", RailEditor);
        if (RailEditor)
        {
            RailTexture = EditorGUILayout.ObjectField("Rail", RailTexture, typeof(Texture), false) as Texture;
            RailWeight = EditorGUILayout.IntField("Rail Weight", RailWeight);
            RailOffsetHeight = EditorGUILayout.FloatField("Rail Offset Height", RailOffsetHeight);

            using (new EditorGUILayout.HorizontalScope())
            {
                GiftRoot = GUILayout.Toggle(GiftRoot, "Gift Include");

                if (GUILayout.Button("Reset"))
                {
                    RailReset();
                }
            }
        }
        #endregion

        GUI.enabled = true;

        GUILayout.Space(20.0f);
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Parts Editor");
        EditorStyles.label.fontStyle = FontStyle.Normal;

        #region Star Editor Mode
        StarEditor = EditorGUILayout.ToggleLeft("Star Editor", StarEditor);
        if (StarEditor)
        {
            StarPosition = EditorGUILayout.Vector2Field("StarPosition", StarPosition);
        }
        #endregion

        #region Free Editor Mode
        FreeEditor = EditorGUILayout.ToggleLeft("Free Editor", FreeEditor);
        if (FreeEditor)
        {
            FreePosition = EditorGUILayout.Vector2Field("FreePosition", FreePosition);
            WaveRootPosition = EditorGUILayout.Vector2Field("WaveRootPosition", WaveRootPosition);
        }
        #endregion

        #region Material Editor Mode
        MaterialEditor = EditorGUILayout.ToggleLeft("Material Editor", MaterialEditor);
        if (MaterialEditor)
        {
            MaterialRootPosition = EditorGUILayout.Vector2Field("MaterialRootPosition", MaterialRootPosition);
            MaterialPosition = EditorGUILayout.Vector2Field("MaterialPosition", MaterialPosition);
        }
        #endregion

        #region Score Editor Mode
        ScoreEditor = EditorGUILayout.ToggleLeft("Score Editor", ScoreEditor);
        if (ScoreEditor)
        {   
            ScoreRootPosition = EditorGUILayout.Vector2Field("ScoreRootPosition", ScoreRootPosition);
            ScorePosition = EditorGUILayout.Vector2Field("ScorePosition", ScorePosition);

            //스코어모드 보상 라인 설정
            AutoMakeRewardLIne_ScoreMode = EditorGUILayout.Toggle("스코어모드 보상 라인 자동 생성", AutoMakeRewardLIne_ScoreMode);
            if (AutoMakeRewardLIne_ScoreMode == false)
            {
                EditorGUILayout.BeginHorizontal();
                scoreModeLineCount = EditorGUILayout.IntField("   스코어 모드 뱃지 카운트 : ", scoreModeLineCount);
                if (GUILayout.Button("적용"))
                {
                    SetScoreModeRewardLine();
                }
                EditorGUILayout.EndHorizontal();
            }

            //마지막 보상 위치 설정
            if (eventTarget.maxBadgeText == null)
            {
                Transform maxBadgeTr = eventTarget.scoreProgressBar.transform.parent.Find("BadgeCount");
                if (maxBadgeTr != null)
                    eventTarget.maxBadgeText = maxBadgeTr.gameObject.GetComponent<UILabel>();
            }

            //스코어 모드 아틀라스 설정
            ScoreAtlas = EditorGUILayout.ObjectField("ScoreMode_Atlas", ScoreAtlas, typeof(NGUIAtlas), false) as NGUIAtlas;
            if (GUILayout.Button("ScoreMode Atlas Apply"))
            {
                ApplyScoreModeAtas();
            }
        }
        #endregion

        GUILayout.Space(20.0f);
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Point Editor");
        EditorStyles.label.fontStyle = FontStyle.Normal;

        PointOffset = EditorGUILayout.Vector2Field("Point Offset", PointOffset);

        #region PointMove
        using (new EditorGUILayout.HorizontalScope())
        {
            //EditorGUILayout.IntSlider(property_Step.arraySize, 3, 5, GUILayout.ExpandWidth);
            EditorGUILayout.LabelField("Point Move");

            if (GUILayout.Button("<", EditorStyles.miniButtonLeft))
            {
                StepMoveSub();
            }

            if (GUILayout.Button(">", EditorStyles.miniButtonRight))
            {
                StepMoveAdd();
            }
        }
        #endregion

        #region Pokota Editor
        GUI.enabled = PrefabUtility.GetPrefabType(target) == PrefabType.Prefab ? false : true;
        GUILayout.Space(20.0f);
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Pokota Editor" + (PrefabUtility.GetPrefabType(target) == PrefabType.Prefab ? " (PrefabInstance Only)" : ""));
        EditorStyles.label.fontStyle = FontStyle.Normal;
        
        DummyActive = EditorGUILayout.ToggleLeft("DummyPopup", DummyActive);
        if (DummyActive)
        {
            EditorGUILayout.PropertyField(property_Live2D_Character);
            EditorGUILayout.PropertyField(property_Live2D_Size);
            EditorGUILayout.PropertyField(property_Live2D_Offset);
        }

        GUI.enabled = true;
        #endregion

        #region Voice Editor
        GUILayout.Space(20.0f);
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Voice Editor");
        EditorStyles.label.fontStyle = FontStyle.Normal;

        EditorGUILayout.PropertyField(property_Voice_Ready);
        EditorGUILayout.PropertyField(property_Voice_Move);
        EditorGUILayout.PropertyField(property_Voice_Fail);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Set None"))
        {
            eventTarget.readyVoice = AudioLobby.NO_SOUND;
            eventTarget.moveVoice = AudioLobby.NO_SOUND;
            eventTarget.failVoice = AudioLobby.NO_SOUND;
        }
        if (GUILayout.Button("Set Boni Sound"))
        {
            eventTarget.readyVoice = AudioLobby.m_boni_tubi;
            eventTarget.moveVoice = AudioLobby.m_bird_hehe;
            eventTarget.failVoice = AudioLobby.m_bird_hansum;
        }
        EditorGUILayout.EndHorizontal();
        
        #endregion

        #region Copy Utility

        GUILayout.Space(20.0f);
        EditorStyles.label.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField("Copy Utility");
        EditorStyles.label.fontStyle = FontStyle.Normal;

        CopyTarget = EditorGUILayout.ObjectField("target", CopyTarget, typeof(ReadyEvent), false) as ReadyEvent;

        GUI.enabled = CopyTarget != null;
        if (GUILayout.Button("Copy Step"))
        {
            CopyStep();
        }
        if (GUILayout.Button("Copy Parts"))
        {
            CopyParts();
        }

        GUI.enabled = true;

        #endregion

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(eventTarget.gameObject.scene);

            serializedObject.ApplyModifiedProperties();
        }
    }

    private void OnSceneGUI()
    {
        Tools.current = Tool.None;

        foreach (var step in eventTarget._step)
        {
            step.transform.position = _instance.PositionHandle(step.transform);
        }
        eventTarget.giftRoot.position = _instance.PositionHandle(eventTarget.giftRoot);

        _instance.SetStepMove(_instance.CurrentStepIndex);

        if(_instance.isAutomaticMode)
            _instance.AutomaticSortingGuide();

        if (_instance.DummyActive)
            _instance.PokotaGuide();
    }

    private Vector2 PositionHandle(Transform transform)
    {
        Vector2 position = transform.position;

        Handles.color = Handles.xAxisColor;
        position = Handles.Slider(position, transform.right, 0.1f, Handles.ArrowHandleCap, snap.x);
        Handles.color = Handles.yAxisColor;
        position = Handles.Slider(position, transform.up, 0.1f, Handles.ArrowHandleCap, snap.y);

        //Handles.color = Color.red;
        position = Handles.FreeMoveHandle(position, transform.rotation, 0.02f, snap, Handles.RectangleHandleCap);

        return position;
    }

    private float handle_Up = 0.3f;
    private float handle_Down = 0.2f;
    private void AutomaticSortingGuide()
    {
        Rect r = new Rect(
            eventTarget._step[0].transform.position.x, 
            handle_Down, 
            eventTarget.giftRoot.position.x - eventTarget._step[0].transform.position.x, 
            handle_Up - handle_Down
            );
        Handles.DrawSolidRectangleWithOutline(r, Color.clear, Color.black);

        handle_Up = Handles.FreeMoveHandle(Vector3.up * handle_Up, eventTarget.transform.rotation, 0.05f, snap, Handles.SphereHandleCap).y;
        handle_Down = Handles.FreeMoveHandle(Vector3.up * handle_Down, eventTarget.transform.rotation, 0.05f, snap, Handles.SphereHandleCap).y;

        Handles.color = Color.red;
        int count = eventTarget._step.Count;
        for (int i=0; i<=eventTarget._step.Count; i++)
        {
            float x = Mathf.Lerp(eventTarget._step[0].transform.position.x, eventTarget.giftRoot.position.x, (float)i / count);
            float y = (i % 2 == 0 ? handle_Up : handle_Down);
            Handles.SphereHandleCap(0, new Vector2(x, y), eventTarget.transform.rotation, 0.02f, EventType.Repaint);
        }
    }

    private void PokotaGuide()
    {
        Vector2 size = property_Live2D_Size.vector3Value;
        size.x = size.x * eventTarget.transform.lossyScale.x;
        size.y = size.y * eventTarget.transform.lossyScale.y;

        Vector2 pos = property_Live2D_Offset.vector3Value;
        pos += new Vector2(221f, -280f);
        pos.x = pos.x * eventTarget.transform.lossyScale.x;
        pos.y = pos.y * eventTarget.transform.lossyScale.y;
        pos -= size * 0.5f;


        //Handles.RectangleHandleCap(0, Vector3.zero + pos, eventTarget.transform.rotation, 0.15f, EventType.repaint);

        Rect r = new Rect(pos.x, pos.y, size.x, size.y);
        Handles.DrawSolidRectangleWithOutline(r, new Color(0.0f, 1.0f, 0.0f, 0.5f), Color.green);
    }

    [DrawGizmo(GizmoType.NonSelected | GizmoType.Active)]
    private static void DrawGizmo(ReadyEvent eTarget, GizmoType type)
    {
        Gizmos.color = Color.blue;

        Gizmos.DrawWireCube(eTarget._texturePointShadow.transform.position - eTarget._texturePointShadow.transform.TransformVector(eTarget._offsetPoint), Vector3.one * 0.02f);

        Gizmos.color = Color.red;

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.red;
        style.fontSize = 20;
        int count = 0;
        foreach (var step in eTarget._step)
        {
            count++;
            Handles.Label(step.transform.position + Vector3.down * 0.02f, "step<" + count.ToString() + ">", style);
            Gizmos.DrawLine(step.transform.position + Vector3.left * 0.02f, step.transform.position + Vector3.right * 0.02f);
            Gizmos.DrawLine(step.transform.position + Vector3.down * 0.02f, step.transform.position + Vector3.up * 0.02f);
        }

        Handles.Label(eTarget.giftRoot.position + Vector3.down * 0.02f, "GiftRoot", style);
        Gizmos.DrawLine(eTarget.giftRoot.position + Vector3.left * 0.02f, eTarget.giftRoot.position + Vector3.right * 0.02f);
        Gizmos.DrawLine(eTarget.giftRoot.position + Vector3.down * 0.02f, eTarget.giftRoot.position + Vector3.up * 0.02f);
    }

    private void AddStep()
    {
        GameObject temp = Instantiate(stepTemplate);
        temp.name = "step" + (eventTarget._step.Count + 1);
        temp.transform.SetParent(eventTarget.transform);
        temp.transform.localScale = Vector3.one;
        temp.transform.localPosition = Vector3.zero;

        eventTarget._step.Add(temp.GetComponent<UITexture>());
        eventTarget._star.Add(temp.transform.Find("star").GetComponent<UITexture>());
        eventTarget._free.Add(temp.transform.Find("free").gameObject);
    }

    private void SubStep()
    {
        var steps = GetSteps();

        DestroyImmediate(steps[steps.Length - 1].gameObject);

        eventTarget._step.RemoveAt(eventTarget._step.Count - 1);
        eventTarget._star.RemoveAt(eventTarget._star.Count - 1);
        eventTarget._free.RemoveAt(eventTarget._free.Count - 1);
    }

    private ReadyEventStage[] GetSteps()
    {
        return eventTarget.GetComponentsInChildren<ReadyEventStage>();
    }

    private void SetStepTexture(bool isOn)
    {
        foreach (UITexture step in eventTarget._step)
        {
            step.mainTexture = isOn ? eventTarget._textureStepOn : eventTarget._textureStepOff;
            step.MakePixelPerfect();
        }
    }

    private void AutomaticPositioning()
    {
        var steps = GetSteps();

        for(int i=0; i<steps.Length; i++)
        {
            float x = Mathf.Lerp(eventTarget._step[0].transform.position.x, eventTarget.giftRoot.position.x, (float)i / steps.Length);
            float y = (i % 2 == 0 ? handle_Up : handle_Down);

            steps[i].transform.position = new Vector2(x, y);
        }

        eventTarget.giftRoot.position = new Vector3(eventTarget.giftRoot.position.x, (steps.Length % 2 == 0 ? handle_Up : handle_Down), eventTarget.giftRoot.position.z);
    }

    private int GetCurrentStep()
    {
        float pointPosition = eventTarget._texturePointShadow.transform.localPosition.x + eventTarget._offsetPoint.x;

        for (int i=0; i<eventTarget._step.Count; i++)
        {
            if (eventTarget._step[i].transform.localPosition.x - ERROR_RANGE <= pointPosition &&
                eventTarget._step[i].transform.localPosition.x + ERROR_RANGE >= pointPosition)
            {
                return i;
            }
        }

        if (eventTarget.giftRoot.localPosition.x - ERROR_RANGE <= pointPosition &&
            eventTarget.giftRoot.localPosition.x + ERROR_RANGE >= pointPosition)
        {
            return eventTarget._step.Count;
        }

        return -1;
    }
    
    private Vector2 GetCurrentStepPosition()
    {
        int stepIndex = CurrentStepIndex;

        if (stepIndex < eventTarget._step.Count && stepIndex >= 0)
        {
            return eventTarget._step[stepIndex].transform.localPosition;
        }
        else if (stepIndex == eventTarget._step.Count)
        {
            return eventTarget.giftRoot.localPosition;
        }

        return eventTarget._step[0].transform.localPosition;
    }

    private void SetStepMove(int stepIndex)
    {
        //Debug.Log("SetStepMove : " + stepIndex);
        if (stepIndex >= 0 && stepIndex < eventTarget._step.Count)
        {
            eventTarget._texturePointShadow.transform.localPosition = eventTarget._step[stepIndex].transform.localPosition + eventTarget._offsetPoint;
        }
        else if (stepIndex == eventTarget._step.Count)
        {
            eventTarget._texturePointShadow.transform.localPosition = eventTarget.giftRoot.localPosition + eventTarget._offsetPoint;
        }
        else
        {
            CurrentStepIndex = 0;
            eventTarget._texturePointShadow.transform.localPosition = eventTarget._step[0].transform.localPosition + eventTarget._offsetPoint;
        }
    }

    private void StepMoveAdd()
    {
        CurrentStepIndex++;
        SetStepMove(CurrentStepIndex);
    }

    private void StepMoveSub()
    {
        CurrentStepIndex--;
        SetStepMove(CurrentStepIndex);
    }

    private UITexture[] GetRailsTexture()
    {
        Transform rail = eventTarget.transform.Find("back").Find("rail");
        if (rail == null)
            return null;

        return rail.GetComponentsInChildren<UITexture>(true);
    }

    private void RailReset()
    {
        UITexture[] rails = GetRailsTexture();

        int stepCount = GiftRoot ? eventTarget._step.Count : eventTarget._step.Count - 1;

        if(rails.Length > stepCount)
        {
            int deleteCount = rails.Length - stepCount;
            for(int i=0; i<deleteCount; i++)
            {
                DestroyImmediate(rails[rails.Length - 1 - i].gameObject);
            }
        }
        else if(rails.Length < stepCount)
        {
            int createCount = stepCount - rails.Length;
            for (int i = 0; i < createCount; i++)
            {
                MakeRail(rails.Length + i);
            }
        }

        rails = GetRailsTexture();

        for (int i=0; i < stepCount; i++)
        {
            Transform stepA = eventTarget._step[i].transform;
            Transform stepB = i + 1 == eventTarget._step.Count ? eventTarget.giftRoot : eventTarget._step[i + 1].transform;

            rails[i].transform.localPosition = Vector3.Lerp(stepA.localPosition, stepB.localPosition, 0.5f);
            Vector2 tanXY = stepB.position - stepA.position;
            rails[i].transform.eulerAngles = Vector3.forward * Mathf.Atan2(tanXY.y, tanXY.x) * Mathf.Rad2Deg;
            rails[i].width = Mathf.RoundToInt( Vector3.Distance(stepA.transform.position, stepB.transform.position) / rails[i].transform.lossyScale.x );
        }
    }

    private GameObject MakeRail(int index)
    {
        Transform tempRail = eventTarget.transform.Find("back").Find("rail");

        GameObject rail = Instantiate<GameObject>(railTemplate);
        rail.name = "rail" + (index + 1).ToString();
        rail.layer = LayerMask.NameToLayer("UI");
        rail.transform.SetParent(tempRail);
        rail.transform.localPosition = Vector3.zero;
        rail.transform.localScale = Vector3.one;

        return rail;
    }

    private void CopyStep()
    {
        if (eventTarget._step.Count < CopyTarget._step.Count)
        {
            int count = CopyTarget._step.Count - eventTarget._step.Count;

            for (int i = 0; i < count; i++)
                AddStep();
        }
        else if (eventTarget._step.Count < CopyTarget._step.Count)
        {
            int count = eventTarget._step.Count - CopyTarget._step.Count;

            for (int i = 0; i < count; i++)
                SubStep();
        }

        for (int i = 0; i < eventTarget._step.Count; i++)
        {
            eventTarget._step[i].transform.localPosition = CopyTarget._step[i].transform.localPosition;
        }

        eventTarget.giftRoot.localPosition = CopyTarget.giftRoot.localPosition;
    }

    private void CopyParts()
    {
        StarPosition = CopyTarget._star[0].transform.localPosition;
        FreePosition = CopyTarget._free[0].transform.localPosition;
        MaterialRootPosition = CopyTarget.collectRoot.transform.localPosition;
        if (CopyTarget.waveRoot != null)
            WaveRootPosition = CopyTarget.waveRoot.transform.localPosition;
        MaterialPosition = CopyTarget._step[0].transform.Find("MaterialRoot").localPosition;
        if (CopyTarget.scoreRoot != null)
            ScoreRootPosition = CopyTarget.scoreRoot.transform.localPosition;
        ScorePosition = CopyTarget._step[0].transform.Find("ScoreRoot").localPosition;
    }

    private void ApplyScoreModeAtas()
    {
        //스코어 모드에서 아틀라스 사용하는 이미지들에 사용할 아틀라스 적용
        if (eventTarget.scoreModeUseAtlasSprite != null)
        {
            for (int i = 0; i < eventTarget.scoreModeUseAtlasSprite.Length; i++)
            {
                eventTarget.scoreModeUseAtlasSprite[i].atlas = ScoreAtlas;
            }
        }

        //스코어 모드 보상 프로그레스 바 이미지에 아틀라스 적용
        for (int i = 0; i < eventTarget.listScoreRewardLine.Count; i++)
        {
            if (eventTarget.listScoreRewardLine[i].spriteRewardLine != null)
                eventTarget.listScoreRewardLine[i].spriteRewardLine.atlas = ScoreAtlas;
        }

        //스코어 모드 발판 아래 뱃지 이미지에 아틀라스 적용
        for (int i = 0; i < eventTarget._step.Count; i++)
        {
            Transform scoreTransform = eventTarget._step[i].transform.Find("ScoreRoot");
            if (scoreTransform == null)
                continue;

            UISprite[] scoreBadgeImages = scoreTransform.gameObject.GetComponentsInChildren<UISprite>();
            for (int k = 0; k < scoreBadgeImages.Length; k++)
            {
                scoreBadgeImages[k].atlas = ScoreAtlas;
            }
        }
    }
}
