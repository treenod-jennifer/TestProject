using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionSetWorkPosition : ActionBase
{
    public Transform _pos;
    
    public void Awake()
    {
    }
    public override void DoAction()
    {
        LobbyAreaIcon._instance.SetWorkPosition(_pos.position, true);
        ManagerLobby._instance._workCameraPosition = _pos.position;
    }
    void OnDrawGizmosSelected()
    {
        if (_pos != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(_pos.position, 0.5f);
        }
    }
}
