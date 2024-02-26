using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/UI/NGUI Renderer Widget")]
public class UIRendererWidget  : UIWidget
{
    protected Renderer m_Renderer;
    protected Material m_Mat;

    public override Material material
    {
        get => m_Mat;

        set
        {
            if (m_Mat != value)
            {
                RemoveFromPanel();
                m_Mat = value;
                MarkAsChanged();
            }
        }
    }

    protected override void OnEnable()
    {
        Init();
        base.OnEnable();
    }

    protected void Init()
    {
        if (null == m_Renderer)
        {
            m_Renderer = GetComponent<Renderer>();
        }

        if (null == m_Renderer)
        {
            return;
        }

        if (null == material)
        {
            if (false == Application.isPlaying)
            {
                material                  = m_Renderer.sharedMaterial;
                m_Renderer.sharedMaterial = material;
            }
            else
            {
                material            = m_Renderer.material;
                m_Renderer.material = material;
            }
        }
    }

    private void OnWillRenderObject()
    {
        if (null != drawCall && drawCall.finalRenderQueue != material.renderQueue)
        {
            material.renderQueue = drawCall.finalRenderQueue;
        }

        if (Application.isPlaying == true && m_Renderer.material != material)
        {
            m_Renderer.material = material;
        }
    }

    public override void OnFill(List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
    {
        for (var i = 0; i < 4; i++)
        {
            verts.Add(Vector3.zero);
            uvs.Add(Vector2.zero);
            cols.Add(color);
        }
    }
}