using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;


public class ActionSendMessage : ActionBase
{
    public string _messageName = "";

    public override void DoAction()
    {
        ManagerLobby._instance.SendMessage(_messageName);
    }

}
