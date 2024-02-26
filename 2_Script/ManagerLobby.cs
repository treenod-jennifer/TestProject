using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System.Text.RegularExpressions;
using DG.Tweening;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

[System.Serializable]
public enum TypeLobbyState
{
    None,
    Preparing,              //  로비 준비중                  
    PreparingEnd,           //  로비 준비 완료

    StartEvent,             //  앱 기동 이벤트(세일,이벤트 스테이지등등 초반 앱 기동시 강제 이벤트)

    Wait,                   //  대기 상태, 보니를 조정하거나 보니가 스스로 움직이는 상태(최소 가능)
    TriggerEvent,    //  미션을 받거나 미션 완료 수행중 (취소 불가)
    NewDayEvent,            //  새로운 날 이벤트
    
    LoadingNextScene,
}

[System.Serializable]
public class SpawnPosition
{
    public Vector3 position = Vector3.zero;
    public Color color = Color.white;
    //public float radius = 2f;

    [System.NonSerialized]
    public bool used = false;
}


public class ManagerLobby : MonoBehaviour, IImageRequestable
{

    public static ManagerLobby _instance = null;

    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Character
    public List<GameObject> _objCharacter;

    public GameObject _objCharacterNPC;

    //[System.NonSerialized]
    public TypeLobbyState _state = TypeLobbyState.None;

    static public int _missionThreshold_eventstageOpen_noticeOpen_packageshopOpen = 6;    // 공지와 팩키지 상품 이벤트 스테이지는 미션이 6스테이지 이후 일때 노출
    static public int _missionThreshold_eventHousingOpen = 8;    // 이벤트 타입의 하우징은 미션진행수가 8 이상 일때 노출.

    static public bool _loadComplete = false;
    static public bool _firstLobby = true;
    static public bool _firstNotice = true;
    static public bool _stageClear = false;
    static public bool _eventStageClear = false;
    static public bool _eventStageFail = false;
    static public int _stageTryCount = 0;
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Object
    public AnimationCurve objectTouchAnimationCurve;
    public GameObject _objMissionIcon;
    public GameObject _objHousingIcon;
    public GameObject _objEventObjectUI;

    public List<GameObject> _objCharacterMotionblur;
    public AnimationCurve objectRideCurveJump;
    public AnimationCurve objectRideCurveMove;
    public AnimationCurve motionblurCurveMove;
    public List<AnimationCurve> motionblurCurveFrameList = new List<AnimationCurve>();

    public ObjectMaterial _objMaterial;
    public ObjectGiftbox _objGiftBox;
    public Pokoyura _objPokoyura;
    public Pokogoro _objPokogoro = null;
    ////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public SpawnPosition[] _spawnMaterialPosition = null;
    public SpawnPosition[] _spawnGiftBoxPosition = null;
    public SpawnPosition[] _spawnPokogoroPosition = null;
    public ObjectHousingIcon _housingIcon;
 //   public SpawnMaterialPosition[] _spawnMaterialPosition = null;

    [System.NonSerialized]
    private Dictionary<int, Character> _characterList = new Dictionary<int, Character>();
    public static bool _newDay = false;

    public List<ActionObjectHousing> _objectHousing = new List<ActionObjectHousing>();
    public static Dictionary<string, GameObject> _assetBankHousing = new Dictionary<string, GameObject>();
    public static Dictionary<string, GameObject> _assetBankEvent = new Dictionary<string, GameObject>();
    public static Dictionary<string, GameObject> _assetBank = new Dictionary<string, GameObject>();
    public static Dictionary<string, AssetBundle> _assetBankBundle = new Dictionary<string, AssetBundle>();

    [System.NonSerialized]
    public int lateClearMission = -1;

    // 네비게이트 관련
    [System.NonSerialized]
    public Vector3 _homeCameraPosition = new Vector3(-10.0f, 0f, -7.7f);
    [System.NonSerialized]
    public Vector3 _workCameraPosition = new Vector3(-10.0f, 0f, -7.7f);


    public List<ParticleSystem> _staticEffectObj = new List<ParticleSystem>();
    public List<AnimationClip> _staticAnimationObj = new List<AnimationClip>();

    // 튜토리얼 관련
    static public bool _tutorialLobbyMission_Play = false;
    static public bool _tutorialDiaryMission_Play = false;
    static public bool _tutorialQuestComplete_Play = false;
    // 포코유라 획득 연출
    static public int _activeGetPokoyura = 0;

    // 활성화된 이벤트가 있는 상태
    static public bool _activeEvent = false;
    static public bool _activeEventList = false;

