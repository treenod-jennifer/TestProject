using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIItemCollaborationTag))]
public class UIItemCollaborationTagEditor : Editor
{
    private void OnEnable()
    {
        var tagObject = target as UIItemCollaborationTag;
        tagObject.MakeTag(true);
    }
}
