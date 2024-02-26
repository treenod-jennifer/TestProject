using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousingObject_Group: MonoBehaviour, IHousingTrigger 
{
    [SerializeField] public int _housingIndex = 0;
    [SerializeField] public HousingGroup _waitObject = null;
    [SerializeField] List<HousingGroup> _finishObject = new List<HousingGroup>();

    [SerializeField] public Vector3 _firstGetBoniPosOffset = Vector3.zero;
    [SerializeField] public TypeCharacterDir _firstGetBonidirection = TypeCharacterDir.Right;
    [SerializeField] public List<string> _firstGetBoniTextList = new List<string>();

    [SerializeField] public Dictionary<int, List<string>> _firstGetTextSpecial = new Dictionary<int, List<string>>();

    [SerializeField] public ObjectEvent _parentObj = null;

    [SerializeField] bool openPopupAtWakeup = true;

    [SerializeField] public Vector3 housingScale = Vector3.one;

    [System.NonSerialized] public int _modelIndex = 0; // 0이면 없는 상태,
    [System.NonSerialized] public HousingGroup currentObject = null;
    [System.NonSerialized] public Dictionary<int, HousingGroup> _objectList = new Dictionary<int, HousingGroup>();

    // Use this for initialization
    void Start()
    {
        if (ManagerHousing.FindHousing(this._housingIndex) != null)
        {
            return;
        }

        ManagerHousing.RegisterHousing(this);
        DoAction();
    }

    public void CloneFromLegacy(ActionObjectGroupHousing legacyObject)
    {
        _housingIndex = legacyObject._housingIndex;
        _waitObject = legacyObject._waitObject;
        _finishObject = new List<HousingGroup>(legacyObject._finishObject);

        _objectList = new Dictionary<int, HousingGroup>(legacyObject._objectList);

        _firstGetBoniPosOffset = legacyObject._firstGetBoniPosOffset;
        _firstGetBonidirection = legacyObject._firstGetBonidirection;
        _firstGetBoniTextList = new List<string>(legacyObject._firstGetBoniTextList);

        _firstGetTextSpecial = new Dictionary<int, List<string>>(legacyObject._firstGetTextSpecial);

        _parentObj = legacyObject._parentObj;
        this.housingScale = legacyObject.housingScale;

        openPopupAtWakeup = legacyObject.openPopupAtWakeup;

        _modelIndex = 0;
        currentObject = legacyObject.currentObject;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public int GetHousingIdx() { return _housingIndex; }

    public bool IsHousingActivated()
    {
        List<PlusHousingModelData> listProduction = new List<PlusHousingModelData>();
        List<PlusHousingModelData> listInstall = new List<PlusHousingModelData>();

        ManagerHousing.GetHousingProgress(listProduction, listInstall);
        return listInstall.Exists(x => x.housingIndex == this._housingIndex);
    }

    public void SelectModel(int modelIndex)
    {
        _modelIndex = modelIndex;
        RefreshModel(false);
    }

    public void MakeHousingChat(int modelIdx)
    {
        if (_firstGetTextSpecial.ContainsKey(modelIdx))
        {
            Character character = ManagerLobby._instance.GetCharacter(TypeCharacterType.Boni);
            character._ai.PlayAnimation(false, "haughty", WrapMode.Loop, 0f, 1f);

            var textList = _firstGetTextSpecial[modelIdx];

            int index = UnityEngine.Random.Range(0, textList.Count);
            var lobbyChat = UILobbyChat.MakeLobbyChat(character._transform, Global._instance.GetString(textList[index]), 2.5f);
            lobbyChat.heightOffset = character.GetBubbleHeightOffset();
        }
        else if (_firstGetBoniTextList.Count > 0)
        {
            Character character = ManagerLobby._instance.GetCharacter(TypeCharacterType.Boni);
            character._ai.PlayAnimation(false, "haughty", WrapMode.Loop, 0f, 1f);

            int index = UnityEngine.Random.Range(0, _firstGetBoniTextList.Count);
            var lobbyChat = UILobbyChat.MakeLobbyChat(character._transform, Global._instance.GetString(_firstGetBoniTextList[index]), 2.5f);
            lobbyChat.heightOffset = character.GetBubbleHeightOffset();
        }
    }

    public Vector3 GetHousingFocusPosition()
    {
        if (_finishObject.Count > 0)
        {
            var objBase = _finishObject[0].objectList[0].GetComponent<ObjectBase>();
            
            return objBase == null ? _finishObject[0].objectList[0].transform.position : objBase.GetFocusPosition();
        }
        else
            return transform.position;
    }

    public ObjectEvent GetCurrentMatchedObject(ObjectEvent org)
    {
        for (int i = 0; i < _waitObject.objectList.Count; ++i)
        {
            if (_waitObject.objectList[i].gameObject == org)
            {
                return currentObject.objectList[i].GetComponent<ObjectEvent>();
            }
        }
        return null;
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
    }
    public void MakeObject(int in_index, GameObject in_object)
    {
        try
        {
            if (_objectList.ContainsKey(in_index))
                return;

            var model = Instantiate<GameObject>(in_object).GetComponent<HousingGroup>();
            model.InitInstantiate();
            model.ApplyTransforms(_waitObject);
            if (_finishObject.Count == 0)
            {
                model.transform.parent = transform;
                model.transform.position = transform.position;
                model.transform.localScale = this.housingScale;
            }
            else
            {
                model.transform.parent = _finishObject[0].transform.parent;
                model.transform.position = _finishObject[0].transform.position;
                model.transform.localScale = this.housingScale;
            }
            

            model.HousingIdx = this._housingIndex;
            _finishObject.Add(model);
            _objectList.Add(in_index, model);

            // 추가적 데이터가 있으면 반영한다
            HousingData data = model.gameObject.GetComponent<HousingData>();
            if (data)
            {
                if (data._firstGetTextSpecial.Count > 0)
                    this._firstGetTextSpecial.Add(in_index, data._firstGetTextSpecial);
            }

            var currentParent = GetCurrentParentObj();
            if (currentParent != null && currentParent._transformModel)
            {
                if (model != null)
                {
                    model.transform.parent = currentParent._transformModel;
                    currentParent.childObjects.Add(model.gameObject);
                }
            }

            if (_finishObject.Count > 0 && _waitObject != null && currentObject == null)
            {
                _waitObject.HousingIdx = _housingIndex;
                currentObject = _waitObject;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
        }
    }
    public void DoAction()
    {
        _modelIndex = ManagerHousing.GetSelectedHousingModelIdx(_housingIndex);

        // 플러스 하우징 모델 읽고 추가,(나중에는 선택된것만 ??)
        if (_housingIndex > 0 && _modelIndex > 0)
        {
            if (_finishObject.Count > 0 && _waitObject != null && currentObject == null)
            {
                _waitObject.HousingIdx = this._housingIndex;
                currentObject = _waitObject;
            }

            PlusHousingModelData item = ManagerHousing.GetHousingModel(_housingIndex, _modelIndex);

            if (item.housingIndex == _housingIndex && item.active == 1)
            {
                HousingGroup model = null;

                //해당 하우징에 획득한 모델 일경우는 list에 오브젝트 비확성화 시켜서 추가
                if (item.type == PlusHousingModelDataType.byProgress || item.type == PlusHousingModelDataType.byPackage)
                {
                    if (model == null && _objectList.TryGetValue(_modelIndex, out model))
                    {
                    }
                    else
                    {
                        if (Global.LoadFromInternal)
                        {
#if UNITY_EDITOR
                            string path = "Assets/5_OutResource/housing/housing_" + _housingIndex + "_" + _modelIndex + "/" + _housingIndex + "_" + _modelIndex + ".prefab";
                            GameObject BundleObject = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                            model = Instantiate<GameObject>(BundleObject).GetComponent<HousingGroup>();
                            model.InitInstantiate();
#endif
                        }
                        else
                        {
                            string name = "h_" + _housingIndex + "_" + _modelIndex;
                            model = Instantiate<GameObject>(ManagerLobby._assetBankHousing[name]).GetComponent<HousingGroup>();
                            model.InitInstantiate();
                        }

                        if (model != null)
                        {
                            model.ApplyTransforms(_waitObject);
                            if (_finishObject.Count == 0)
                            {
                                model.transform.parent = transform;
                                model.transform.position = transform.position;
                                model.transform.localScale = this.housingScale;
                            }
                            else
                            {
                                model.transform.parent = _finishObject[0].transform.parent;
                                model.transform.position = _finishObject[0].transform.position;
                                model.transform.localScale = this.housingScale;
                            }

                            _finishObject.Add(model);
                            _objectList.Add(_modelIndex, model);
                        }
                    }
                }

                if (model != null)
                {

                    if (currentObject != model)
                    {
                        MigrateChildObjects(currentObject, model);
                    }

                    currentObject = model;

                    HousingData data = model.gameObject.GetComponent<HousingData>();
                    if (data)
                    {
                        if (data._firstGetTextSpecial.Count > 0)
                            this._firstGetTextSpecial.Add(_modelIndex, data._firstGetTextSpecial);
                    }
                }
            }

        }

        if(this.IsHousingActivated())
        {
            _modelIndex = ManagerHousing.GetSelectedHousingModelIdx(_housingIndex);

            // 하우징 미션을 완료는 했는데 아직 선택전이면 1번 모델 표시
            if (_modelIndex == 0)
            {
                _modelIndex = 1;
            }

            RefreshModel();
        }
        else
        {
            for (int i = 0; i < _finishObject.Count; i++)
            {
                _finishObject[i].SetActive(false);
                _finishObject[i].gameObject.SetActive(false);
            }

        }

        var currentParent = GetCurrentParentObj();

        if (currentParent != null && currentParent._transformModel)
        {
            for (int i = 0; i < _finishObject.Count; i++)
            {
                if (_finishObject[i] != null)
                {
                    _finishObject[i].transform.parent = currentParent._transformModel;
                    currentParent.childObjects.Add(_finishObject[i].gameObject);
                }
            }

            currentParent.OnPostChangedHousing();
        }

        for (int i = 0; i < _finishObject.Count; i++)
        {
            if (_finishObject[i] != null)
                _finishObject[i].HousingIdx = this._housingIndex;
        }
    }
    public void RefreshModel(bool in_skipScaleEffect = true)
    {
        for (int i = 0; i < _finishObject.Count; i++)
        {
            _finishObject[i].SetActive(false);
            _finishObject[i].gameObject.SetActive(false);
        }


        var prevObject = currentObject;

        if (_modelIndex > 0 && _objectList.ContainsKey(_modelIndex))
        {
            _objectList[_modelIndex].SetActive(true);
            _objectList[_modelIndex].gameObject.SetActive(true);

            currentObject = _objectList[_modelIndex];
            currentObject.HousingIdx = _housingIndex;

            if (_parentObj)
            {
                var parentObj = GetCurrentParentObj();
                if (parentObj)
                    currentObject.transform.parent = parentObj._transformModel;
            }

            if (prevObject != currentObject)
            {
                MigrateChildObjects(prevObject, currentObject);
            }

            if (!in_skipScaleEffect && gameObject.active)
            {
                var objGroup = _objectList[_modelIndex];
                for (int i = 0; i < objGroup.objectList.Count; ++i)
                {
                    ObjectEvent objEvent = objGroup.objectList[i].GetComponent<ObjectEvent>();
                    if (objEvent == null)
                        continue;

                    StartCoroutine(CoRefreshModel(objEvent));
                }
            }
        }

        if (_objectList.Count > 0 && _waitObject != null)
        {
            if (_finishObject.Count == 0)
            {
                _waitObject.SetActive(false);
                _waitObject.gameObject.SetActive(false);
            }
            else if (_waitObject != _finishObject[0])
            {
                _waitObject.SetActive(false);
                _waitObject.gameObject.SetActive(false);
            }

        }

        if (_parentObj)
        {
            var parentObj = GetCurrentParentObj();
            if (parentObj)
                parentObj.OnPostChangedHousing();
        }
    }

    void MigrateChildObjects(HousingGroup prevObjGroup, HousingGroup nextObjGroup)
    {
        if (prevObjGroup == null || nextObjGroup == null)
            return;

        if (prevObjGroup.objectList.Count != nextObjGroup.objectList.Count)
            return;

        for (int i = 0; i < prevObjGroup.objectList.Count; ++i)
        {
            if (prevObjGroup.objectList[i] == null || nextObjGroup.objectList[i] == null)
                continue;

            var prevObj = prevObjGroup.objectList[i].GetComponent<ObjectEvent>();
            var nextObj = nextObjGroup.objectList[i].GetComponent<ObjectEvent>();

            if (prevObj == null || nextObj == null)
                continue;

            for (int j = 0; j < prevObj.childObjects.Count; ++j)
            {
                prevObj.childObjects[i].transform.parent = nextObj._transformModel;
            }
            nextObj.childObjects = new List<GameObject>(prevObj.childObjects);
            prevObj.childObjects.Clear();
            nextObj._callbackTapList = new List<TriggerState>(prevObj._callbackTapList);
        }
    }

    ObjectEvent GetCurrentParentObj()
    {
        if (_parentObj == null)
        {
            return null;
        }

        if (_parentObj.housingIdx != 0)
        {
            var parentHousing = ManagerHousing.FindHousing(_parentObj.housingIdx);

            if (parentHousing == null)
                return null;

            return parentHousing.GetCurrentMatchedObject(_parentObj);
        }

        return _parentObj;
    }

    IEnumerator CoRefreshModel(ObjectEvent ev)
    {
        var in_transform = ev._transformModel;
        float timer = 0f;
        Vector3 positon = in_transform.localPosition;
        Vector3 scale = Vector3.one;// in_transform.localScale;
        while (timer <= 1f)
        {
            timer += Global.deltaTimeLobby * 5f;

            in_transform.localPosition = positon + Vector3.up * (0.8f - Mathf.Sin(Mathf.PI * 0.5f * timer) * 0.8f);

            float s = (Mathf.Sin(Mathf.PI * 3f * timer) / Mathf.Exp(timer)) * 0.2f;
            in_transform.localScale = new Vector3((1f - s) * scale.x, (1f + s) * scale.y, 1f);
            yield return null;
        }
        in_transform.localScale = scale;
        in_transform.localPosition = positon;
    }

    public void BuyNewHousing()
    {
        if (_firstGetBoniTextList.Count == 0)
            return;

        LobbyBehavior._instance.ResetSelectBehavior();
        LobbyBehavior._instance.CancleBehavior();
        Character character = ManagerLobby._instance.GetCharacter(TypeCharacterType.Boni);



        CharacterBoni._boni.StopPath();

        AIChangeCommand command = new AIChangeCommand();
        command._state = AIStateID.eIdle;
        character._ai.ChangeState(command);

        Vector3 pos = Vector3.zero;
        if (_finishObject.Count > 0)
            pos = _finishObject[0].transform.position + _firstGetBoniPosOffset;
        else
            pos = transform.position + _firstGetBoniPosOffset;

        pos.y = 0f;
        character._transform.position = pos;
        character._ai.BodyTurn(_firstGetBonidirection);
        character._ai.PlayAnimation(false, "haughty", WrapMode.Loop, 0f, 1f);

        Debug.Log("BuyNewHousing");
    }

    void OnDrawGizmosSelected()
    {
        if (_finishObject.Count > 0)
        {
            if (_finishObject[0] != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 pos = _finishObject[0].transform.position + _firstGetBoniPosOffset;
                pos.y = 0f;
                Gizmos.DrawWireCube(pos, new Vector3(0.6f, 0.6f, 0.6f));
            }
        }
        else
        {
            Gizmos.color = Color.yellow;
            Vector3 pos = transform.position + _firstGetBoniPosOffset;
            pos.y = 0f;
            Gizmos.DrawWireCube(transform.position + _firstGetBoniPosOffset, new Vector3(0.6f, 0.6f, 0.6f));

        }
    }

    bool IHousingTrigger.IsMovableHousing()
    {
        return false;
    }
}