    void Awake()
    {
        _instance = this;
        _loadComplete = false;
        _activeEvent = false;
        _activeEventList = false;
        _state = TypeLobbyState.None;
        ActionCameraCollider.ResetCameraCollider();
    }
    void OnDestroy()
    {
        _tutorialLobbyMission_Play = false;
        _tutorialDiaryMission_Play = false;
        _tutorialQuestComplete_Play = false;

        _activeGetPokoyura = 0;

        _activeEvent = false;
        _activeEventList = false;
    }
    IEnumerator Start()
    {

        Screen.sleepTimeout = SleepTimeout.SystemSetting;

        CameraEffect staticCamera = null;
        if (Global.join && _firstLobby)
        {
            staticCamera = CameraEffect.MakeScreenEffect(1);
            staticCamera.ApplyScreenEffect(Color.black, Color.black, 0f, false);

            /*//그로씨
            {
                var playEnd = new ServiceSDK.GrowthyCustomLog_Money
                    (
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_TAG.AP_CLOVAR,
                    ServiceSDK.GrowthyCustomLog_Money.Code_L_MRSN.G_FIRST_USE_REWARD,
                    0,
                    5,                   
                    0, //(int)(GameData.User.clover),
                    (int)(GameData.User.AllClover)//(int)(GameData.User.fclover)
                    );
                var doc = JsonConvert.SerializeObject(playEnd);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("MONEY", doc);


                var useReadyItem20 = new ServiceSDK.GrowthyCustomLog_ITEM
                  (
                     ServiceSDK.GrowthyCustomLog_ITEM.Code_L_TAG.CHANGE,
                      ServiceSDK.GrowthyCustomLog_ITEM.Code_L_ICAT.MATERIAL,
                      "MATERIAL_2",
                      "material",
                      3,
                      ServiceSDK.GrowthyCustomLog_ITEM.Code_L_RSN.G_EVENT_ADMIN
                  );
                var docStamp20 = JsonConvert.SerializeObject(useReadyItem20);
                ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ITEM", docStamp20);
            }*/


        }
        //    CameraEffect staticCamera = CameraEffect.MakeScreenEffect(1);
        //  staticCamera.ApplyScreenEffect(Color.black, Color.black, 0f, false);




        // 로비 씬에서 바로 씬 시작할때를 위해
        {
            ManagerData._instance.StartNetwork();
            // 데이타 완전이 받을때까지 대기
            while (true)
            {
                if (Global._pendingReboot) {
                    break;
                }
                
                if (ManagerData._instance._state == DataLoadState.eComplete)
                    break;
                yield return null;
            }
        }



        yield return null;
        yield return null;

        // 이벤트 연출이 있는지 없는지
        int playEventKey = -1;
        int playEventScene = -1;



        // 보니 생성
        // 로비 area들 데이타 로딩하고 상태에따라 생성(보니 위치, 카메라 위치 설정)
        _state = TypeLobbyState.Preparing;
;
        while (_state == TypeLobbyState.Preparing)
            yield return null;

        bool showBoxEventOpen = false;



        //BGM 재생.
        ManagerSound._instance.PlayBGM();

        yield return null;
        _state = TypeLobbyState.StartEvent;
        yield return null;

        SceneLoading._plaeaseWait = false;
        ManagerUI._instance.CoShowUI(0f, true, TypeShowUI.eAll);
        // 읽고 설치하고 등 모든 상태가 완료되고 밝아짐
        if (Global.join && _firstLobby)
            if (staticCamera != null)
                staticCamera.ApplyScreenEffect(Color.black, new Color(0f, 0f, 0f, 0f), 0.4f, true);
        ManagerUI._instance.UpdateUI(true);

        var reservedActions = new List<KeyValuePair<StartPostProcTypes, int>>(); 



        //string startEventName = null;

        bool pokoyuraRefreshed = false;

        // 대기
        _state = TypeLobbyState.Wait;
        // 조인(첫진입)
        if (Global.join && ManagerData._instance._missionData[1].state == TypeMissionState.Inactive)//Global.join && ManagerData._instance.missionData[0].state == TypeMissionState.Inactive)
        {
            ManagerData._instance._missionData[1].state = TypeMissionState.Active;
            _tutorialLobbyMission_Play = true;
            // 미션 아이콘 미리 읽어두기
            UIImageLoader.Instance.Load(Global.gameImageDirectory, "IconMission/", "m_1", this);

            if (ManagerArea._instance._areaStep.Count > 0)
                if (ManagerArea._instance._areaStep[0].area != null)
                {
                    PlayerPrefs.SetInt("missionCheck" + 1, 1);   // 다이어리 미션 아이콘 연출관련
                    PlaySceneWakeUp(ManagerArea._instance._areaStep[0].area, 1, -1, false);
                }

            yield return null;
        }
        else if (_newDay)
        {
            if (Global.day > 1)
            {
                int minMission = int.MaxValue;
                foreach (var item in ManagerData._instance._missionData)
                {
                    if (item.Value.day == Global.day)
                    {
                        if (minMission > item.Key)
                            minMission = item.Key;
                    }
                }
                //ManagerData._instance._missionData[minMission].state = TypeMissionState.Active;
                //PlaySceneWakeUp(ManagerArea._instance._areaStep[0].area, 16,-1,false);

                MissionData mission = ManagerData._instance._missionData[minMission];
                mission.state = TypeMissionState.Active;

                // 미션 아이콘 미리 읽어두기
                UIImageLoader.Instance.Load(Global.gameImageDirectory, "IconMission/", "m_" + minMission, this);
                PlayerPrefs.SetInt("missionCheck" + minMission, 1);   // 다이어리 미션 아이콘 연출관련

                PlaySceneWakeUp(ManagerArea._instance._areaStep[mission.sceneArea - 1].area, mission.sceneIndex, -1, false);
                //Debug.Log("하루 시작  " + minMission + "  " + mission.sceneIndex);
            }


            yield return null;
        }
        else if (_tutorialDiaryMission_Play)
        {
            _tutorialDiaryMission_Play = false;
            ManagerTutorial.PlayTutorial(TutorialType.TutorialDiaryMission);
        }
        else if (_tutorialQuestComplete_Play)
        {
            _tutorialQuestComplete_Play = false;
            ManagerTutorial.PlayTutorial(TutorialType.TutorialQuestComplete);
        }

        //모으기 이벤트 상태일 때, 목적 달성하면 띄우기.
        else if (Global.specialEventIndex > 0 && PlayerPrefs.HasKey("ShowSpeicalEventPopup") )
        {
            //첫 진입 시 남아있는 키는 제거.
            if (_firstLobby)
            {
                PlayerPrefs.DeleteKey("ShowSpeicalEventPopup");
            }
            else
            {
                reservedActions.Add( new KeyValuePair<StartPostProcTypes, int>(StartPostProcTypes.SpecialEventPopup, (int)Global.specialEventIndex) );
            }
        }
        else
        {


        }



        yield return null;



        yield return null;

        // newDay로 한번 씬을 시작하고 나면 기존으로 돌리기
        _newDay = false;
        _loadComplete = true;
        _firstLobby = false;
        if (_stageClear)
            _stageTryCount = 0;
        _stageClear = false;
        _eventStageClear = false;
        _eventStageFail = false;

        StartCoroutine(CoStartPostProcess(reservedActions));
    }

