using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



[CanEditMultipleObjects]
[CustomEditor(typeof(ObjectBase), true)]
public class EditObjectBase : Editor
{
    protected ObjectBase _target;

    protected void OnEnable()
    {
        _target = target as ObjectBase;


        if (_target._transformModel == null)
        {
            for (int i = 0; i < _target.transform.childCount; i++)
			{
			    if(_target.transform.GetChild(i).gameObject.name == "rootModel")
                {
                    _target._transformModel = _target.transform.GetChild(i);
                    break;
                }
			}
            if (_target._transformModel == null)
            {
                GameObject model = new GameObject("rootModel");
                _target._transformModel = model.transform;
                _target._transformModel.parent = _target.transform;
                _target._transformModel.localPosition = Vector3.zero;
            }
        }
        
    }

#if UNITY_EDITOR
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Apply model billboard"))
            {
                if (_target._transformModel != null)
                    _target._transformModel.rotation = Global.billboardRotation;//Quaternion.Euler(50f, -45f, 0f);
            }

            if (GUILayout.Button("Apply collider billboard"))
            {
                if (_target._transformCollider != null)
                    _target._transformCollider.rotation = Global.billboardRotation; //Quaternion.Euler(50f, -45f, 0f);
            }
        }
        GUILayout.EndHorizontal();

    }
#endif  
}
