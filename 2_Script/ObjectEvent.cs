using System.Collections;
using System.Collections.Generic;
using Spine;
using UnityEngine;
using Spine.Unity;

public class ObjectEvent : ObjectBase
{

    private const string ANIM_SPINE_TOUCH = "touch";
    

    public delegate void onTouch();
    public onTouch _onTouch = null;

    public bool _touch = true;
    public bool _passAction = false;
    [SerializeField]
    public bool _housingDummyParent = false;

    private SkeletonAnimation spineModel { get { return _spineModel ?? (_spineModel = _transformModel?.GetComponent<SkeletonAnimation>()); } }
    private SkeletonAnimation _spineModel;
    private Material material;
    private Color initColor = Color.white;
    public List<Transform> _transformModelList = null;
    Vector3 initScale = Vector3.one;

    public Vector3 _iconOffset = Vector3.zero;
    void Awake()
    {
        _transform = transform;
        if (_addRenderQueue != 0 && _transformModel != null)
        {
            _transformModel.GetComponent<MeshRenderer>().material.renderQueue += _addRenderQueue;
        }
        if (_transformModel != null)
        {
            initScale = _transformModel.localScale;
        }
    }
	// Use this for initialization
	void Start () {
        _transform = transform;

        bool disableSpine = true;
        var housingData = GetComponent<HousingData>();
        if( housingData)
        {
            if( housingData.haveDefaultLoopAnimation )
            {
                disableSpine = false;
            }
        }

        if (disableSpine && _transformModel != null)
	    {
            if (spineModel != null)
	            spineModel.enabled = false;
	    }
	}

    private void Update()
    {
        if (spineModel != null && spineModel.state != null)
        {
            if( spineModel.state.GetCurrent(0) != null )
            {
                var currentTimeScale = spineModel.state.GetCurrent(0).TimeScale;
                if (currentTimeScale != Global.timeScaleLobbySpine)
                {
                    //Debug.LogFormat("Set Spine {0} Timescale {1} -> {2}", spineModel.name, currentTimeScale, Global.timeScaleLobby);
                    spineModel.state.GetCurrent(0).TimeScale = Global.timeScaleLobbySpine;
                }
            }
        }
    }

    public override void SetAlpha(float in_alpha) 
    {
        if (_transformModel != null)
        {
            if (material == null)
            {
                material = _transformModel.GetComponent<MeshRenderer>().material;
                if (material.HasProperty("_Color"))
                    initColor = material.GetColor("_Color");

            }
            initColor.a = in_alpha;
            material.SetColor("_Color",initColor);// new Color(1f, 1f, 1f, in_alpha));
        }
    }
    public void OnEnterCharacter(Character in_character)
    {
        //Debug.Log(" OnEnterCharacter ");
        if (_passAction)
        {
            if (_transformModel != null)
            {
                if (spineModel != null)
                {
                    //    spineModel.skeleton.SetSkin("02");
                    //   spineModel.skeleton.SetSlotsToSetupPose();
                    PlaySpineAnimation(ANIM_SPINE_TOUCH,false);
                    //spineModel.state.Update(0.1f);
                    //spineModel.state.AddAnimation(0, "idle", true,0f);
                }
                else
                {
                    StartCoroutine(CoPassAction(in_character));
                }
            }
        }
    }

    private void PlaySpineAnimation(string clipName,bool in_loop, System.Action completeDelegate = null)
    {
        spineModel.enabled = true;

        TrackEntry trackEntry = spineModel.state.SetAnimation(0, clipName, in_loop);
        trackEntry.TimeScale = Global.timeScaleLobbySpine;

        if (!in_loop)
        {
            trackEntry.End += delegate
            {
                spineModel.enabled = false;
            };
            if( completeDelegate != null)
            {
                trackEntry.Complete += delegate
                {
                    completeDelegate();
                };
            }
        }
        
    }
   