    enum StartPostProcTypes
    {
        SpecialEventPopup,
        MissionDiary,
        StageAction,
        RequestCloverSmall,
        Ranking,
        HelloBehavior,
        UpdateStageBehavior,
    }

    private IEnumerator CoStartPostProcess(List< KeyValuePair<StartPostProcTypes, int> > reservedList)
    {
        for(int i = 0; i < reservedList.Count; ++i)
        {
            if (SceneManager.GetActiveScene().name != "Lobby")
                yield break;
            var popupType = reservedList[i].Key;
            switch (popupType)
            {
                case StartPostProcTypes.SpecialEventPopup:
                    {
                        ManagerUI._instance.OpenPopupSpecialEvent(reservedList[i].Value, true);
                        yield return null;
                        while (UIPopupStage._instance != null)
                            yield return null;
                    }
                    break;
                case StartPostProcTypes.MissionDiary:
                    {
                        ManagerUI._instance.OpenPopupDiary((TypePopupDiary)(reservedList[i].Value));
                        yield return null;
                        while (UIPopupDiary._instance != null)
                            yield return null;

                    }
                    break;
                case StartPostProcTypes.StageAction:
                    {
                        ManagerUI._instance.OpenPopupStageAction(reservedList[i].Value == 1);
                        yield return null;
                        while (UIPopupStage._instance != null)
                            yield return null;

                    }
                    break;
                case StartPostProcTypes.RequestCloverSmall:
                    {
                        ManagerUI._instance.OpenPopupRequestSmall();
                        yield return null;
                        while (ManagerUI._instance._OpenPopupRequestSmallWorkEnd == 0)
                            yield return null;

                        if (ManagerUI._instance._OpenPopupRequestSmallWorkEnd == 1)
                        {
                            while (UIPopupRequestCloverSmall._instance != null)
                                yield return null;
                        }
                        else
                        {
                            if (PlayerPrefs.GetInt("OpenPopupRaking", 0) != reservedList[i].Value)
                            {
                                PlayerPrefs.SetInt("OpenPopupRaking", reservedList[i].Value);
                                ManagerUI._instance.OpenPopupRaking();
                                yield return null;
                                while (!ManagerUI._instance._OpenPopupRankingWorkEnd)
                                    yield return null;
                                while (UIPopupRanking._instance != null)
                                    yield return null;
                            }
                        }

                    }
                    break;
                case StartPostProcTypes.Ranking:
                    {
                        ManagerUI._instance.OpenPopupRaking();
                        yield return null;
                        while (!ManagerUI._instance._OpenPopupRankingWorkEnd)
                            yield return null;
                        while (UIPopupRanking._instance != null)
                            yield return null;
                    }
                    break;
                case StartPostProcTypes.HelloBehavior:
                    {
                        LobbyBehavior._instance.PlayHelloBehavior();
                        
                    }
                    break;
                case StartPostProcTypes.UpdateStageBehavior:
                    {
                        LobbyBehavior._instance.PlayUpdateStageBehavior(reservedList[i].Value);
                    }
                    break;
            }
        }

    }

