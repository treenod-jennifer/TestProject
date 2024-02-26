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
    bool _housingDummyParent = false;

    private SkeletonAnimation spineModel { get { return _spineModel ?? (_spineModel = _transformModel.GetComponent<SkeletonAnimation>()); } }
    private SkeletonAnimation _spineModel;
    private Material material;
    private Color initColor = Color.white;
    public List<Transform> _transformModelList = null;
    Vector3 initScale = Vector3.one;
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

        if (!in_loop)
        {
            trackEntry.Complete += delegate
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
        if (_housingDummyParent && _transformModel != null)
        {
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
                if (_onTouch != null)
                    _onTouch();

                childObj.OnTap();

                for (int j = 0; j < _callbackTapList.Count; j++)
                {
                    if( _callbackTapList[j].CanExecuteTapAction() )
                        _callbackTapList[j].OnTap(this);
                }
                return;
            }
        }

        if (_onTouch != null)
            _onTouch();

        ManagerSound.AudioPlay(AudioLobby.Button_01);
        StartCoroutine(DoTouchObjectAnimation());

        if (_actionHousing != null && housingIconMaketime < Time.time)
        {
            housingIconMaketime = Time.time + 1;
            ManagerLobby._instance.OpenHousingIcon(this,_actionHousing, _transform.position + _actionHousing._iconOffset);
        }

        for (int i = 0; i < _callbackTapList.Count; i++)
        {
            if (_callbackTapList[i].CanExecuteTapAction())
                _callbackTapList[i].OnTap(this);
        }
    }

    public void PlayAnimation(string in_name, bool in_loop, string in_skin, System.Action onAniComplete = null)
    {
        if (_transformModel != null)
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

                var orgCollider = childObj._transformCollider.GetComponent<BoxCollider>();
                var destCollider = this._transformCollider.GetComponent<BoxCollider>();

                if( destCollider == null )
                {
                    destCollider = _transformCollider.gameObject.AddComponent<BoxCollider>();
                }

                destCollider.center = orgCollider.center;
                destCollider.size = orgCollider.size;

                orgCollider.enabled = false;
                destCollider.enabled = true;
            }
        }

    }
}
