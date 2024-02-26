using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILobbyMission : MonoBehaviour {
    Transform _tranform;
    public UITexture _textureIcon;
    public UISprite _spriteCheck;
    public UISprite _spriteTimeBoxBg;
    public UILabel _labelTime;
    public Transform _tranformRoot;
    public AnimationCurve _curveShow;


    Vector3 _targetPosition = Vector3.zero;

    bool _canClick = false;
    MissionData _data = null;

    void Awake()
    {
        _tranform = transform;
    }
	// Use this for initialization
	void Start () {
        		
	}

    public void InitLobbyMission(MissionData mData, Vector3 in_position)
    {
        if (mData == null)
            return;
        _textureIcon.mainTexture = mData._icon;
        _tranformRoot.localScale = Vector3.one;
        _data = mData;
        _targetPosition = in_position;
        _labelTime.gameObject.SetActive(false);
        _spriteTimeBoxBg.gameObject.SetActive(false);
        _spriteCheck.gameObject.SetActive(false);

        //MissionTimeSetting();
        StartCoroutine(CoOpenLobbyMission());
    }
    private IEnumerator CoOpenLobbyMission()
    {
        float animationTimer = 0f;
        float ratio;
        while (animationTimer < 1f)
        {
            ratio = _curveShow.Evaluate(animationTimer);
            transform.localScale = Vector3.one * ratio;
            animationTimer += Time.deltaTime * 2.5f;
            yield return null;
        }
        transform.localScale = Vector3.one;
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
            transform.localScale = Vector3.one * ratio;
            animationTimer -= Time.deltaTime * 2.5f;
            yield return null;
        }
        Destroy(gameObject);
    }
    public Vector3 ChangeTouchPosNGUI(Vector3 in_touchPos)
    {
        return new Vector3((in_touchPos.x * Screen.width), in_touchPos.y * Screen.width, 0);
    }
	// Update is called once per frame
	void LateUpdate()
    {
        _tranform.localPosition = ChangeTouchPosNGUI(ManagerUI._instance._camera.ScreenToWorldPoint(CameraController._instance.moveCamera.WorldToScreenPoint(_targetPosition)));
        if (_canClick)
            _tranformRoot.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 5f) *8f);

	}
    public void BtnMission()
    {
        if (_canClick)
        {

        }
    }
}
