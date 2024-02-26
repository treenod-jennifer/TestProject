using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class ObjectBase : MonoBehaviour {

    [System.NonSerialized]
    public Transform _transform = null;
    public Transform _transformModel = null;
    public Transform _transformCollider = null;
    public List<Collider> _naviEditCollider = new List<Collider>();
    

    public int _addRenderQueue = 0;

    [System.NonSerialized]
    public ActionObjectHousing _actionHousing = null;
    [System.NonSerialized]
    public List<TriggerState> _callbackTapList = new List<TriggerState>();  // 터치 하면 불러줘야할 콜백 트리거들

    bool doActionActive = false;

    static List<Collider> _naviEditBank = new List<Collider>();
    public static void EditNaviCollider()
    {
        for (int i = 0; i < _naviEditBank.Count; i++)
        {
            GraphUpdateObject guo = new GraphUpdateObject(_naviEditBank[i].bounds);
            guo.modifyTag = true;
            guo.setTag = 2;// !_active ? 2 : 1;
            guo.updatePhysics = false;
            AstarPath.active.UpdateGraphs(guo);
        }
    }
    void OnEnable()
    {
        for (int i = 0; i < _naviEditCollider.Count; i++)
            _naviEditBank.Add(_naviEditCollider[i]);
    }
    void OnDisable()
    {
        for (int i = 0; i < _naviEditCollider.Count; i++)
            _naviEditBank.Remove(_naviEditCollider[i]);
    }
    virtual public void SetAlpha(float in_alpha) { }



    public void DoActionShake(bool in_active, float in_speed = 1f, float in_scale =1f)
    {
        if (in_active)
        {
            doActionActive = true;
            StartCoroutine(CoDoActionShake(in_speed, in_scale));
        }
        else
            doActionActive = false;
    }

    IEnumerator CoDoActionShake(float in_speed, float in_scale)
    {
        Vector3 scale = _transformModel.localScale;

        while (doActionActive)
        {
            float ratio = Mathf.Sin(Time.time * 20f * in_speed) * 0.06f * in_scale;

            _transformModel.localScale = new Vector3(scale.x * (1f - ratio),scale.y * (1f + ratio), 1f);

            yield return null;
        }
        _transformModel.localScale = scale;
    }
}
