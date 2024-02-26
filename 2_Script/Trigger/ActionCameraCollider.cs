using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionCameraCollider : ActionBase
{

    public ActionCameraColliderRootHelp _colliderRoot = null;

    public override void DoAction()
    {
        /*if (_stateType == TypeTriggerState.WakeUp)
        {
            for (int i = 0; i < _colliderBank.Count - 1; i++)
                _colliderBank[i].SetActive(false);
            _colliderRoot.SetActive(true);
        }
        else
        {
            for (int i = 0; i < _colliderBank.Count - 1; i++)
                _colliderBank[i].SetActive(false);
            _colliderRoot.SetActive(true);
        }*/
        for (int i = 0; i < _colliderBank.Count; i++)
            _colliderBank[i].gameObject.SetActive(false);
        _colliderRoot.gameObject.SetActive(true);


        //if(_stateType == TypeTriggerState)

      //  Debug.Log("ActionCameraCollider_________________________" + _colliderBank.Count);


        //Debug.Log("ActionCameraCollider DoAction " + gameObject.transform.parent.name);
    }



    static List<ActionCameraColliderRootHelp> _colliderBank = new List<ActionCameraColliderRootHelp>();
    static public void AddCameraCollider(ActionCameraColliderRootHelp in_obj)
    {
        //Debug.Log("ActionCameraCollider AddCameraCollider " + in_obj.transform.parent.name);

        _colliderBank.Add(in_obj);
    }
    static public void LastColliderActive()
    {
        if (_colliderBank.Count > 2)
        {
            for (int i = 0; i < _colliderBank.Count - 1; i++)
                _colliderBank[i].gameObject.SetActive(false);
            _colliderBank[_colliderBank.Count - 1].gameObject.SetActive(true);
        }
    }
    static public void ResetCameraCollider()
    {
     /*   for (int i = 0; i < _colliderBank.Count - 1; i++)
            _colliderBank[i].SetActive(false);
        if (_colliderBank.Count > 0)
        {
            _colliderBank[_colliderBank.Count - 1].SetActive(true);
        }
        */
        _colliderBank.Clear();
    }
}
