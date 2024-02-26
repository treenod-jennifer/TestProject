using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Area Prefab 에서 포코고로 나무에 해당하는 ObjectEvent 에다 이 스크립트를 붙이면
// Area가 로딩될 때, 로비에다가 고로 나무를 등록시킨다
// 그리고 이후 로비에서 포코유라가 추가될 때 나무의 하위에다가 유라를 붙이게 됨

public class Pokogoro : MonoBehaviour {
    [SerializeField]
    private Transform rootModel = null;

	// Use this for initialization
	void Start () {
        ObjectEvent objEvent = gameObject.GetComponent<ObjectEvent>();
        rootModel = objEvent._transformModel;
        objEvent._onTouch = OnTapPokogoro;

        ManagerLobby._instance._objPokogoro = this;
    }

    private void Awake()
    {
        ObjectEvent objEvent = gameObject.GetComponent<ObjectEvent>();
        objEvent.OnCompleteMakeObject += RemakePokoyura;
    }

    void OnDestroy()
    {
        if( ManagerLobby._instance._objPokogoro == this )
            ManagerLobby._instance._objPokogoro = null;
    }

    private void OnEnable()
    {
        if (ManagerLobby._instance._objPokogoro == this)
            return;

        ManagerLobby._instance._objPokogoro = this;
        for (int i = 0; i < Pokoyura._pokoyuraList.Count; i++)
        {
            AttachPokoyura(Pokoyura._pokoyuraList[i]);
        }
    }

    private void RemakePokoyura()
    {
        ManagerLobby._instance.ReMakePokoyura();
    }

    public void AttachPokoyura(Pokoyura obj)
    {
        obj.transform.parent = rootModel;
    }

    public void OnTapPokogoro()
    {
        ManagerUI._instance.OpenPopupPokoyuraSelector(true);
    }
}