	// Update is called once per frame
	void Update () {

//
        //if (Input.GetKeyDown(KeyCode.A))
            //ManagerTutorial.PlayTutorial(TutorialType.TutorialLobbyMission);

        
      /*  if (Input.GetKeyDown(KeyCode.A))
        {
            ManagerCinemaBox._instance.OnBox(1f);
        }*/
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
          /*  if (Input.GetKeyDown(KeyCode.F1))
            {
                PlayTriggerWakeUp("Extend_pokoura");
            }*/
        }

        // 대기 상태이거나 팝업창이 없을때
        if (_state == TypeLobbyState.Wait)
        {
            bool touchUI = false;
            if (ManagerUI._instance != null)
                if (UICamera.selectedObject != null)
                    if (UICamera.selectedObject != ManagerUI._instance.gameObject)
                        touchUI = true;


            if (Global._touchTap && !touchUI)
            {
                Ray ray = CameraController._instance.moveCamera.ScreenPointToRay(Global._touchPos);
                RaycastHit hitInfo;
                bool skipWalk = false;
                
                if (Physics.Raycast(ray, out hitInfo, 400, Global.eventObjectMask))
                {
                    ObjectIcon objIcon = hitInfo.collider.transform.GetComponent<ObjectIcon>();
                    if (objIcon != null)
                    {
                        skipWalk = true;
                        objIcon.OnTap();
                    }
                    else
                    {
                        Character objCharacter = hitInfo.collider.GetComponent<Character>();
                        if (objCharacter != null)
                        {
                            skipWalk = true;
                            objCharacter.OnTap();
                        }
                        else
                        {
                            ObjectEvent objEvent = hitInfo.collider.transform.parent.GetComponent<ObjectEvent>();
                            if (objEvent != null)
                            {
                                if (objEvent._touch)
                                {
                                    skipWalk = true;
                                    objEvent.OnTap();
                                }
                            }
                            ObjectMaterial objMaterial = hitInfo.collider.transform.parent.GetComponent<ObjectMaterial>();
                            if (objMaterial != null)
                            {
                                skipWalk = true;
                                objMaterial.OnTap();
                            }
                        }
                    }
                }
                if (!skipWalk)
                {
                   
                    ManagerSound.AudioPlay(AudioLobby.Button_01);

                    if (Random.value >= 0.5f)
                        ManagerSound.AudioPlay(AudioLobby.m_boni_haa);
                    else
                        ManagerSound.AudioPlay(AudioLobby.m_boni_hoa);

                    LobbyBehavior._instance.CommandWalk();
                }
            }
            //    Debug.Log(UICamera.selectedObject);
        }


