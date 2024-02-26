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
    }

#if UNITY_EDITOR
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Set Model"))
            {
                Transform model = FindOrMakeModel();

                if (_target._transformModel == null)
                    _target._transformModel = model;

                SetRotation(model);
            }

            if (GUILayout.Button("Set Collider"))
            {
                Transform collider = FindOrMakeCollider();

                if (_target._transformCollider == null)
                    _target._transformCollider = collider;

                SetRotation(collider);
            }
        }
        GUILayout.EndHorizontal();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(_target.gameObject.scene);

            serializedObject.ApplyModifiedProperties();
        }
    }

    private Transform FindOrMakeModel()
    {
        Transform model = _target.transform.Find("rootModel");

        if (model == null)
        {
            model = new GameObject("rootModel").transform;
            model.parent = _target.transform;
            model.localPosition = Vector3.zero;
        }

        return model;
    }

    private Transform FindOrMakeCollider()
    {
        Transform model = _target.transform.Find("rootCollider");

        if (model == null)
        {
            model = new GameObject("rootCollider").transform;
            model.parent = _target.transform;
            model.localPosition = Vector3.zero;
            model.gameObject.layer = LayerMask.NameToLayer("EventObject");
        }

        return model;
    }

    private void SetRotation(Transform targetObject)
    {
        targetObject.rotation = Global.billboardRotation;
    }

#endif  
}
