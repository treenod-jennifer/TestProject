using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBoni : Character
{
    

    void Awake()
    {
        base.Awake();
        //_boni = this;
        _ai.AIStart(this, _animation);
    }
	// Use this for initialization
	void Start () {
        _type = TypeCharacterType.Boni;
        _runDust.Stop();

        ManualyStart();
	}

    public override void ManualyStart()
    {
        
        ChangeState(AIStateID.eIdle);
    }

   // Collider[] colliderList;
    //public List<Collider> colliderBank = new List<Collider>();
	// Update is called once per frame
	void Update () {

        _ai.AIUpdate();
        MoveUpdate();

        ColliderUpdate(_transform.position);

        // 컬라이드에 따른 메세지 보내기
     /*   {
            colliderList = Physics.OverlapSphere(_transform.position + new Vector3(-0.4f, 0f, 0.4f), 1f, Global.eventObjectMask);
            if (colliderList.Length > 0)
            {
                for (int j = 0; j < colliderList.Length; j++)
                {
                    bool newColl = true;
                    for (int i = 0; i < colliderBank.Count; i++)
                    {
                        if (colliderBank[i] == colliderList[j])
                        {
                            newColl = false;
                            break;
                        }
                    }
                    //send
                    if (newColl)
                    {
                        if (colliderList[j].transform.parent != null)
                        {
                            //Debug.Log("CharacterBoni " + colliderList[j].name);
                            ObjectEvent eventObj = colliderList[j].transform.parent.GetComponent<ObjectEvent>();
                            if (eventObj != null)
                                eventObj.OnEnterCharacter(this);
                        }


                        colliderBank.Add(colliderList[j]);
                    }
                }
            }
            else
                colliderBank.Clear();
        }*/
        

        //if (Physics.OverlapSphereNonAlloc(_transform.position + new Vector3(-0.4f, 0f, 0.4f), 1f, colliderList)>0)
        
	}
}
