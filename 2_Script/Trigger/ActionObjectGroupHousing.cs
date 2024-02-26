using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionObjectGroupHousing : ActionBase
{

    [SerializeField] public int _housingIndex = 0;
    [SerializeField] public HousingGroup _waitObject = null;
    [SerializeField] public List<HousingGroup> _finishObject = new List<HousingGroup>() ;

    [SerializeField] public Vector3 _firstGetBoniPosOffset = Vector3.zero;
    [SerializeField] public TypeCharacterDir _firstGetBonidirection = TypeCharacterDir.Right;
    [SerializeField] public List<string> _firstGetBoniTextList = new List<string>();

    [SerializeField] public Dictionary<int, List<string>> _firstGetTextSpecial = new Dictionary<int, List<string>>();

    [SerializeField] public ObjectEvent _parentObj = null;

    [SerializeField] public bool openPopupAtWakeup = true;

    [SerializeField] public Vector3 housingScale = Vector3.one;

    [System.NonSerialized] public int _modelIndex = 0; // 0이면 없는 상태,
    [System.NonSerialized] public HousingGroup currentObject = null;
    [System.NonSerialized] public Dictionary<int, HousingGroup> _objectList = new Dictionary<int, HousingGroup>();

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public int GetHousingIdx() { return _housingIndex; }

    public bool IsHousingActivated()
    {
        if (_stateType == TypeTriggerState.Wait || _stateType == TypeTriggerState.Active)
            return false;
        else if (_stateType == TypeTriggerState.Finish || _stateType == TypeTriggerState.WakeUp)
            return true;
        return true;
    }

    public void Awake()
    {
        for (int i = 0; i < _finishObject.Count; i++)
        {
            _finishObject[i].InitInstantiate();
            _finishObject[i].ApplyTransforms(_waitObject);
            _objectList.Add(i + 1, _finishObject[i]);
        }
            

        if (_finishObject.Count > 0 && _waitObject != null)
        {
            _waitObject.HousingIdx = _housingIndex;
            currentObject = _waitObject;
        }
        var area = ScanNearestAreaBase(this.transform);
        if (area != null)
        {
            var housingRoot = area.transform.Find("GroupHousing");
            if (housingRoot == null)
            {
                var hrgo = new GameObject("GroupHousing");
                hrgo.transform.parent = area.transform;
                housingRoot = hrgo.transform;
            }
            var copiedObjectRoot = new GameObject($"Deco_G_Obj_{_housingIndex}_({_stateType.ToString()})");
            var comp = copiedObjectRoot.AddComponent<HousingObject_Group>();
            comp.CloneFromLegacy(this);
            copiedObjectRoot.transform.parent = housingRoot;
            copiedObjectRoot.transform.position = this.transform.position;
        }
    }
   
    public override void DoAction()
    {
        if (_stateType == TypeTriggerState.WakeUp)
        {
            var housing = ManagerHousing.FindHousing(_housingIndex);
            housing.SelectModel(1);

            if ( this.openPopupAtWakeup )
            {
                if (ManagerCinemaBox._instance != null)
                    ManagerCinemaBox._instance.SkipEmergencyStop();

                ManagerUI._instance.OpenPopupHousing(this._housingIndex, -1, false, currentObject.objectList[0].transform.position, OnChatComplete);
                bWaitActionFinishOnOff = true;
            }
        }
    }
    public void OnChatComplete()
    {


        bActionFinish = true;
    }    
}
