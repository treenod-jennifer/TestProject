using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionObjectHousing : ActionBase
{
    public int _housingIndex = 0;
    public ObjectBase _waitObject = null;
    public List<ObjectBase> _finishObject = new List<ObjectBase>();

    public Dictionary<int, ObjectBase> _objectList = new Dictionary<int, ObjectBase>();    

    [System.Obsolete("Deprecated")]
    [Header("iconOffset은 더이상 사용되지 않음: ObjectEvent 쪽의 iconOffset")]
    public Vector3 _iconOffset = Vector3.zero;

    public Vector3 _firstGetBoniPosOffset = Vector3.zero;
    public TypeCharacterDir _firstGetBonidirection = TypeCharacterDir.Right;
    public List<string> _firstGetBoniTextList = new List<string>();

    public Dictionary<int, List<string>> _firstGetTextSpecial = new Dictionary<int, List<string>>();

    public ObjectEvent _parentObj = null;

    [SerializeField] public bool openPopupAtWakeup = true;

    public Vector3 housingScale = Vector3.one;

    [System.NonSerialized] public ObjectBase currentObject = null;

    public void Awake()
    {
        for (int i = 0; i < _finishObject.Count; i++)
        {
            AddObjectToList(i + 1, _finishObject[i]);
        }


        if (_finishObject.Count > 0 && _waitObject != null)
        {
            _waitObject.housingIdx = _housingIndex;
            currentObject = _waitObject;
        }

        var area = ScanNearestAreaBase(this.transform);
        if( area != null)
        {
            var housingRoot = area.transform.Find("GroupHousing");
            if( housingRoot == null )
            {
                var hrgo = new GameObject("GroupHousing");
                hrgo.transform.parent = area.transform;
                housingRoot = hrgo.transform;
            }
            var copiedObjectRoot = new GameObject($"Deco_S_Obj_{_housingIndex}_({_stateType.ToString()})");
            var comp = copiedObjectRoot.AddComponent<HousingObject_Single>();
            comp.CloneFromLegacy(this);
            copiedObjectRoot.transform.parent = housingRoot;
            copiedObjectRoot.transform.position = this.transform.position;
            copiedObjectRoot.transform.localScale = this.housingScale;

            //ManagerHousing.RegisterHousing(comp);
        }
        
    }
   
    void AddObjectToList(int idx, ObjectBase model)
    {
        _objectList.Add(idx, model);
        var model_asObjectEvent = model as ObjectEvent;
        if (model_asObjectEvent != null)
        {
            if(model_asObjectEvent._iconOffset.sqrMagnitude == 0)
            {
                model_asObjectEvent._iconOffset = this._iconOffset;
            }
        }
    }


    public override void DoAction()
    {
        if( _stateType == TypeTriggerState.WakeUp )
        {
            var housing = ManagerHousing.FindHousing(_housingIndex);
            housing.SelectModel(1);

            if (this.openPopupAtWakeup)
            {
                if (ManagerCinemaBox._instance != null)
                    ManagerCinemaBox._instance.SkipEmergencyStop();

                ManagerUI._instance.OpenPopupHousing(this._housingIndex, -1, false, housing.GetHousingFocusPosition(), OnChatComplete);
                bWaitActionFinishOnOff = true;
            }
        }
    }
    
    public void OnChatComplete()
    {
        bActionFinish = true;
    }

    void OnDrawGizmosSelected()
    {
        //if (_finishObject.Count > 0)
        //{
        //    if (_finishObject[0] != null)
        //    {
        //        Gizmos.color = Color.green;
        //        Gizmos.DrawWireCube(_finishObject[0].transform.position + _iconOffset, new Vector3(0.4f, 0.6f, 0.4f));


        //        Gizmos.color = Color.yellow;
        //        Vector3 pos = _finishObject[0].transform.position + _firstGetBoniPosOffset;
        //        pos.y = 0f;
        //        Gizmos.DrawWireCube(pos, new Vector3(0.6f, 0.6f, 0.6f));
        //    }
        //}
        //else
        //{
        //    Gizmos.color = Color.green;
        //    Gizmos.DrawWireCube(transform.position + _iconOffset, new Vector3(0.4f, 0.6f, 0.4f));


        //    Gizmos.color = Color.yellow;
        //    Vector3 pos = transform.position + _firstGetBoniPosOffset;
        //    pos.y = 0f;
        //    Gizmos.DrawWireCube(transform.position + _firstGetBoniPosOffset, new Vector3(0.6f, 0.6f, 0.6f));

        //}
    }
}

