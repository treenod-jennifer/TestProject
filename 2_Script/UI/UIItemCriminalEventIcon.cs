using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemCriminalEventIcon : MonoBehaviour
{
    [SerializeField] private GameObject objCoco;
    [SerializeField] private GameObject objComplete;
    [SerializeField] private Animation anim;
    [SerializeField] private GameObject objNewIcon;

    //번들 처리 sprite
    [SerializeField] private List<UISprite> sprCriminalList;

    private void OnDestroy()
    {
        for (var i = 0; i < sprCriminalList.Count; i++)
        {
            sprCriminalList[i].atlas = null;
        }
    }

    private IEnumerator Start()
    {
        yield return StartCoroutine(ManagerCriminalEvent.instance.LoadCriminalEventResource());
        
        //번들 Atlas 세팅
        for (int i = 0; i < sprCriminalList.Count; i++)
        {
            sprCriminalList[i].atlas =
                ManagerCriminalEvent.instance.criminalEventPack.AtlasUI;
        }
    }

    public void InitData()
    {
        objComplete.SetActive(ManagerCriminalEvent.instance.IsClearStage());
        if (ManagerCriminalEvent.instance.IsClearStage() == false)
            objCoco.SetActive(true);
    }

    public void StartDirection()
    {
        if (ManagerCriminalEvent.instance.IsClearStage() == false)
        {
            objNewIcon.SetActive(ManagerCriminalEvent.instance.IsGetReward());
            return;
        }

        StartCoroutine(PlayClear());
    }

    private IEnumerator PlayClear()
    {
        anim.Stop();
        anim.Play("CriminalIngameComplete");

        yield return new WaitWhile(() => anim.isPlaying);

        objNewIcon.SetActive(ManagerCriminalEvent.instance.IsGetReward());
    }

    private void OnClickCriminalEventOpen()
    {
        if (ManagerCriminalEvent.CheckStartable())
        {
            ManagerUI._instance.OpenPopup<UIPopUpCriminalEvent>((popup) =>
            {
                popup.InitPopup(true);
                popup._callbackClose += () => { objNewIcon.SetActive(ManagerCriminalEvent.instance.IsGetReward()); };
            });
        }
        else
        {
            ManagerUI._instance.OpenPopup<UIPopupSystem>((popup) =>
            {
                popup.SortOrderSetting();
                popup.InitSystemPopUp(Global._instance.GetString("p_t_4"), Global._instance.GetString("n_ev_13"), false,
                    null);
            });
        }
    }
}