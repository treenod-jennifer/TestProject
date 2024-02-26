using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionCharacterChating : ActionBase
{

    public ChatGameData chatGameData = null;


    public void Awake()
    {
        bWaitActionFinishOnOff = true;
    }
    public override void DoAction()
    {
        //base.DoAction();

        //Debug.Log("ActionCharacterChating _ DoAction");

        //ManagerSound._instance.UnPauseBGM();
        ManagerUI._instance.OpenPopupChat(chatGameData, OnChatComplete);
    }
    public void OnChatComplete()
    {
        bActionFinish = true;
    }
}
