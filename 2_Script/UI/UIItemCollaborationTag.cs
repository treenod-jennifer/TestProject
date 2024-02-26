using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIItemCollaborationTag : MonoBehaviour
{
    private GameObject tagObject = null;

    private void Awake()
    {
        MakeTag();        
    }

    public void MakeTag(bool isTestMode = false)
    {
        if(tagObject != null)
        {
            return;
        }

        var tagTemplate = Resources.Load("UIPrefab/CollaborationTag");
        tagObject = Instantiate(tagTemplate, transform) as GameObject;

        if (isTestMode)
        {
            tagObject.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}
