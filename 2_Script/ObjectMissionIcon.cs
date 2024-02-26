using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectMissionIcon : ObjectIcon
{
    public AnimationCurve _curveShow;
    public AnimationCurve _curveTouch;

    public Transform _transformSprite;
    //public Transform _transformCheck;
   // public MeshRenderer _meshRenderer;

    public Image _uiCheck;
    public Image _uiTimeBg;
    public Text _uiTime;
    public RawImage _uiMission;

    bool _canClick = false;
    int missionIndex = -1;

    private ResourceBox box;
    private ResourceBox Box
    {
        get
        {
            if (box == null)
            {
                box = ResourceBox.Make(gameObject);
            }

            return box;
        }
    }

    public static List<ObjectMissionIcon> _iconList = new List<ObjectMissionIcon>();
    void Awake()
    {
        base.Awake();
        _iconList.Add(this);
    }

	// Use this for initialization
	void Start () {
		
	}
    void OnDestroy()
    {
        _iconList.Remove(this);
    }

    static public void RemoveMissionIcon(int in_missionIndex)
    {
        if (in_missionIndex == -1)
        {
            for (int i = 0; i < _iconList.Count; i++)
                _iconList[i].CloseLobbyMission();
            return;
        }

        for (int i = 0; i < _iconList.Count; i++)
        {
            if (_iconList[i].missionIndex == in_missionIndex)
            {
                _iconList[i].CloseLobbyMission();
                break;
            }
        }
    }

    static public ObjectMissionIcon FindMissionIcon(int in_missionIndex)
    {
        for (int i = 0; i < _iconList.Count; i++)
        {
            if (_iconList[i].missionIndex == in_missionIndex)
            {
                return _iconList[i];
                
            }
        }
        return null;
    }

    override public void OnTap()
    {
        if (_canClick == false)
            return;
        _canClick = false;

        ManagerSound.AudioPlay(AudioLobby.Mission_ButtonClick);
        StartCoroutine(DoTouchObjectAnimation());
    }
	// Update is called once per frame
	void Update () {
        //if (_canClick)
          
        _transformSprite.rotation = Quaternion.Euler(50f, -45f, Mathf.Sin(Time.time * 5f) * 8f);
	}

    public void InitLobbyMission(int in_missionIndex, Vector3 in_position)
    {
        missionIndex = in_missionIndex;
        string fileName = "m_" + in_missionIndex;
        IconImageLoad("IconMission/", fileName);
        _transform.position = in_position;
        _uiCheck.gameObject.SetActive(false);

        MissionData mData = ManagerData._instance._missionData[missionIndex];
        //시간 미션 관련.
        if (mData.waitTime > 0)
        {
            if (mData.clearTime == 0)
            {   
                _uiTime.gameObject.SetActive(false);
                _uiTimeBg.gameObject.SetActive(false);
            }

            else
            {
                if (mData.waitTime >= 60)
                {
                    _uiTime.text = Global.GetTimeText_HHMM(mData.waitTime, false);
                }
                else
                {
                    _uiTime.text = Global.GetTimeText_SS(mData.waitTime, false);
                }
                StartCoroutine(CoMissionTimer(mData));
            }
        }
        else
        {
            _uiTime.gameObject.SetActive(false);
            _uiTimeBg.gameObject.SetActive(false);
        }

        //MissionTimeSetting();
        StartCoroutine(CoOpenLobbyMission());
    }

    public void CloseLobbyMission()
    {
        StartCoroutine(CoCloseLobbyMission());
    }

    private IEnumerator CoMissionTimer(MissionData mData)
    {
        while (true)
        {
            if (gameObject.activeInHierarchy == false)
                break;
            long leftTime = Global.LeftTime(mData.clearTime);

            if (leftTime >= 60)
            {
                if (Mathf.Repeat(Time.time, 1f) >= 0.5f)
                    _uiTime.text = Global.GetTimeText_HHMM(mData.clearTime, true, false);
                else
                    _uiTime.text = Global.GetTimeText_HHMM(mData.clearTime);
            }
            else
            {
                _uiTime.text = Global.GetTimeText_SS(mData.clearTime);
            }

            if ((leftTime) <= 0)
            {
                break;
            }
            yield return null;
        }

        if (gameObject.activeInHierarchy == true)
        {
            _uiTime.gameObject.SetActive(false);
            _uiTimeBg.gameObject.SetActive(false);
            _uiCheck.gameObject.SetActive(true);
            StartCoroutine(DoTouchObjectAnimation(false));
        }
    }

    private IEnumerator CoOpenLobbyMission()
    {
        float animationTimer = 0f;
        float ratio;
        while (animationTimer < 1f)
        {
            ratio = _curveShow.Evaluate(animationTimer);
            _transformSprite.localScale = Vector3.one * ratio;
            animationTimer += Time.deltaTime * 2.5f;
            yield return null;
        }
        _transformSprite.localScale = Vector3.one;
        _canClick = true;
    }
    private IEnumerator CoCloseLobbyMission()
    {
        _canClick = false;
        float animationTimer = 1f;
        float ratio;
        while (animationTimer > 0f)
        {
            ratio = _curveShow.Evaluate(animationTimer);
            _transformSprite.localScale = Vector3.one * ratio;
            animationTimer -= Time.deltaTime * 2.5f;
            yield return null;
        }
        Destroy(gameObject);
    }

    IEnumerator DoTouchObjectAnimation(bool bEndOpenPopup = true)
    {
        float animationTimer = 0f;
        float ratio;
        bool sendMessage = false;

        while (animationTimer < 1f)
        {
            ratio = _curveTouch.Evaluate(animationTimer);
            _transformSprite.localScale = Vector3.one * ratio;
            animationTimer += Time.deltaTime * 2.5f;

            if (animationTimer > 0.5f && sendMessage == false)
            {
                if (bEndOpenPopup == true)
                {
                    ManagerUI._instance.OpenPopupDiaryMission();
                }
                sendMessage = true;
            }

            yield return null;
        }

        _transformSprite.localScale = Vector3.one;
        _canClick = true;
        yield return null;
    }

    void IconImageLoad(string path, string fileName)
    {
        if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(fileName))
        {
            Box.LoadCDN<Texture2D>(Global.gameImageDirectory, path, fileName, OnLoadComplete);
        }
        else
        {
            OnLoadComplete(null);
        }
    }

    public void OnLoadComplete(Texture2D r)
    {
        _uiMission.gameObject.SetActive(true);
        r.wrapMode = TextureWrapMode.Clamp;
        _uiMission.texture = r;
        //_meshRenderer.material.mainTexture = texture;
    }
}
