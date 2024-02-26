using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ObjectStatic : ObjectBase
{
    Material material = null;


    void Awake()
    {
        _transform = transform;
        if (_addRenderQueue != 0 && _transformModel != null)
        {
            _transformModel.GetComponent<MeshRenderer>().material.renderQueue += _addRenderQueue;
        }
    }
	// Use this for initialization
	void Start () {
        _transform = transform;
	}


    public override void SetAlpha(float in_alpha)
    {
        if (_transformModel != null)
        {
            if (material == null)
                material = _transformModel.GetComponent<MeshRenderer>().material;

            material.SetColor("_Color", new Color(1f, 1f, 1f, in_alpha));
        }

    }
}
