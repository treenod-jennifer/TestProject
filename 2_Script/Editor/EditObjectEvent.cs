using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
/*
[CanEditMultipleObjects]
[CustomEditor(typeof(ObjectMaterial), true)]
public class EditObjectMaterial : EditObjectBase
{
    protected void OnEnable()
    {
        base.OnEnable();

        if (_target._transformCollider == null)
        {
            for (int i = 0; i < _target.transform.childCount; i++)
            {
                if (_target.transform.GetChild(i).gameObject.name == "rootCollider")
                {
                    _target._transformCollider = _target.transform.GetChild(i);
                    break;
                }
            }
            if (_target._transformCollider == null)
            {
                GameObject model = new GameObject("rootCollider");
                _target._transformCollider = model.transform;
                _target._transformCollider.parent = _target.transform;
                _target._transformCollider.localPosition = Vector3.zero;

                model.layer = LayerMask.NameToLayer("EventObject");
            }
        }
    }
}
*/