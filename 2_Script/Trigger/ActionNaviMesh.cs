using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionNaviMesh : ActionBase
{

    public TextAsset _naviData = null;

    public override void DoAction()
    {
        if (_stateType == TypeTriggerState.WakeUp)
        {
            AstarPath.active.data.DeserializeGraphs(_naviData.bytes);
            ObjectBase.EditNaviCollider();
        }
        else
        {
            //Debug.Log("ActionNaviMesh 등록_________________________" + _naviBank.Count);
            _naviBank.Add(_naviData);
        }

      //  Debug.Log("ActionCameraCollider_________________________" + _colliderBank.Count);

    }
    static List<TextAsset> _naviBank = new List<TextAsset>();
    static public void LoadLastNaviMesh()
    {
        if (_naviBank.Count > 0)
        {
            AstarPath.active.data.DeserializeGraphs(_naviBank[_naviBank.Count - 1].bytes);
            //Debug.Log("ActionNaviMesh 읽기_________________________ " + _naviBank.Count);
        }

        _naviBank.Clear();
    }
}