    IEnumerator CoPassAction(Character in_character)
    {
        Transform cT = in_character._transform;
        float maxLen = 3f;

        float cAngle = 0f;
        float vAngle = 0f;
        float angle = 0;
        while (true)
        {
            if ( in_character == null)
                break;

            Vector3 v = in_character._transform.position - _transform.position;
            float len = v.magnitude;
            if (maxLen <= len)
            {
                cAngle = Mathf.SmoothDamp(cAngle, 0f, ref vAngle, 0.1f);
                _transformModel.rotation = Quaternion.Euler(50f, -45f, cAngle);

                if (Mathf.Abs(cAngle) < 0.1f)
                    break;
            }
            else
            {
                if (len < 2.5f)
                {
                    if (Vector3.Dot(CameraController.cameraRight, v.normalized) > 0f)
                    {
                        angle = 20f * (2.5f - len);
                        cAngle = Mathf.SmoothDamp(cAngle, angle, ref vAngle, 0.1f);
                        _transformModel.rotation = Quaternion.Euler(50f, -45f, cAngle);
                    }
                    else
                    {
                        angle = -20f * (2.5f - len);
                        cAngle = Mathf.SmoothDamp(cAngle, angle, ref vAngle, 0.1f);
                        _transformModel.rotation = Quaternion.Euler(50f, -45f, cAngle);
                    }
                }
            }
            yield return null;
        }
    }
    float housingIconMaketime = 0f;
    public void OnTap()
    {
        ClickBlocker.Make();

        if (_housingDummyParent && _transformModel != null)
        {
            for (int i = 0; i < _transformModel.childCount; ++i)
            {
                if (_transformModel.GetChild(i).gameObject.activeSelf == false)
                {
                    continue;
                }

                var objList = _transformModel.GetChild(i).GetComponentsInChildren<ObjectEvent>();
                for(int j = 0; j < objList.Length; ++j)
                {
                    ObjectEvent childObj = objList[j];

                    if (childObj == null || childObj.gameObject.activeInHierarchy == false)
                    {
                        continue;
                    }
                    if (_onTouch != null)
                        _onTouch();

                    childObj.OnTap();

                    for (int k = 0; k< _callbackTapList.Count; k++)
                    {
                        if (_callbackTapList[k].CanExecuteTapAction())
                            _callbackTapList[k].OnTap(this);
                    }
                    return;
                }
            }
        }

        if (_onTouch != null)
            _onTouch();

        ManagerSound.AudioPlay(AudioLobby.Button_01);
        StartCoroutine(DoTouchObjectAnimation());

        /* 오브젝트 터치 시, 하우징 선택 기능 제거
        if (housingIdx != 0 && ManagerHousing.IsHousingActivated(housingIdx) && housingIconMaketime < Time.time)
        {
            housingIconMaketime = Time.time + 1;
            //ManagerLobby._instance.OpenHousingIcon(this, housingIdx, _transform.position + _actionHousing._iconOffset);
            ManagerLobby._instance.OpenHousingIcon(this, housingIdx, _transform.position + _iconOffset);
        }*/

        for (int i = 0; i < _callbackTapList.Count; i++)
        {
            if (_callbackTapList[i].CanExecuteTapAction())
                _callbackTapList[i].OnTap(this);
        }
    }

    private Coroutine touchingRoutine = null;
    public void OnTouching()
    {
        if (touchingRoutine != null)
            return;

        //하우징의 하위 오브젝트 중, ObjectEvent가 있다면 Touching 동작 실행
        if (_housingDummyParent && _transformModel != null)
        {
            for (int i = 0; i < _transformModel.childCount; ++i)
            {
                if (_transformModel.GetChild(i).gameObject.activeSelf == false)
                {
                    continue;
                }

                var objList = _transformModel.GetChild(i).GetComponentsInChildren<ObjectEvent>();
                for (int j = 0; j < objList.Length; ++j)
                {
                    ObjectEvent childObj = objList[j];

                    if (childObj == null || childObj.gameObject.activeInHierarchy == false)
                    {
                        continue;
                    }

                    childObj.OnTouching();
                    return;
                }
            }
        }

        if (housingIdx != 0 && ManagerHousing.IsHousingActivated(housingIdx))
        {
            ManagerSound.AudioPlay(AudioLobby.Button_01);
            StartCoroutine(DoTouchObjectAnimation());
            touchingRoutine = StartCoroutine(CoTouching());
        }
    }

    private enum HousingChangeUIState
    {
        NONE = 0,       //아무것도 아닌 상태
        TOUCH_WAIT,     //터치 후 UI출력 대기
        APPEAR_ARROW,   //화살표 UI출력된 상태
        COMPLETE,       //하우징 UI출력 가능상태
    }

    private IEnumerator CoTouching()
    {
        float uiAppearTime = 0.1f;
        float arrowTime = 0.5f;
        float dragDistance = 30f;

        float timer = 0f;
        HousingChangeUIState uiState = HousingChangeUIState.TOUCH_WAIT;
        UIHousingGauge housingGauge = null;
        while (true)
        {
            //터치가 끝난 상태이거나, 일정 범위 이상을 드래그 했다면 터치 취소 상태로 전환
            if (Global._touchEnd == true || Global._touchDeltaPos.sqrMagnitude > dragDistance)
            {
                uiState = HousingChangeUIState.NONE;
                if (housingGauge != null)
                {
                    housingGauge.HideHousingGauge();
                }
                touchingRoutine = null;
                yield break;
            }

            timer += Time.deltaTime;
            switch (uiState)
            {
                case HousingChangeUIState.TOUCH_WAIT:
                    {
                        if (timer >= uiAppearTime)
                        {
                            uiState = HousingChangeUIState.APPEAR_ARROW;
                            housingGauge = ManagerUI._instance.MakeHousingGaugeUI();
                            housingGauge.transform.localPosition = Vector3.zero;
                            if (housingGauge != null)
                                housingGauge.InitHousingGauge(arrowTime);
                        }
                    }
                    break;
                case HousingChangeUIState.APPEAR_ARROW:
                    {
                        if (timer >= uiAppearTime + arrowTime)
                        {
                            StartCoroutine(DoTouchObjectAnimation());
                            ManagerSound.AudioPlay(AudioLobby.Button_01);
                            uiState = HousingChangeUIState.COMPLETE;
                        }
                    }
                    break;
                case HousingChangeUIState.COMPLETE:
                    {
                        ManagerUI._instance.OpenPopupHousing(housingIdx, -1, false, GetFocusPosition());
                        if (housingGauge != null)
                        {
                            housingGauge.HideHousingGauge();
                        }
                        touchingRoutine = null;
                        yield break;
                    }
            }
            yield return null;
        }
    }

