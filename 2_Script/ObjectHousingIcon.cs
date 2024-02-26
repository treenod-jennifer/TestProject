using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectHousingIcon : ObjectIcon
{
    public AnimationCurve _curveShow;
    public AnimationCurve _curveTouch;

    public Transform _transformSprite;
    public MeshRenderer _meshRenderer;
    bool _canClick = false;
    bool _skipAutoRemove = false;
    ActionObjectHousing _actionHousing = null;
    float deadTimer = 0f;
    void Awake()
    {
        base.Awake();
    }
	// Use this for initialization
	void Start () {

	}
    override public void OnTap()
    {
        if (_canClick == false)
            return;
        _canClick = false;
        _skipAutoRemove = true;
        ManagerSound.AudioPlay(AudioLobby.Button_01);
        StartCoroutine(DoTouchObjectAnimation());
    }
	// Update is called once per frame
	void Update () {
        //if (_canClick)
        
        _transformSprite.localScale = new Vector3(1f,1f - Mathf.Sin(Time.time * 10f) * 0.05f, 1f);

        if (!_canClick)
            return;

        deadTimer += Global.deltaTimeLobby;
        if (deadTimer > 3f && !_skipAutoRemove)
        {
            CloseLobbyMission();
            _canClick = false;
        }
	}

    public void InitHousingIcon(ObjectBase in_obj, ActionObjectHousing in_action,Vector3 in_position)
    {
        deadTimer = 0f;
        _canClick = false;
        _skipAutoRemove = false;
        _transformSprite.localScale = Vector3.one;
        _transform.position = in_position;
        _actionHousing = in_action;
        //MissionTimeSetting();
        StartCoroutine(CoOpenLobbyMission());
    }

    public void CloseLobbyMission()
    {
        StartCoroutine(CoCloseLobbyMission());
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
         //   ratio = _curveShow.Evaluate(animationTimer);
            _transformSprite.localScale = Vector3.one * animationTimer;
            //_meshRenderer.material.SetColor("_MainColor", new Color(1f, 1f, 1f, animationTimer));
            animationTimer -= Time.deltaTime * 4f;
            yield return null;
        }
        gameObject.SetActive(false);
  
    }

    IEnumerator DoTouchObjectAnimation()
    {
        float animationTimer = 0f;
        float SpeedRatio = 2f;
        float ratio;
        bool sendMessage = false;

        while (animationTimer < 1f)
        {
            ratio = _curveTouch.Evaluate(animationTimer);
            _transformSprite.localScale = Vector3.one * ratio;
            animationTimer += Time.deltaTime * 2.5f;

            if (animationTimer > 0.5f && sendMessage == false)
            {
                ManagerUI._instance.OpenPopupHousing(_actionHousing, -1, false);
                //UIHousing housing = NGUITools.AddChild(ManagerUI._instance.anchorCenter, ManagerUI._instance._objPopupHousing).GetComponent<UIHousing>();
                //housing.InitHousing(_actionHousing, _actionHousing.OnChatComplete);
                sendMessage = true;
            }
            yield return null;
        }

        _transformSprite.localScale = Vector3.one;
        gameObject.SetActive(false);
    //    CloseLobbyMission();
        yield return null;
    }
}
