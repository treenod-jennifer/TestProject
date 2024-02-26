using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialCapsuleGacha : TutorialBase
{
    public GameObject GetObjCapsuleGacha_OutGameObject()
    {
        return CapsuleGachaAreaBase._instance._touchTarget.gameObject;
    }

    public List<GameObject> GetListObjCapsuleGacha_OutGameObject()
    {
        List<GameObject> listObj = new List<GameObject>();

        listObj.Add(CapsuleGachaAreaBase._instance._touchTarget.gameObject);

        return listObj;
    }

    public void CameraMove()
    {
        StartCoroutine(CoCameraMove());
    }

    private IEnumerator CoCameraMove()
    {
        Transform capsuleGacha = CapsuleGachaAreaBase._instance._touchTarget.transform;
        CameraController._instance.MoveToPosition(capsuleGacha.position, 0.25f);
        yield return new WaitForSeconds(0.3f);
    }

    public bool IsOpenPopupCapsuleGacha()
    {
        if(UIPopupCapsuleGacha._instance != null)
        {
            return true;
        }
        return false;
    }

    public GameObject GetObjFreeGachaButton()
    {
        return UIPopupCapsuleGacha._instance.objFreeGacha;
    }

    public List<GameObject> GetListObjFreeGachaButton()
    {
        List<GameObject> listObject = new List<GameObject>();

        listObject.Add(UIPopupCapsuleGacha._instance.objFreeGacha);

        return listObject;
    }

    public void OpenPopupCapsuleGacha()
    {
        ManagerUI._instance.OpenPopup<UIPopupCapsuleGacha>((popup) => popup._callbackOpen
        = () => ManagerCapsuleGachaEvent.IsTutorialClear = true);
    }

    public List<GameObject> GetCenterAnchor()
    {
        List<GameObject> listObject = new List<GameObject>();

        listObject.Add(ManagerUI._instance.anchorCenter);

        return listObject;
    }

    public void OnClickFreeGachaBtn()
    {
        UIPopupCapsuleGacha._instance.CapsuleGachaSinglePlay();
    }
}