    public void PlayAnimation(string in_name, bool in_loop, string in_skin, System.Action onAniComplete = null)
    {
        if (this.gameObject.activeSelf && _transformModel != null && spineModel != null)
        {
            spineModel.state.ClearTracks();

            if(in_skin != null && in_skin.Length > 0)
            {
                spineModel.skeleton.SetSkin(in_skin);
                spineModel.skeleton.SetSlotsToSetupPose();
                PlaySpineAnimation(in_name, in_loop, onAniComplete);
               // spineModel.state.SetAnimation(0, in_name, in_loop);
                spineModel.state.Apply(spineModel.skeleton);
            }
            else
                PlaySpineAnimation(in_name, in_loop, onAniComplete);
                //spineModel.state.SetAnimation(0, in_name, in_loop);
        }
    }
    IEnumerator DoTouchObjectAnimation()
    {
        float animationTimer = 0f;
        float SpeedRatio = 3f;
        float ratio;
        while (animationTimer < 1f)
        {
            ratio = ManagerLobby._instance.objectTouchAnimationCurve.Evaluate(animationTimer);
            Vector3 scale = initScale;
            scale.x *= (ratio - 1f) + 1f;
            scale.y *= (ratio - 1f) + 1f;
            scale.z *= (ratio - 1f) + 1f;
            _transformModel.localScale = scale;
            animationTimer += Time.deltaTime * SpeedRatio;
            yield return null;
        }

        _transformModel.localScale = initScale;
        yield return null;
    }

    public void OnPostChangedHousing()
    {
        // 더미 하우징인 경우, 실제 하우징에서 충돌체를 받아와서 세팅하고 실제 하우징의 충돌체를 꺼야한다
        if (_housingDummyParent && _transformModel != null)
        {
            // 기존에 네비메시에 영향주던 부분 삭제
            bool active = false;
            for (int i = 0; i < this._naviEditCollider.Count; i++)
            {
                Pathfinding.GraphUpdateObject guo = new Pathfinding.GraphUpdateObject(_naviEditCollider[i].bounds);
                guo.modifyTag = true;
                guo.setTag = !active ? 1 : 2;
                guo.updatePhysics = false;
                AstarPath.active.UpdateGraphs(guo);
            }
            _naviEditCollider.Clear();


            // 충돌체 승계가 필요한, 선택된 하우징 찾는 과정.
            ObjectEvent selectedObj = null;
            for (int i = 0; i < _transformModel.childCount; ++i)
            {
                if (_transformModel.GetChild(i).gameObject.activeSelf == false)
                {
                    continue;
                }

                ObjectEvent childObj = _transformModel.GetChild(i).GetComponent<ObjectEvent>();
                if (childObj == null)
                {
                    continue;
                }

                if( childObj._transformCollider == null || this._transformCollider == null)
                {
                    continue;
                }

                selectedObj = childObj;
            }

            if( selectedObj != null )
            {
                // 세팅돼있던 기존 충돌체 삭제
                var destColliders = this._transformCollider.GetComponents<BoxCollider>();
                foreach (var col in destColliders)
                {
                    Destroy(col);
                }

                // 충돌체 승계
                var orgColliders = selectedObj._transformCollider.GetComponents<BoxCollider>();
                foreach (var col in orgColliders)
                {
                    var destCollider = _transformCollider.gameObject.AddComponent<BoxCollider>();

                    destCollider.center = col.center;
                    destCollider.size = col.size;

                    col.enabled = false;
                    destCollider.enabled = true;

                    // 네비메쉬에 영향줘야되는 경우 naviEditCollider 에 추가해줘야됨
                    for (int i = 0; i < selectedObj._naviEditCollider.Count; ++i)
                    {
                        if( selectedObj._naviEditCollider[i] == col )
                        {
                            this._naviEditCollider.Add(destCollider);
                            break;
                        }
                    }
                }
                active = true;

                // 네비메쉬 업데이트
                for (int j = 0; j < this._naviEditCollider.Count; j++)
                {
                    Pathfinding.GraphUpdateObject guo = new Pathfinding.GraphUpdateObject(_naviEditCollider[j].bounds);
                    guo.modifyTag = true;
                    guo.setTag = !active ? 1: 2;
                    guo.updatePhysics = false;
                    AstarPath.active.UpdateGraphs(guo);
                }
            }
        }
    }

    override public Vector3 GetFocusPosition()
    {
        var pos = transform.position;
        if (housingIdx == 100)
        {
            pos = new Vector3(transform.position.x - 13f, 0, transform.position.z + 13f);
        }
        return pos + _iconOffset;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if(_iconOffset.sqrMagnitude > 0f)
            Gizmos.DrawWireCube(transform.position + _iconOffset, new Vector3(0.4f, 0.6f, 0.4f));
    }
}
