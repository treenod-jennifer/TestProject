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
    //public List<ObjectBase> _finishObject = new List<ObjectBase>();

    public Vector3 _iconOffset = Vector3.zero;
    [System.NonSerialized]
    public int _modelIndex = 0; // 0이면 없는 상태,

    [System.NonSerialized]
    public bool _haveIndex = true;


    public Vector3 _firstGetBoniPosOffset = Vector3.zero;
    public TypeCharacterDir _firstGetBonidirection = TypeCharacterDir.Right;
    public List<string> _firstGetBoniTextList = new List<string>();

    public Dictionary<int, List<string>> _firstGetTextSpecial = new Dictionary<int, List<string>>();

    public ObjectEvent _parentObj = null;

    public void Awake()
    {
        for (int i = 0; i < _finishObject.Count; i++)
            _objectList.Add(i + 1, _finishObject[i]);
    }
    public void MakeObject(int in_index, GameObject in_object)
    {
        try {
            if (_objectList.ContainsKey(in_index))
                return;

            ObjectBase model = Instantiate<GameObject>(in_object).GetComponent<ObjectBase>();
            if (_finishObject.Count == 0) {
                model.transform.parent = transform;
                model.transform.position = transform.position;
            }
            else {
                model.transform.parent = _finishObject[0].transform.parent;
                model.transform.position = _finishObject[0].transform.position;
            }

            model._actionHousing = this;
            _finishObject.Add(model);
            _objectList.Add(in_index, model);

            // 추가적 데이터가 있으면 반영한다
            HousingData data = model.gameObject.GetComponent<HousingData>();
            if ( data )
            {
                if (data._firstGetTextSpecial.Count > 0)
                    this._firstGetTextSpecial.Add(in_index, data._firstGetTextSpecial);
            }
                

            if (_parentObj != null && _parentObj._transformModel)
            {
                if (model != null)
                {
                    model.transform.parent = _parentObj._transformModel;
                }
            }
        }
        catch (System.Exception e) {
            Debug.LogException(e);
        }
    }
    public override void DoAction()
    {
        //base.DoAction();
        ManagerLobby._instance._objectHousing[_housingIndex - 1] = this;

        if (_stateType == TypeTriggerState.Wait || _stateType == TypeTriggerState.Active || _stateType == TypeTriggerState.Finish)
        {
          //  if (_housingIndex <= ManagerData._instance._housingSelectData.Count)
            //    _modelIndex = ManagerData._instance._housingSelectData[_housingIndex - 1].selectModel;
            for (int i = 0; i < ManagerData._instance._housingSelectData.Count; i++)
            {
                if (ManagerData._instance._housingSelectData[i].index == _housingIndex)
                {
                    _modelIndex = ManagerData._instance._housingSelectData[i].selectModel;
                    break;
                }
            }
        }


       // Debug.Log("하우징 " + _housingIndex + "  " + _modelIndex + "   " + _stateType + "   " + ManagerData._instance._housingSelectData.Count);

        // 플러스 하우징 모델 읽고 추가,(나중에는 선택된것만 ??)
        if (_housingIndex > 0 && _modelIndex > 0)
        {

            PlusHousingModelData item = ManagerData._instance._housingGameData[_housingIndex + "_" + _modelIndex];
        //    foreach (var item in ManagerData._instance._housingGameData)
            {

               // Debug.Log("하우징_" + item.housingIndex + "  " + _modelIndex + "  " + item.type + "  " + item.active);
                if (item.housingIndex == _housingIndex)
                {
                    //해당 하우징에 획득한 모델 일경우는 list에 오브젝트 비확성화 시켜서 추가
                    if (item.type == PlusHousingModelDataType.byProgress || item.type == PlusHousingModelDataType.byEvent)
                    {
                        if (item.active == 1)
                        {
                            if (!Global._instance.ForceLoadBundle && (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer))
                            {
#if  UNITY_EDITOR
                                string path = "Assets/5_OutResource/housing/housing_" + _housingIndex + "_" + _modelIndex + "/" + _housingIndex + "_" + _modelIndex + ".prefab";
                                GameObject BundleObject = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(path);
                                ObjectBase model = Instantiate<GameObject>(BundleObject).GetComponent<ObjectBase>();
                                if (_finishObject.Count == 0)
                                {
                                    model.transform.parent = transform;
                                    model.transform.position = transform.position;
                                }
                                else
                                {
                                    model.transform.parent = _finishObject[0].transform.parent;
                                    model.transform.position = _finishObject[0].transform.position;
                                }
                                _finishObject.Add(model);
                                _objectList.Add(_modelIndex, model);

                                HousingData data = model.gameObject.GetComponent<HousingData>();
                                if (data)
                                {
                                    if (data._firstGetTextSpecial.Count > 0)
                                        this._firstGetTextSpecial.Add(_modelIndex, data._firstGetTextSpecial);
                                }
#endif
                            }
                            else
                            {
                                string name = "h_" + _housingIndex + "_" + _modelIndex;
                               
                            //    Debug.Log("하우징1= " + name );

                            //    AssetBundle assetBundle = ManagerLobby._assetBankHousing[name]; //AssetBundle.LoadFromFile(localPath);
                            //    GameObject Obj = assetBundle.LoadAsset<GameObject>(_housingIndex + "_" + _modelIndex);
                               // Debug.Log("하우징2= " + Obj);

                                ObjectBase model = Instantiate<GameObject>(ManagerLobby._assetBankHousing[name]).GetComponent<ObjectBase>();
                                if (_finishObject.Count == 0)
                                {
                                    model.transform.parent = transform;
                                    model.transform.position = transform.position;
                                }
                                else
                                {
                                    model.transform.parent = _finishObject[0].transform.parent;
                                    model.transform.position = _finishObject[0].transform.position;
                                }
                              //  Debug.Log("하우징3= " + model);

                                _finishObject.Add(model);
                                _objectList.Add(_modelIndex, model);

                                HousingData data = model.gameObject.GetComponent<HousingData>();
                                if (data)
                                {
                                    if (data._firstGetTextSpecial.Count > 0)
                                        this._firstGetTextSpecial.Add(_modelIndex, data._firstGetTextSpecial);
                                }
                            }
                        }
                    }
                }
            }
        }

        if (_stateType == TypeTriggerState.Wait || _stateType == TypeTriggerState.Active)
        {
            for (int i = 0; i < _finishObject.Count; i++)
                _finishObject[i].gameObject.SetActive(false);
        }
        else if (_stateType == TypeTriggerState.Finish)
        {
            for (int i = 0; i < ManagerData._instance._housingSelectData.Count; i++)
            {
                if (ManagerData._instance._housingSelectData[i].index == _housingIndex)
                {
                    _modelIndex = ManagerData._instance._housingSelectData[i].selectModel;
                    break;
                }
            }
        //    if (_housingIndex <= ManagerData._instance._housingSelectData.Count)
           //     _modelIndex = ManagerData._instance._housingSelectData[_housingIndex - 1].selectModel;
            // 하우징 미션을 완료는 했는데 아직 선택전이면 1번 모델 표시
            if (_modelIndex == 0)
            {
                _haveIndex = false;
                _modelIndex = 1;
            }


            RefreshModel();
        }
        else if (_stateType == TypeTriggerState.WakeUp)
        {
            _modelIndex = 1;    // 첫번쨰 모델로 설정되어 고르는 UI 출력
            RefreshModel();
            //UIHousing housing = NGUITools.AddChild(ManagerUI._instance.anchorCenter, ManagerUI._instance._objPopupHousing).GetComponent<UIHousing>();
            //housing.InitHousing(this, OnChatComplete);
            ManagerUI._instance.OpenPopupHousing(this, -1, false, OnChatComplete);
            bWaitActionFinishOnOff = true;
            // UI콜(this,OnChatComplete);
        }

        if (_parentObj != null && _parentObj._transformModel)
        {
            for (int i = 0; i < _finishObject.Count; i++)
            {
                if (_finishObject[i] != null)
                {
                    _finishObject[i].transform.parent = _parentObj._transformModel;
                }
            }

            _parentObj.OnPostChangedHousing();
        }

        for (int i = 0; i < _finishObject.Count; i++)
        {
            if (_finishObject[i] != null)
                _finishObject[i]._actionHousing = this;
        }
    }
    public void RefreshModel(bool in_skipScaleEffect = true)
    {

        //Debug.Log("하우징 RefreshModel " + _modelIndex + " _objectList  " + _objectList.Count);

        for (int i = 0; i < _finishObject.Count; i++)
            _finishObject[i].gameObject.SetActive(false);


      //  Debug.Log("하우징2 " + _housingIndex + "    " + _modelIndex + "          " + _objectList.Count);

        if (_modelIndex > 0)
        {
            //Debug.Log("하우징 " + _housingIndex + "    " + _modelIndex + "          " + _objectList.Count);

            if (_objectList.ContainsKey(_modelIndex))
            {
                _objectList[_modelIndex].gameObject.SetActive(true);
                if (!in_skipScaleEffect && gameObject.active)
                    StartCoroutine(CoRefreshModel(_objectList[_modelIndex]._transformModel));
            }
          //  Debug.Log("하우징 3");
        }


        if (_objectList.Count > 0)
            if (_waitObject != null)
            {
                if (_finishObject.Count==0)
                    _waitObject.gameObject.SetActive(false);
                else if (_waitObject != _finishObject[0])
                    _waitObject.gameObject.SetActive(false);
            }

        if(_parentObj)
        {
            _parentObj.OnPostChangedHousing();
        }

     //   Debug.Log("하우징 4");
    /*    for (int i = 0; i < _finishObject.Count; i++)
        {
            if (i == (_modelIndex - 1))
            {
                _finishObject[i].gameObject.SetActive(true);
                if (!in_skipScaleEffect)
                    StartCoroutine(CoRefreshModel(_finishObject[i]._transformModel));
                continue;
            }
            _finishObject[i].gameObject.SetActive(false);
        }

        if (_waitObject != null)
            _waitObject.gameObject.SetActive(false);*/
    }
    IEnumerator CoRefreshModel(Transform in_transform)
    {
        float timer = 0f;
        Vector3 positon = in_transform.localPosition;
        Vector3 scale = Vector3.one;// in_transform.localScale;
        while (timer<=1f)
        {
            timer += Global.deltaTimeLobby * 5f;

            in_transform.localPosition = positon + Vector3.up * (0.8f - Mathf.Sin(Mathf.PI * 0.5f * timer) * 0.8f);

            float s = (Mathf.Sin(Mathf.PI * 3f * timer)/Mathf.Exp(timer)) * 0.2f;
            in_transform.localScale = new Vector3((1f - s) * scale.x, (1f + s) * scale.y, 1f);
            yield return null;
        }
        in_transform.localScale = scale;
        in_transform.localPosition = positon;
    }
    public void OnChatComplete()
    {


        bActionFinish = true;
    }

    public void BuyNewHousing()
    {
        LobbyBehavior._instance.ResetSelectBehavior();
        LobbyBehavior._instance.CancleBehavior();
        Character character = ManagerLobby._instance.GetCharacter(TypeCharacterType.Boni);



        CharacterBoni._boni.StopPath();

        AIChangeCommand command = new AIChangeCommand();
        command._state = AIStateID.eIdle;
        character._ai.ChangeState(command);

        Vector3 pos = Vector3.zero;
        if (_finishObject.Count>0)
            pos = _finishObject[0].transform.position + _firstGetBoniPosOffset;
        else
            pos = transform.position + _firstGetBoniPosOffset;

        pos.y = 0f;
        character._transform.position = pos;
        character._ai.BodyTurn(_firstGetBonidirection);
        character._ai.PlayAnimation(false, "question", WrapMode.Loop, 0f, 1f);

        Debug.Log("BuyNewHousing");
    }
    public string GetString(string in_key)
    {
        Dictionary<string, string> _stringBank = null;
        if (_stateType == TypeTriggerState.BehaviorGlobal)
            _stringBank = Global._instance._stringData;
        else
        {
            if (transform.parent.parent.parent.GetComponent<AreaBase>() != null)
                _stringBank = transform.parent.parent.parent.GetComponent<AreaBase>()._stringData;
        }

        if (!_stringBank.ContainsKey(in_key))
            return in_key + ": string empty";
        else
            return _stringBank[in_key];
    }


    void OnDrawGizmosSelected()
    {
        if (_finishObject.Count > 0)
        {
            if (_finishObject[0] != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(_finishObject[0].transform.position + _iconOffset, new Vector3(0.4f, 0.6f, 0.4f));


                Gizmos.color = Color.yellow;
                Vector3 pos = _finishObject[0].transform.position + _firstGetBoniPosOffset;
                pos.y = 0f;
                Gizmos.DrawWireCube(pos, new Vector3(0.6f, 0.6f, 0.6f));
            }
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + _iconOffset, new Vector3(0.4f, 0.6f, 0.4f));


            Gizmos.color = Color.yellow;
            Vector3 pos = transform.position + _firstGetBoniPosOffset;
            pos.y = 0f;
            Gizmos.DrawWireCube(transform.position + _firstGetBoniPosOffset, new Vector3(0.6f, 0.6f, 0.6f));

        }
    }
}