    /*    if (Global._touchBegin)
        {
            Vector3 target = CameraController._instance.GetWorldPosFromScreen(Global._touchPos);
            Character._boni.StartPath(target);
            _commandUI.transform.position = target;
            _commandUI.skeleton.SetSkin("click_walk");
            _commandUI.skeleton.SetSlotsToSetupPose();

            _commandUI.state.SetAnimation(0,"click_walk_start" , false);
            _commandUI.state.AddAnimation(0,"click_walk_loop",true,0.5f);
        }


        if (Input.GetKey(KeyCode.Space))
        {
            Time.timeScale = 0f;
            Global.timeScaleLobby = 0f;
        }
        else
        {
            Time.timeScale = 1f;
            Global.timeScaleLobby = 1f;
        }
        */
        //CameraController.Get
	}

    void ResetShader(Transform in_transform)
    {
        for (int i = 0; i < in_transform.childCount; i++)
        {
            GameObject t = in_transform.GetChild(i).gameObject;
            if (t.GetComponent<Renderer>() != null)
            {
               // if (t.GetComponent<Renderer>().sharedMaterial != null)
                {
                    for (int m = 0; m < t.GetComponent<Renderer>().sharedMaterials.Length; m++)
                    {
                        if (t.GetComponent<Renderer>().sharedMaterials[m] != null)
                        {
                         //   int renderQueue = t.GetComponent<Renderer>().materials[m].renderQueue;

                      //      if (t.GetComponent<Renderer>().sharedMaterials[m].renderQueue == 3000)

                       //     Debug.Log("   " + t.GetComponent<Renderer>().sharedMaterials[m].shader.name + "   " + t.GetComponent<Renderer>().materials[m].renderQueue + "/" + t.GetComponent<Renderer>().sharedMaterials[m].renderQueue);



                            if (t.GetComponent<Renderer>().sharedMaterials[m].shader.name.Contains("Spine"))
                            {
                                t.GetComponent<Renderer>().sharedMaterials[m].shader = Shader.Find(t.GetComponent<Renderer>().sharedMaterials[m].shader.name);
                            }
                            else
                            {
                                int renderQueue = t.GetComponent<Renderer>().sharedMaterials[m].renderQueue;
                                t.GetComponent<Renderer>().materials[m].shader = Shader.Find(t.GetComponent<Renderer>().materials[m].shader.name);
                                Debug.Log(" renderQueue " + renderQueue + "  " + t.GetComponent<Renderer>().materials[m].shader.name);
                              //  if (renderQueue != 3000)//t.GetComponent<Renderer>().sharedMaterials[m].renderQueue)
                              //   
                              //  t.GetComponent<Renderer>().materials[m].renderQueue = renderQueue;

                            }
                        }
                    }
                }
              /*  else
                {
                    for (int m = 0; m < t.GetComponent<Renderer>().materials.Length; m++)
                        t.GetComponent<Renderer>().materials[m].shader = Shader.Find(t.GetComponent<Renderer>().materials[m].shader.name);
                }*/
            }
            ResetShader(in_transform.GetChild(i));

        }
    /*    foreach (Transform t in in_transform.GetComponentsInChildren<Transform>())
        {
            if (t.gameObject.GetComponent<Renderer>() != null)
                t.gameObject.GetComponent<Renderer>().material.shader = Shader.Find(t.gameObject.GetComponent<Renderer>().material.shader.name);
            ResetShader(t);
        }*/

    }
    GameObject NewObject(Object obj)
    {
        try {
            GameObject tmp = Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;
            tmp.name = obj.name;

            tmp.transform.parent = ManagerArea._instance.transform;
            return tmp;
        }
        catch (System.Exception e) {
            //Debug.LogError("Lobby NewObject Exception: " + e);
        }

        return null;
#if UNITY_EDITOR
       // ResetShader(tmp.transform);
#endif
    }

    static int oldStage = -1;
    IEnumerator CoClearStage()
    {
        yield return new WaitForSeconds(0.8f);

        ManagerUI._instance._labelStage[0].text = ManagerData._instance.userData.stage.ToString();
        ManagerUI._instance._labelStage[1].text = ManagerData._instance.userData.stage.ToString();

        ManagerUI._instance._labelStage[0].cachedTransform.localScale = Vector3.one * 1.5f;
        ManagerUI._instance._labelStage[0].cachedTransform.DOScale(1f, 0.5f).SetEase(Ease.OutSine);
        yield return null;
    }
    public void NewMakePokoyura()
    {
        ManagerSound.AudioPlay(AudioLobby.Mission_Finish);

        Pokoyura poko = Instantiate<Pokoyura>(_objPokoyura);
        poko.transform.position = _spawnPokogoroPosition[ManagerData._instance._pokoyuraData.Count].position;
        poko._index = _activeGetPokoyura;
        poko._newMakeShow = true;

        if( _objPokogoro != null )
        {
            _objPokogoro.AttachPokoyura(poko);
        }
        

        PokoyuraData.SetUserData();
    }
    public void ReMakePokoyura()
    {
        while (Pokoyura._pokoyuraList.Count > 0)
            DestroyImmediate(Pokoyura._pokoyuraList[0].gameObject);

        for (int i = 0; i < ManagerData._instance._pokoyuraData.Count; i++)
        {
            if (_spawnPokogoroPosition.Length > i)
            {
                Pokoyura poko = Instantiate<Pokoyura>(_objPokoyura);
                poko.transform.position = _spawnPokogoroPosition[i].position;
                poko._index = ManagerData._instance._pokoyuraData[i].index;

                if (_objPokogoro != null)
                {
                    _objPokogoro.AttachPokoyura(poko);
                }
            }
        }
    }
    public void ReMakeGiftbox()
    {

    }
    public void ReMakeMaterial()
    {
        while (ObjectMaterial._materialList.Count > 0)
            DestroyImmediate(ObjectMaterial._materialList[0].gameObject);



        foreach (var item in ManagerData._instance._materialSpawnData)
        {
            if (item.materialCount > 0)
            {
                if (PlayerPrefs.HasKey("Material" + item.index))
                {
                    ObjectMaterial material = Instantiate<ObjectMaterial>(_objMaterial);
                    material._data = item;
                    int index = PlayerPrefs.GetInt("Material" + item.index);
                    _spawnMaterialPosition[index].used = true;
                    material.transform.position = _spawnMaterialPosition[index].position;
                }
            }
        }
        foreach (var item in ManagerData._instance._materialSpawnData)
        {
            if (item.materialCount > 0)
            {
                if (!PlayerPrefs.HasKey("Material" + item.index))
                {
                    ObjectMaterial material = Instantiate<ObjectMaterial>(_objMaterial);
                    material._data = item;
                    int count = 0;
                    int index = Random.Range(0, _spawnMaterialPosition.Length - 1);
                    while (true)
                    {

                        if (_spawnMaterialPosition[index].used == false)
                        {
                            PlayerPrefs.SetInt("Material" + item.index, index);
                            _spawnMaterialPosition[index].used = true;
                            material.transform.position = _spawnMaterialPosition[index].position;
                            break;
                        }

                        index++;
                        if (index >= _spawnMaterialPosition.Length)
                            index = 0;

                        count++;
                        if (count > _spawnMaterialPosition.Length)
                        {
                            material.transform.position = _spawnMaterialPosition[0].position;
                            break;

                        }
                    }
                }
            }
            
        }
    }
    public void PlayTriggerWakeUp(string in_triggerName)
    {
        TriggerScene trigger = null;
        if (ManagerArea._instance._extendTrigger.TryGetValue(in_triggerName, out trigger))
        {
            LobbyBehavior._instance.ResetSelectBehavior();
            LobbyBehavior._instance.CancleBehavior();

            ManagerUI._instance.CoShowUI(0.2f, false, TypeShowUI.eAll);
            ManagerCinemaBox._instance.OnBox(1f, true);

            trigger._triggerWait.gameObject.SetActive(false);
            trigger._triggerWakeUp.gameObject.SetActive(true);
            trigger._triggerWakeUp.StartCondition();

            ManagerSound._instance.SetTimeBGM(96f);

            _state = TypeLobbyState.TriggerEvent;
        }
    }

    public void PlayScene_FromEditor<T>(T in_area, int in_scene, int in_missionIndex = -1, bool in_startSound = true) where T : AreaBase
    {
        StartCoroutine(CoPlayScene_FromEditor(in_area, in_scene, in_missionIndex, in_startSound));
    }

    public IEnumerator CoPlayScene_FromEditor<T>(T in_area, int in_scene, int in_missionIndex = -1, bool in_startSound = true) where T: AreaBase
    {
        ManagerCharacter._instance.AddLoadList(in_area._characters, in_area._live2dChars);

        yield return ManagerCharacter._instance.LoadCharacters();

        PlaySceneWakeUp(in_area, in_scene, in_missionIndex, in_startSound);
    }

    
   
    public void PlaySceneWakeUp<T>(T in_area, int in_scene, int in_missionIndex = -1, bool in_startSound = true) where T : AreaBase
    {
        LobbyBehavior._instance.ResetSelectBehavior();
        LobbyBehavior._instance.CancleBehavior();

        if (in_area._listSceneDatas.Count >= in_scene && in_scene > 0)
        {
            if (in_area._listSceneDatas[in_scene - 1].sceneData._triggerWakeUp.gameObject.active == false)
            {
                ManagerUI._instance.CoShowUI(0.2f, false, TypeShowUI.eAll);
                ManagerCinemaBox._instance.OnBox(1f, in_startSound);
                in_area._listSceneDatas[in_scene - 1].sceneData._triggerWait.gameObject.SetActive(false);
                in_area._listSceneDatas[in_scene - 1].sceneData._triggerWakeUp.gameObject.SetActive(true);
                in_area._listSceneDatas[in_scene - 1].sceneData._triggerWakeUp.StartCondition();

                if( !in_area.IsEventArea() )
                {
                    if (in_scene != 1)
                        ManagerSound._instance.SetTimeBGM(96f);
                        //ManagerSound._instance.PauseBGM();
                }
            }
        }
        lateClearMission = in_missionIndex;
        _state = TypeLobbyState.TriggerEvent;
    }

    public void ChageState(TypeLobbyState in_state)
    {
        if (_state == TypeLobbyState.TriggerEvent && in_state == TypeLobbyState.Wait)
        {
            _state = in_state;
            CharacterBoni._boni.StopPath();
            AIChangeCommand command = new AIChangeCommand();
            command._state = AIStateID.eIdle;
            CharacterBoni._boni._ai.ChangeState(command);

            ManagerCinemaBox._instance.OffBox(2f);

            ManagerUI._instance.CoShowUI(0.5f, true, TypeShowUI.eAll);

            ManagerSound._instance.UnPauseBGM();


            CameraController._instance._cameraTarget.transform.position = CameraController._instance.GetCenterWorldPos();
            CameraController._instance._cameraTarget.velocity = Vector3.zero;


            // 투토리얼
            {
                if (Global.join)
                    if(_tutorialLobbyMission_Play)
                    {
                        _tutorialLobbyMission_Play = false;
                        ManagerTutorial.PlayTutorial(TutorialType.TutorialLobbyMission);
                    }

                if (lateClearMission == 7)
                    ManagerTutorial.PlayTutorial(TutorialType.TutorialGetPlusHousing);



            }
            
            
        }
    }
    public void EndDay()
    {
        _newDay = true;
        ManagerCinemaBox._instance.OffBox(0.1f);
        ManagerUI._instance.CoShowUI(0.1f, false, TypeShowUI.eAll);
        ManagerSound._instance.StopBGM();
        SceneLoading.MakeSceneLoading("NewDay");
    }
    public Character GetCharacter(TypeCharacterType in_Type)
    {
        if (in_Type == TypeCharacterType.None)
            return null;
        // null 이면 만든다
        if (!_characterList.ContainsKey((int)in_Type))
        {
            return MakeCharacter(in_Type, Vector3.zero);
        }
        else
            return _characterList[(int)in_Type];

       /* if (in_Type == TypeCharacterType.Boni)
        {
            return CharacterBoni._boni;
        }
        */

        return null;
    }
    public void RemoveCharacter(TypeCharacterType in_Type)
    {
        if (_characterList.ContainsKey((int)in_Type))
        {
            Destroy(_characterList[(int)in_Type].gameObject);
            _characterList.Remove((int)in_Type);
        }
        
    }
    public Character MakeCharacter(TypeCharacterType in_Type, Vector3 in_Pos)//, Vector3 vecPos, bool bForcePos = false, int nModelIndex = 1000, CHAT_SIDE Char_Direction = CHAT_SIDE.LEFT, float fForcePosDist = 5.0f, float fTransparentTime = 0.5f)
    {
        Character character = null;
        
        var charData = ManagerCharacter._instance.GetCharacter((int)in_Type, 0);
        character = Instantiate(charData.obj).GetComponent<Character>();
        character.tapSound = charData.tapSound;

        character.SetFallbackSound();

        if(character != null)
            character._transform.position = in_Pos;
        _characterList.Add((int)in_Type,  character);

        return character;
    }

    public Character MakeCharacter(TypeCharacterType in_Type, int costumeId, Vector3 in_Pos)
    {
        Character character = null;
        var charData = ManagerCharacter._instance.GetCharacter((int)in_Type, costumeId);

        if( charData == null )
        {   // 테스트중에 리소스 없는 코스튬 실수로 선택한 경우 로그인 안되는 사태를 방지하기 위해
            charData = ManagerCharacter._instance.GetCharacter((int)in_Type, 0);
        }
        character = Instantiate(charData.obj).GetComponent<Character>();
        character.tapSound = charData.tapSound;

        character.SetFallbackSound();

        if (character != null)
            character._transform.position = in_Pos;
        _characterList.Add((int)in_Type, character);

        return character;
    }

    public void SetCostume(int costumeId)
    {
        StartCoroutine(CoChangeCostume(costumeId));
    }

    IEnumerator CoChangeCostume(int costumeId)
    {
        if(ManagerCharacter._instance.GetCharacter((int)TypeCharacterType.Boni, costumeId) == null)
        {
            yield return CoLoadCostume(costumeId);
        }

        var charImportInfo = ManagerCharacter._instance.GetCharacter((int)TypeCharacterType.Boni, costumeId);
        var c = Instantiate(charImportInfo.obj).GetComponent<Character>();
        var orgChar = GetCharacter(TypeCharacterType.Boni);
        RemoveCharacter(TypeCharacterType.Boni);
        c.transform.position = orgChar.transform.position;
        _characterList[(int)TypeCharacterType.Boni] = c;

        yield return new WaitForSeconds(0.1f);
        c._ai.PlayAnimation(false, "haughty", WrapMode.Loop, 0f, 1f);
    }

    IEnumerator CoLoadCostume(int costumeId)
    {
        if (!Global._instance.ForceLoadBundle && (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer))
        {
#if UNITY_EDITOR
            ManagerCharacter._instance.LoadFromInternal(TypeCharacterType.Boni, costumeId);
#endif
        }
        else
        {
            if(ManagerCharacter._instance.GetCharacter((int)TypeCharacterType.Boni, costumeId) == null)
            {
                yield return ManagerCharacter._instance.LoadFromBundle(TypeCharacterType.Boni, costumeId);
            }
        }
    }
    
    public void OpenPopupLobbyMission(int in_missionIndex, Vector3 in_positoin)
    {
        ObjectMissionIcon icon = Instantiate(_objMissionIcon).GetComponent<ObjectMissionIcon>();
        icon.InitLobbyMission(in_missionIndex, in_positoin);
    }
    public void OpenHousingIcon(ObjectBase in_obj, ActionObjectHousing in_action, Vector3 in_positoin)
    {
        //ObjectHousingIcon icon = Instantiate(_objHousingIcon).GetComponent<ObjectHousingIcon>();
        //icon.InitHousingIcon(in_obj,in_action, in_positoin + new Vector3(2f,0f,-2f));
        _housingIcon.transform.position = in_positoin + new Vector3(2f,0f,-2f);
        _housingIcon.gameObject.SetActive(true);
        _housingIcon.InitHousingIcon(in_obj, in_action, in_positoin + new Vector3(2f, 0f, -2f));
    }
    #region IImageRequestable
    // 미션 아이콘 미리 받기용 더미
    public void OnLoadComplete(ImageRequestableResult r)
    {
    }

    public void OnLoadFailed() { }

    public int GetWidth()
    {
        return 10;
    }

    public int GetHeight()
    {
        return 10;
    }
    #endregion
    void OnDrawGizmos()
    {

        if (_spawnMaterialPosition != null)
        {
            Gizmos.color = Color.white;

            for (int i = 0; i < _spawnMaterialPosition.Length; i++)
            {
                Gizmos.DrawWireSphere(_spawnMaterialPosition[i].position, 0.3f);
                PokoUtil.drawString("<" + i + ">", _spawnMaterialPosition[i].position, 0f, 0f, Color.white);
            }
            
        }
        if (_spawnGiftBoxPosition != null)
        {
            Gizmos.color = Color.green;

            for (int i = 0; i < _spawnGiftBoxPosition.Length; i++)
            {
                PokoUtil.drawString("<" + i + ">", _spawnGiftBoxPosition[i].position, 0f, 0f, Color.green);
                Gizmos.DrawWireSphere(_spawnGiftBoxPosition[i].position, 0.3f);
            }

        }
        if (_spawnPokogoroPosition != null)
        {
            Gizmos.color = Color.yellow;

            for (int i = 0; i < _spawnPokogoroPosition.Length; i++)
            {
                PokoUtil.drawString("<" + i + ">", _spawnPokogoroPosition[i].position, 0f, 0f, Color.yellow);
                Gizmos.DrawWireSphere(_spawnPokogoroPosition[i].position, 0.3f);
            }

        }
        if (Global._touchBegin)
        {
            //   Debug.Log("ddddd");
        }
    }

#if UNITY_EDITOR
    public static Dictionary<string, string> areaRedirect = new Dictionary<string, string>();

    void ApplyAreaRedirect(ref string prefabPath)
    {
        if (areaRedirect.Count > 0)
        {
            foreach(var r in areaRedirect)
            {
                prefabPath = prefabPath.Replace(r.Key, r.Value);
            }
        }
    }
#endif
}
