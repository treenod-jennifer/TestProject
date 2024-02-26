using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectMissionIcon : ObjectIcon, IImageRequestable
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
            UIImageLoader.Instance.Load(Global.gameImageDirectory, path, fileName, this);
        }
        else
        {
            OnLoadComplete(null);
        }
    }

    #region IImageRequestable
    public void OnLoadComplete(ImageRequestableResult r)
    {
        _uiMission.gameObject.SetActive(true);
        r.texture.wrapMode = TextureWrapMode.Clamp;
        _uiMission.texture = r.texture;
        //_meshRenderer.material.mainTexture = texture;
    }

    public void OnLoadFailed()
    {
        StartCoroutine(RetryLoad());
    }

    float _retryInterval = 1f;
    IEnumerator RetryLoad()
    {
        yield return new WaitForSeconds(_retryInterval);

        // 재시도를 여러번 할수록 최대 10초까지 재시도 간격이 커지게 해서 CDN 부하를 줄인다.
        // 어짜피 재시도하는 상황이 나는 것 자체가 서버 과부하거나, 회선이 구리거나, 파일이 없거나이기 때문에 거기다가 계속 부하를 주는건 별로 좋지않음
        _retryInterval *= 1.5f;
        if (_retryInterval > 10.0f) _retryInterval = 10.0f; 
        string fileName = "m_" + missionIndex;
        IconImageLoad("IconMission/", fileName);
    }

    public int GetWidth()
    {
        return (int)_uiMission.rectTransform.sizeDelta.x;
        //return _meshRenderer.material.mainTexture.width;
    }

    public int GetHeight()
    {
        return (int)_uiMission.rectTransform.sizeDelta.y;
        //return _meshRenderer.material.mainTexture.height;
    }
    #endregion
}
