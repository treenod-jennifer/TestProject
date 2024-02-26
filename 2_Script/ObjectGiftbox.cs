using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using Protocol;

public class ObjectGiftbox : ObjectIcon
{
    public Transform _transformSprite;
    public Image _uiBg;
    public List<Sprite> _spriteList = new List<Sprite>();
    public Image _uiCheck;
    public Image _uiTimeBg;
    public Text _uiTime;

    public Animator animator;

    [System.NonSerialized]
   // public ServerUserGiftBox _data = null;
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
           // ServerAPI.OpenGiftBox((int)_data.index, recvOpenGiftBox);
            _canClick = false;
        }

        else
        {
          //  ManagerUI._instance.OpenPopupTimeGiftBox(_data);
        }

        //Debug.Log("@@@@ 4");
    }

/*
    public void InitGiftBox(ServerUserGiftBox giftBoxData)
    {
        _data = giftBoxData;

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
            StartCoroutine(CoGiftBoxTimer(_data));
        }
        else
        {
            bCanOpenBox = true;
            _uiCheck.gameObject.SetActive(true);
            _uiTime.gameObject.SetActive(false);
            _uiTimeBg.gameObject.SetActive(false);
            animator.Play("giftBox_ani_loop");
        }
    }
    
    private IEnumerator CoGiftBoxTimer(ServerUserGiftBox gData)
    {
        
        while (true)
        {
            if (gameObject.activeInHierarchy == false)
            {
                break;
            }*

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
        }
    }

    void recvOpenGiftBox(OpenGiftBoxResp resp)
    {
        if (resp.IsSuccess)
        {
            if (PlayerPrefs.HasKey("Giftbox" + _data.index))
            {
                int index = PlayerPrefs.GetInt("Giftbox" + _data.index);
                if (index < ManagerLobby._instance._spawnGiftBoxPosition.Length)
                    ManagerLobby._instance._spawnGiftBoxPosition[index].used = false;
                PlayerPrefs.DeleteKey("Giftbox" + _data.index);
            }
            UIPopUpOpenGiftBox popup = ManagerUI._instance.OpenPopupGiftBox();
            popup._data = _data;
            Destroy(gameObject);
            //Debug.Log("** OpenGiftBox ok count:" + resp.userGiftBoxes.Count);
            MaterialData.SetUserData();
            //LocalNotification.RemoveLocalNotification(LOCAL_NOTIFICATION_TYPE.GIFT_BOX, resp.id);
        }
    }
*/
}
