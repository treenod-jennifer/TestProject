using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Protocol;

public class ObjectGiftbox : ObjectIcon
{
    public Transform _transformSprite;
    public Image _uiBg;
    public List<Sprite> _spriteList = new List<Sprite>();
    public Image _uiCheck;
    public Image _uiTimeBg;
    public Text _uiTime;

    public Animator animator;

    [SerializeField] private GameObject IconRoot;

    [SerializeField] private GameObject adObject;
    [SerializeField] private GameObject timeObject;

    [System.NonSerialized]
    public ServerUserGiftBox _data = null;
    bool _canClick = true;
    bool bCanOpenBox = false;

    static public List<ObjectGiftbox> _giftboxList = new List<ObjectGiftbox>();

    void Awake()
    {
        base.Awake();

        _giftboxList.Add(this);
    }
	// Use this for initialization
	void Start () {
		
	}

    void OnDestroy()
    {
        _giftboxList.Remove(this);
    }

    override public void OnTap()
    {
        //Debug.Log("@@@@ 1");
        if (_canClick == false)
            return;
        //StartCoroutine(DoTouchObjectAnimation());
        //Debug.Log("@@@@ 2");

        if (bCanOpenBox == true)
        {
            //Debug.Log("@@@@ 3   " + _data._data.index);
            ManagerSound.AudioPlay(AudioLobby.Mission_ButtonClick);
            ServerAPI.OpenGiftBox((int)_data.index, recvOpenGiftBox);
            _canClick = false;
        }

        else
        {
            ManagerUI._instance.OpenPopupTimeGiftBox(_data);
        }

        //Debug.Log("@@@@ 4");
    }

    IEnumerator CoIconChangeAction()
    {
        bool changeValue = false;

        while(IconRoot.activeSelf)
        {

            adObject.SetActive(changeValue);
            timeObject.SetActive(!changeValue);

            changeValue = !changeValue;

            yield return new WaitForSeconds(1f);
        }
    }

    public void InitGiftBox(ServerUserGiftBox giftBoxData)
    {
        _data = giftBoxData;

        StartCoroutine(CoIconChangeAction());

        if (_data.type < _spriteList.Count)
        {
            _uiBg.sprite = _spriteList[_data.type];
            _uiBg.SetNativeSize();
        }
        if (Global.LeftTime(_data.openTimer) > 0)
        {
            bCanOpenBox = false;

            _uiCheck.gameObject.SetActive(false);
            _uiTime.gameObject.SetActive(true);
            _uiTimeBg.gameObject.SetActive(true);
            timerCoroutine = StartCoroutine(CoGiftBoxTimer(_data));
            IconRoot.SetActive(AdManager.ADCheck(AdManager.AdType.AD_3));
        }
        else
        {
            bCanOpenBox = true;
            _uiCheck.gameObject.SetActive(true);
            _uiTime.gameObject.SetActive(false);
            _uiTimeBg.gameObject.SetActive(false);
            animator.Play("giftBox_ani_loop");
            IconRoot.SetActive(false);
        }
    }


    private Coroutine timerCoroutine;

    public void ResetTime(long openTime)
    {
        _data.openTimer = openTime;

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);

        timerCoroutine = StartCoroutine(CoGiftBoxTimer(_data));
    }

    public bool GetCanOpenBox()
    {
        return bCanOpenBox;
    }

    private IEnumerator CoGiftBoxTimer(ServerUserGiftBox gData)
    {
        
        while (true)
        {
         /*   if (gameObject.activeInHierarchy == false)
            {
                break;
            }*/

            long leftTime = Global.LeftTime(gData.openTimer);
            if (leftTime >= 60)
            {
                if (Mathf.Repeat(Time.time,1f)>=0.5f)
                    _uiTime.text = Global.GetTimeText_HHMM(gData.openTimer,true,false);
                else
                    _uiTime.text = Global.GetTimeText_HHMM(gData.openTimer);
            }
            else
            {
                _uiTime.text = Global.GetTimeText_SS(gData.openTimer);
            }
            

            if ((leftTime) <= 0)
            {
                break;
            }
            yield return null;
        }

        //if (gameObject.activeInHierarchy == true)
        {
            bCanOpenBox = true;
            _canClick = true;
            _uiCheck.gameObject.SetActive(true);
            _uiTime.gameObject.SetActive(false);
            _uiTimeBg.gameObject.SetActive(false);
            animator.Play("giftBox_ani_loop");
            IconRoot.SetActive(false);
        }
    }

    void recvOpenGiftBox(OpenGiftBoxResp resp)
    {
        if (resp.IsSuccess)
        {
            ObjectGiftbox.OpenBoxLog(true, this._data.type);

            QuestGameData.SetUserData();
            UIDiaryController._instance.UpdateQuestData(true);

            OpenGiftBox();
        }
    }

    public void OpenGiftBox()
    {
        if (PlayerPrefs.HasKey("Giftbox" + _data.index))
        {
            int index = PlayerPrefs.GetInt("Giftbox" + _data.index);
            if (index < ManagerLobby._instance._spawnGiftBoxPosition.Length)
                ManagerLobby._instance._spawnGiftBoxPosition[index].used = false;
            PlayerPrefs.DeleteKey("Giftbox" + _data.index);
        }

        ManagerUI._instance.OpenPopup<UIPopUpOpenGiftBox>((popup) => popup._data = _data);
        Destroy(gameObject);
        //Debug.Log("** OpenGiftBox ok count:" + resp.userGiftBoxes.Count);
        MaterialData.SetUserData();
        LocalNotification.RemoveLocalNotification(LOCAL_NOTIFICATION_TYPE.GIFT_BOX, _data.index);
    }

    static public void OpenBoxLog(bool adOrCoin, int boxType)
    {
        string anm = adOrCoin ? "AD" : "COIN";
        switch (boxType)
        {
            case 0: anm = "BOX_SMALL_BY_" + anm; break;
            case 1: anm = "BOX_MIDDLE_BY_" + anm; break;
            case 2: anm = "BOX_BIG_BY_" + anm; break;
        }

        var achieve = new ServiceSDK.GrowthyCustomLog_Achievement
                        (
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_TAG.AD,
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_CAT.TIME_REDUCE_BOX,
                            anm,
                            ServiceSDK.GrowthyCustomLog_Achievement.Code_L_ARLT.SUCCESS
                        );
        var doc = Newtonsoft.Json.JsonConvert.SerializeObject(achieve);
        ServiceSDK.ServiceSDKManager.instance.InsertGrowthyCustomLog("ACHIEVEMENT", doc);
    }
}
